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
    /// Shared ADR house-number math (same family as <see cref="BuildingUtilsOverrides"/> GetAddress override).
    /// Prefers ADR name-aggregate membership when the edge is linked.
    /// </summary>
    public static class AdrRoadAddressNumbering
    {
        public const int DenseSegmentCardThreshold = 10;
        public const float HalfKilometerMeters = 500f;
        public const float HalfMileMeters = 804.672f; // 0.5 * 1609.344

        public static bool TryGetAdrAggregate(EntityManager entityManager, Entity edge, out Entity adrAggregate)
        {
            adrAggregate = Entity.Null;
            if (!entityManager.TryGetComponent(edge, out ADRNameAggregated link)
                || link.m_Aggregate == Entity.Null
                || !entityManager.Exists(link.m_Aggregate)
                || !entityManager.HasComponent<ADRNameAggregate>(link.m_Aggregate))
            {
                return false;
            }
            adrAggregate = link.m_Aggregate;
            return true;
        }

        public static bool TryResolveInverseAggregate(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
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

            var zeroMarker = adrMainSystem.GetZeroMarkerPosition();
            isInverseAggregate = math.distancesq(refE0, zeroMarker) > math.distancesq(refE1, zeroMarker);
            return true;
        }

        public static bool TryResolveInverseAggregate(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            DynamicBuffer<ADRNameAggregateElement> aggregationBuffer,
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

            var zeroMarker = adrMainSystem.GetZeroMarkerPosition();
            isInverseAggregate = math.distancesq(refE0, zeroMarker) > math.distancesq(refE1, zeroMarker);
            return true;
        }

        public static bool IsFreedomUnitSystem() =>
            GameManager.instance.settings.userInterface.unitSystem
            == Game.Settings.InterfaceSettings.UnitSystem.Freedom;

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

        /// <summary>
        /// Resolves road naming entity + house number. Prefers ADR membership when linked.
        /// </summary>
        public static bool TryGetAddress(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            Entity sideEntity,
            Entity edge,
            float curvePos,
            out Entity road,
            out int number)
        {
            road = Entity.Null;
            number = 0;

            if (TryGetAdrAggregate(entityManager, edge, out Entity adrAggregate)
                && entityManager.TryGetBuffer(adrAggregate, true, out DynamicBuffer<ADRNameAggregateElement> adrBuffer)
                && adrBuffer.Length > 0)
            {
                return TryGetAddressAlongAdr(
                    entityManager, adrMainSystem, sideEntity, edge, curvePos, adrAggregate, adrBuffer,
                    out road, out number);
            }

            if (entityManager.TryGetComponent(edge, out Aggregated aggregated)
                && entityManager.TryGetBuffer(aggregated.m_Aggregate, true, out DynamicBuffer<AggregateElement> aggregationBuffer)
                && aggregationBuffer.Length > 0)
            {
                return TryGetAddressAlongVanilla(
                    entityManager, adrMainSystem, sideEntity, edge, curvePos, aggregated.m_Aggregate, aggregationBuffer,
                    out road, out number);
            }

            return false;
        }

        public static bool TryGetOddNumberAtCurvePos(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            Entity edge,
            float curvePos,
            out int number)
        {
            return TryGetAddress(entityManager, adrMainSystem, Entity.Null, edge, curvePos, out _, out number);
        }

        private static bool TryGetAddressAlongAdr(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            Entity sideEntity,
            Entity edge,
            float curvePos,
            Entity adrAggregate,
            DynamicBuffer<ADRNameAggregateElement> aggregationBuffer,
            out Entity road,
            out int number)
        {
            road = Entity.Null;
            number = 0;
            if (!TryResolveInverseAggregate(entityManager, adrMainSystem, aggregationBuffer, out bool isInverseAggregate))
            {
                return false;
            }

            float currentDistance = 0f;
            for (int i = isInverseAggregate ? aggregationBuffer.Length - 1 : 0;
                 isInverseAggregate ? i >= 0 : i < aggregationBuffer.Length;
                 i += isInverseAggregate ? -1 : 1)
            {
                ADRNameAggregateElement aggregateElement = aggregationBuffer[i];
                float newDistance = currentDistance;
                bool hasCurve = entityManager.TryGetComponent(aggregateElement.m_Edge, out Curve curve);
                if (hasCurve)
                {
                    newDistance += curve.m_Length;
                }

                if (aggregateElement.m_Edge != edge)
                {
                    currentDistance = newDistance;
                    continue;
                }

                road = adrAggregate;
                bool isReversedSegment = CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);

                if (aggregateElement.numberOverrideActive)
                {
                    number = aggregateElement.numberOverride;
                }
                else if (hasCurve)
                {
                    var t = new Bounds1(isReversedSegment ? curvePos : 0f, isReversedSegment ? 1f : curvePos);
                    float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                    var unitMultiplier = IsFreedomUnitSystem() ? 1.09361329f : 1f;
                    number = (Mathf.RoundToInt((.5f * math.lerp(currentDistance, newDistance, s)) * unitMultiplier) * 2) + 1;
                }
                else
                {
                    return false;
                }

                if (hasCurve)
                {
                    ApplyBuildingSideParity(entityManager, sideEntity, curve, curvePos, isReversedSegment, ref number);
                }
                return true;
            }

            return false;
        }

        private static bool TryGetAddressAlongVanilla(
            EntityManager entityManager,
            AdrMainSystem adrMainSystem,
            Entity sideEntity,
            Entity edge,
            float curvePos,
            Entity vanillaAggregate,
            DynamicBuffer<AggregateElement> aggregationBuffer,
            out Entity road,
            out int number)
        {
            road = Entity.Null;
            number = 0;
            if (!TryResolveInverseAggregate(entityManager, adrMainSystem, aggregationBuffer, out bool isInverseAggregate))
            {
                return false;
            }

            float currentDistance = 0f;
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

                if (aggregateElement.m_Edge != edge)
                {
                    currentDistance = newDistance;
                    continue;
                }

                bool isReversedSegment = CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);
                var t = new Bounds1(isReversedSegment ? curvePos : 0f, isReversedSegment ? 1f : curvePos);
                float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                road = vanillaAggregate;
                var unitMultiplier = IsFreedomUnitSystem() ? 1.09361329f : 1f;
                number = (Mathf.RoundToInt((.5f * math.lerp(currentDistance, newDistance, s)) * unitMultiplier) * 2) + 1;
                ApplyBuildingSideParity(entityManager, sideEntity, curve, curvePos, isReversedSegment, ref number);
                return true;
            }

            return false;
        }

        private static void ApplyBuildingSideParity(
            EntityManager entityManager,
            Entity sideEntity,
            Curve curve,
            float curvePos,
            bool isReversedSegment,
            ref int number)
        {
            if (sideEntity == Entity.Null
                || !entityManager.TryGetComponent(sideEntity, out Transform transform))
            {
                return;
            }

            float2 x2 = transform.m_Position.xz - MathUtils.Position(curve.m_Bezier, curvePos).xz;
            float2 y2 = MathUtils.Right(MathUtils.Tangent(curve.m_Bezier, curvePos).xz);
            if ((math.dot(x2, y2) > 0f) != isReversedSegment)
            {
                number++;
            }
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

        public static bool CheckSegmentReversion(
            EntityManager entityManager,
            DynamicBuffer<ADRNameAggregateElement> dynamicBuffer,
            bool isInverseAggregate,
            int i,
            ADRNameAggregateElement aggregateElement)
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
