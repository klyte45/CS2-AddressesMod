using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Buildings;
using Game.Net;
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

                var refE0 = CheckSegmentReversion(entityManager, aggregationBuffer, false, 0, aggregationBuffer[0]) ? e0.m_Bezier.d.xz : e0.m_Bezier.a.xz;
                var refE1 = CheckSegmentReversion(entityManager, aggregationBuffer, false, aggregationBuffer.Length - 1, aggregationBuffer[^1]) ? e1.m_Bezier.a.xz : e1.m_Bezier.d.xz;

                var zeroMarker = adrMainSystem.GetZeroMarkerPosition();
                bool isInverseAggregate = refE0.SqrDistance(zeroMarker) > refE1.SqrDistance(zeroMarker);

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

                    if (aggregateElement.m_Edge == edge)
                    {
                        bool isReversedSegment = CheckSegmentReversion(entityManager, aggregationBuffer, isInverseAggregate, i, aggregateElement);

                        var t = new Bounds1(isReversedSegment ? curvePos : 0f, isReversedSegment ? 1f : curvePos);
                        float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                        road = aggregated.m_Aggregate;
                        var unitMultiplier = GameManager.instance.settings.userInterface.unitSystem == Game.Settings.InterfaceSettings.UnitSystem.Freedom ? 1.09361329f : 1f; //yd or m
                        number = (Mathf.RoundToInt((.5f * math.lerp(currentDistance, newDistance, s)) * unitMultiplier) * 2) + 1;

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
