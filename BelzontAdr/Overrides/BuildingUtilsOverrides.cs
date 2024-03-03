using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Buildings;
using Game.Net;
using Game.Prefabs;
using Game.Zones;
using Kwytto.Utils;
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
            AddRedirect(GetAddress, GetType().GetMethod("GetAddress", RedirectorUtils.allFlags));
        }

        private static bool GetAddress(ref bool __result, ref EntityManager entityManager, ref Entity entity, ref Entity edge, ref float curvePos, ref Entity road, ref int number)
        {
            if (entityManager.TryGetComponent(edge, out Aggregated aggregated) && entityManager.TryGetBuffer(aggregated.m_Aggregate, true, out DynamicBuffer<AggregateElement> dynamicBuffer))
            {
                float num = 0f;
                if (entityManager.TryGetComponent(dynamicBuffer[0].m_Edge, out Curve e0)) return true;
                if (entityManager.TryGetComponent(dynamicBuffer[^1].m_Edge, out Curve e1)) return true;
                var zeroMarker = adrMainSystem.GetZeroMarkerPosition();
                bool isInverse = e0.m_Bezier.a.SqrDistance(zeroMarker) > e1.m_Bezier.a.SqrDistance(zeroMarker);
                for (int i = isInverse ? dynamicBuffer.Length - 1 : 0;
                     isInverse ? i >= 0 : i < dynamicBuffer.Length;
                    i += isInverse ? -1 : 1)
                {
                    AggregateElement aggregateElement = dynamicBuffer[i];
                    float num2 = num;
                    if (entityManager.TryGetComponent(aggregateElement.m_Edge, out Curve curve)
                        && entityManager.TryGetComponent(aggregateElement.m_Edge, out Composition composition)
                        && entityManager.TryGetComponent(composition.m_Edge, out NetCompositionData netCompositionData))
                    {
                        float2 x = math.normalizesafe(MathUtils.StartTangent(curve.m_Bezier).xz, default);
                        float2 y = math.normalizesafe(MathUtils.EndTangent(curve.m_Bezier).xz, default);
                        float num3 = ZoneUtils.GetCellWidth(netCompositionData.m_Width);
                        float num4 = math.acos(math.clamp(math.dot(x, y), -1f, 1f));
                        num2 += curve.m_Length + (num3 * num4 * 0.5f);
                    }
                    if (aggregateElement.m_Edge == edge)
                    {
                        bool flag = false;
                        if (i > 0)
                        {
                            if (entityManager.TryGetComponent(aggregateElement.m_Edge, out Edge edge2)
                                && entityManager.TryGetComponent(dynamicBuffer[i - 1].m_Edge, out Edge edge3)
                                && (edge2.m_End == edge3.m_Start || edge2.m_End == edge3.m_End))
                            {
                                flag = true;
                            }
                        }
                        else if (i < dynamicBuffer.Length - 1
                            && entityManager.TryGetComponent(aggregateElement.m_Edge, out Edge edge4)
                            && entityManager.TryGetComponent(dynamicBuffer[i + 1].m_Edge, out Edge edge5)
                            && (edge4.m_Start == edge5.m_Start || edge4.m_Start == edge5.m_End))
                        {
                            flag = true;
                        }
                        Bounds1 t = new Bounds1(flag ? curvePos : 0f, flag ? 1f : curvePos);
                        float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));
                        road = aggregated.m_Aggregate;
                        number = Mathf.RoundToInt(math.lerp(num, num2, s) / 8f) * 2 + 1;
                        Game.Objects.Transform transform;
                        if (entityManager.TryGetComponent(entity, out transform))
                        {
                            float2 x2 = transform.m_Position.xz - MathUtils.Position(curve.m_Bezier, curvePos).xz;
                            float2 y2 = MathUtils.Right(MathUtils.Tangent(curve.m_Bezier, curvePos).xz);
                            if (math.dot(x2, y2) > 0f != flag)
                            {
                                number++;
                            }
                        }
                        __result = true;
                        return false;
                    }
                    num = num2;
                }
            }

            __result = false;
            return false;
        }

        private static AdrMainSystem adrMainSystem;
    }
}