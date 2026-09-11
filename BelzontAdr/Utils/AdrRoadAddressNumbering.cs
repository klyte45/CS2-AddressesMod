using Colossal.Entities;
using Colossal.Mathematics;
using Game.Net;
using Game.Objects;
using Game.SceneFlow;
using System.Globalization;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BelzontAdr
{
    /// <summary>
    /// Shared ADR house-number math (same family as <see cref="BuildingUtilsOverrides.GetAddress_Override"/>),
    /// without building-side even/odd adjustment — odd numbers at curve positions.
    /// </summary>
    public static class AdrRoadAddressNumbering
    {
        public const int DenseSegmentCardThreshold = 10;
        public const float HalfKilometerMeters = 500f;
        public const float HalfMileMeters = 804.672f; // 0.5 * 1609.344

        public static bool TryResolveInverseAggregate(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            AdrHighwayRoutesSystem highwayRoutesSystem,
            Entity aggregate,
            DynamicBuffer<AggregateElement> aggregationBuffer,
            out bool isInverseAggregate)
        {
            isInverseAggregate = false;
            if (aggregationBuffer.Length == 0
                || !entityManager.TryGetComponent(aggregationBuffer[0].m_Edge, out Curve e0)
                || !entityManager.TryGetComponent(aggregationBuffer[^1].m_Edge, out Curve e1))
            {
                return false;
            }

            var refE0 = CheckSegmentReversion(entityManager, aggregationBuffer, false, 0, aggregationBuffer[0])
                ? e0.m_Bezier.d.xz
                : e0.m_Bezier.a.xz;
            var refE1 = CheckSegmentReversion(entityManager, aggregationBuffer, false, aggregationBuffer.Length - 1, aggregationBuffer[^1])
                ? e1.m_Bezier.a.xz
                : e1.m_Bezier.d.xz;

            var zeroMarker = entityManager.TryGetComponent(aggregate, out ADRHighwayAggregationData hwDataInfo)
                && hwDataInfo.highwayDataId != default
                && highwayRoutesSystem.TryGetHighwayData(hwDataInfo.highwayDataId, out var hwData)
                    ? hwData.refStartPoint
                    : adrMainSystem.GetZeroMarkerPosition();
            isInverseAggregate = math.distancesq(refE0, zeroMarker) > math.distancesq(refE1, zeroMarker);
            return true;
        }

        public static bool IsFreedomUnitSystem() =>
            GameManager.instance.settings.userInterface.unitSystem
            == Game.Settings.InterfaceSettings.UnitSystem.Freedom;

        /// <summary>
        /// Locale for decimal separators — game UI culture when available, otherwise current UI culture.
        /// </summary>
        public static CultureInfo GetUiCulture()
        {
            try
            {
                var locale = GameManager.instance?.localizationManager?.activeLocaleId;
                if (!string.IsNullOrEmpty(locale))
                {
                    return CultureInfo.GetCultureInfo(locale);
                }
            }
            catch
            {
                // fall through
            }

            return CultureInfo.CurrentUICulture;
        }

        public static string FormatMileageLabel(bool freedom, float majorUnits, CultureInfo culture = null)
        {
            culture ??= GetUiCulture();
            string prefix = freedom ? "mi" : "km";
            float roundedHalf = math.round(majorUnits * 2f) / 2f;
            if (math.abs(roundedHalf - math.round(roundedHalf)) < 0.01f)
            {
                return $"{prefix} {((int)math.round(roundedHalf)).ToString(culture)}";
            }

            return $"{prefix} {roundedHalf.ToString("0.0", culture)}";
        }

        public static string FormatHouseNumberRange(int low, int high, CultureInfo culture = null)
        {
            culture ??= GetUiCulture();
            if (low == high)
            {
                return low.ToString(culture);
            }

            return $"{low.ToString(culture)}–{high.ToString(culture)}";
        }

        public static bool TryGetOddNumberAtCurvePos(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            AdrHighwayRoutesSystem highwayRoutesSystem,
            Entity edge,
            float curvePos,
            out int number)
        {
            number = 0;
            if (!entityManager.TryGetComponent(edge, out Aggregated aggregated)
                || !entityManager.TryGetBuffer(aggregated.m_Aggregate, true, out DynamicBuffer<AggregateElement> aggregationBuffer)
                || aggregationBuffer.Length == 0)
            {
                return false;
            }

            if (!TryResolveInverseAggregate(
                    entityManager, adrMainSystem, highwayRoutesSystem, aggregated.m_Aggregate, aggregationBuffer,
                    out bool isInverseAggregate))
            {
                return false;
            }

            float currentDistance = 0f;
            float lastOverrideNumber = 0f;
            entityManager.TryGetComponent<ADRHighwayAggregationCacheData>(aggregated.m_Aggregate, out var cacheData);
            float overridedToKm = cacheData.startDistanceOverrideKm;
            bool isInverseOverride = cacheData.reverseCounting;

            for (int i = isInverseAggregate ? aggregationBuffer.Length - 1 : 0;
                 isInverseAggregate ? i >= 0 : i < aggregationBuffer.Length;
                 i += isInverseAggregate ? -1 : 1)
            {
                AggregateElement aggregateElement = aggregationBuffer[i];
                float newDistance = currentDistance;

                if (entityManager.TryGetComponent(aggregateElement.m_Edge, out Curve curve))
                {
                    newDistance += curve.m_Length;
                }

                if (entityManager.TryGetBuffer<SubObject>(aggregateElement.m_Edge, true, out var items) && items.Length > 0)
                {
                    bool? isReversedSegment = null;
                    float overridePosition = -1;
                    float targetNewNumber = 0f;
                    float maxTarget = 1;
                    if (aggregateElement.m_Edge == edge)
                    {
                        isReversedSegment ??= CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);
                        maxTarget = isReversedSegment.Value ? 1 - curvePos : curvePos;
                    }

                    for (int j = 0; j < items.Length; j++)
                    {
                        if (entityManager.TryGetComponent<Attached>(items[j].m_SubObject, out var attachmentData)
                            && entityManager.TryGetComponent<ADRHighwayMarkerData>(items[j].m_SubObject, out var markerData)
                            && markerData.overrideMileage)
                        {
                            isReversedSegment ??= CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);
                            var thisPosition = isReversedSegment.Value ? 1 - attachmentData.m_CurvePosition : attachmentData.m_CurvePosition;
                            if (thisPosition > overridePosition && thisPosition <= maxTarget)
                            {
                                overridePosition = thisPosition;
                                targetNewNumber = markerData.newMileage;
                                isInverseOverride = markerData.reverseMileageCounting;
                            }
                        }
                    }

                    if (overridePosition >= 0)
                    {
                        var t = new Bounds1(0f, overridePosition);
                        float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                        lastOverrideNumber = math.lerp(currentDistance, newDistance, s);
                        overridedToKm = targetNewNumber;
                    }
                }

                if (aggregateElement.m_Edge == edge)
                {
                    bool isReversedSegment = CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);
                    var t = new Bounds1(isReversedSegment ? curvePos : 0f, isReversedSegment ? 1f : curvePos);
                    float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                    var unitMultiplier = IsFreedomUnitSystem() ? 1.09361329f : 1f;
                    number = (Mathf.RoundToInt(
                                  ((overridedToKm * 500)
                                   + ((isInverseOverride ? -.5f : .5f)
                                      * (math.lerp(currentDistance, newDistance, s) - lastOverrideNumber)))
                                  * unitMultiplier) * 2) + 1;
                    return true;
                }

                currentDistance = newDistance;
            }

            return false;
        }

        public static bool CheckSegmentReversion(
            EntityManager entityManager,
            DynamicBuffer<AggregateElement> dynamicBuffer,
            bool isInverseAggregate,
            int i,
            AggregateElement aggregateElement)
        {
            bool isReversedSegment = false;
            if (i > 0)
            {
                if (entityManager.TryGetComponent(aggregateElement.m_Edge, out Edge edge2)
                    && entityManager.TryGetComponent(dynamicBuffer[i - 1].m_Edge, out Edge edge3)
                    && (edge2.m_End == edge3.m_Start || edge2.m_End == edge3.m_End))
                {
                    isReversedSegment = true;
                }
            }
            else if (i < dynamicBuffer.Length - 1
                     && entityManager.TryGetComponent(aggregateElement.m_Edge, out Edge edge4)
                     && entityManager.TryGetComponent(dynamicBuffer[i + 1].m_Edge, out Edge edge5)
                     && (edge4.m_Start == edge5.m_Start || edge4.m_Start == edge5.m_End))
            {
                isReversedSegment = true;
            }

            if (isInverseAggregate)
            {
                isReversedSegment = !isReversedSegment;
            }

            return isReversedSegment;
        }
    }
}
