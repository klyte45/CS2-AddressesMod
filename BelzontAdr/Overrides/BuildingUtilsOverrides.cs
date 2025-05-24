using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Buildings;
using Game.Net;
using Game.Prefabs;
using Game.Zones;
using Game.Objects;
using Game.SceneFlow;
using Game.Tools;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BelzontAdr
{
    public class BuildingUtilsOverrides : Redirector, IRedirectable
    {
        public void DoPatches(World world)
        {
            adrMainSystem = world.GetOrCreateSystemManaged<AdrMainSystem>();

            var GetAddress = typeof(BuildingUtils).GetMethod("GetAddress", RedirectorUtils.allFlags, null, new[]
            {
                typeof(EntityManager),
                typeof(Entity),
                typeof(Entity),
                typeof(float),
                typeof(Entity).MakeByRefType(),
                typeof(int).MakeByRefType()
            }, null);
            AddRedirect(GetAddress, GetType().GetMethod(nameof(GetAddress_Override), RedirectorUtils.allFlags));
        }

        private static bool GetAddress_Override(ref bool __result, EntityManager entityManager, Entity entity, Entity edge, float curvePos, ref Entity road, ref int number)
        {
            if (entityManager.TryGetComponent<Temp>(edge, out var temp) && temp.m_Original != Entity.Null)
            {
                edge = temp.m_Original;
                curvePos = temp.m_CurvePosition;
            }

            if (entityManager.TryGetComponent(edge, out Aggregated aggregated) && entityManager.TryGetBuffer(aggregated.m_Aggregate, true, out DynamicBuffer<AggregateElement> aggregationBuffer))
            {
                float currentDistance = 0f;
                if (!entityManager.TryGetComponent(aggregationBuffer[0].m_Edge, out Curve e0) || !entityManager.TryGetComponent(aggregationBuffer[^1].m_Edge, out Curve e1)) return true;
                var zeroMarker = adrMainSystem.GetZeroMarkerPosition();
                bool isInverseAggregate = e0.m_Bezier.a.SqrDistance(zeroMarker) > e1.m_Bezier.a.SqrDistance(zeroMarker);

                float lastOverrideNumber = 0f;
                float overridedTo = 0f;
                bool isInverseOverride = false;

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


                    if (entityManager.TryGetBuffer<Game.Objects.SubObject>(aggregateElement.m_Edge, true, out var items) && items.Length > 0)
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
                            var t = new Bounds1(isReversedSegment.Value ? overridePosition : 0f, isReversedSegment.Value ? 1f : overridePosition);
                            float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                            lastOverrideNumber = math.lerp(currentDistance, newDistance, s);
                            overridedTo = targetNewNumber;
                        }
                    }

                    if (aggregateElement.m_Edge == edge)
                    {
                        bool isReversedSegment = CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);

                        var t = new Bounds1(isReversedSegment ? curvePos : 0f, isReversedSegment ? 1f : curvePos);
                        float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                        road = aggregated.m_Aggregate;
                        var unitMultiplier = GameManager.instance.settings.userInterface.unitSystem == Game.Settings.InterfaceSettings.UnitSystem.Freedom ? 1.09361329f : 1f; //yd or m
                        number = (Mathf.RoundToInt(((overridedTo * 500) + ((isInverseOverride ? -.5f : .5f) * (math.lerp(currentDistance, newDistance, s) - lastOverrideNumber))) * unitMultiplier) * 2) + 1;

                        //Check road side
                        if (entityManager.TryGetComponent(entity, out Game.Objects.Transform transform))
                        {
                            float2 x2 = transform.m_Position.xz - MathUtils.Position(curve.m_Bezier, curvePos).xz;
                            float2 y2 = MathUtils.Right(MathUtils.Tangent(curve.m_Bezier, curvePos).xz);
                            if ((math.dot(x2, y2) > 0f) != isReversedSegment)
                            {
                                number++;
                            }
                        }
                        __result = true;
                        return false;
                    }
                    currentDistance = newDistance;
                }
            }

            __result = false;
            return false;
        }

        private static bool CheckSegmentReversion(EntityManager entityManager, DynamicBuffer<AggregateElement> dynamicBuffer, bool isInverseAggregate, int i, AggregateElement aggregateElement)
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
            if (isInverseAggregate) isReversedSegment = !isReversedSegment;
            return isReversedSegment;
        }

        private static AdrMainSystem adrMainSystem;
    }
}