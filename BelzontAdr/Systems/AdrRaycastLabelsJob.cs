using Colossal.Collections;
using Colossal.Mathematics;
using Game.Common;
using Game.Net;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-05: Burst clone of Game.Net.RaycastJobs.RaycastLabelsJob (~596–747).
    /// Owner resolve: ADRNameAggregated.m_Aggregate (S2) else Aggregated.m_Aggregate (S1).
    /// CheckAggregate math copied verbatim. Re-diff GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public static class AdrRaycastLabelsJobs
    {
#if BURST
        [BurstCompile]
#endif
        public struct AdrRaycastLabelsJob : IJobParallelForDefer
        {
            [ReadOnly] public NativeArray<RaycastInput> m_Input;
            [ReadOnly] public float3 m_CameraRight;
            [ReadOnly] public NativeArray<RaycastSystem.EntityResult> m_Edges;
            [ReadOnly] public ComponentLookup<Aggregated> m_AggregatedData;
            [ReadOnly] public ComponentLookup<ADRNameAggregated> m_AdrAggregatedData;
            [ReadOnly] public ComponentLookup<LabelExtents> m_LabelExtentsData;
            [ReadOnly] public BufferLookup<LabelPosition> m_LabelPositions;
            [NativeDisableContainerSafetyRestriction]
            public NativeAccumulator<RaycastResult>.ParallelWriter m_Results;

            public void Execute(int index)
            {
                RaycastSystem.EntityResult entityResult = m_Edges[index];
                RaycastInput input = m_Input[entityResult.m_RaycastIndex];
                if ((input.m_TypeMask & TypeMask.Labels) == TypeMask.None)
                {
                    return;
                }

                Entity owner = Entity.Null;
                if (m_AdrAggregatedData.TryGetComponent(entityResult.m_Entity, out ADRNameAggregated adrAggregated))
                {
                    owner = adrAggregated.m_Aggregate;
                }
                else if (m_AggregatedData.TryGetComponent(entityResult.m_Entity, out Aggregated aggregated))
                {
                    owner = aggregated.m_Aggregate;
                }

                if (owner != Entity.Null)
                {
                    CheckAggregate(entityResult.m_RaycastIndex, input, owner);
                }
            }

            private void CheckAggregate(int raycastIndex, RaycastInput input, Entity aggregate)
            {
                if (!m_LabelExtentsData.TryGetComponent(aggregate, out LabelExtents componentData))
                {
                    return;
                }
                DynamicBuffer<LabelPosition> dynamicBuffer = m_LabelPositions[aggregate];
                Quad3 quad = default;
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    LabelPosition labelPosition = dynamicBuffer[i];
                    if ((NetUtils.GetCollisionMask(labelPosition) & input.m_CollisionMask) == 0)
                    {
                        continue;
                    }
                    float3 float5 = MathUtils.Position(labelPosition.m_Curve, 0.5f);
                    float num = math.max(math.sqrt(math.distance(input.m_Line.a, float5) * 0.0001f), 0.01f);
                    if (num >= labelPosition.m_MaxScale * 0.95f)
                    {
                        continue;
                    }
                    float3 y = MathUtils.Tangent(labelPosition.m_Curve, 0.5f);
                    Bounds2 bounds = ((math.dot(m_CameraRight, y) < 0f)
                        ? new Bounds2(-componentData.m_Bounds.max, -componentData.m_Bounds.min)
                        : componentData.m_Bounds);
                    bounds *= (float2)num;
                    Bounds1 t = new Bounds1(0f, 1f);
                    Bounds1 t2 = new Bounds1(0f, 1f);
                    float num2 = 0f - bounds.min.x - labelPosition.m_HalfLength;
                    float num3 = bounds.max.x - labelPosition.m_HalfLength;
                    if (num2 < 0f)
                    {
                        MathUtils.ClampLength(labelPosition.m_Curve, ref t, 0f - num2);
                    }
                    else
                    {
                        t.max = 0f;
                    }
                    if (num3 < 0f)
                    {
                        MathUtils.ClampLengthInverse(labelPosition.m_Curve, ref t2, 0f - num3);
                    }
                    else
                    {
                        t2.min = 1f;
                    }
                    if (num2 > 0f)
                    {
                        float3 float6 = math.normalizesafe(MathUtils.StartTangent(labelPosition.m_Curve));
                        float3 float7 = new float3(0f - float6.z, 0f, float6.x);
                        quad.a = labelPosition.m_Curve.a - float6 * num2 + float7 * bounds.min.y;
                        quad.b = labelPosition.m_Curve.a - float6 * num2 + float7 * bounds.max.y;
                        quad.c = labelPosition.m_Curve.a + float7 * bounds.max.y;
                        quad.d = labelPosition.m_Curve.a + float7 * bounds.min.y;
                        if (MathUtils.Intersect(quad, input.m_Line, out var t3))
                        {
                            float num4 = MathUtils.Size(bounds.y);
                            RaycastResult value = default;
                            value.m_Owner = aggregate;
                            value.m_Hit.m_HitEntity = value.m_Owner;
                            value.m_Hit.m_Position = float5;
                            value.m_Hit.m_HitPosition = MathUtils.Position(input.m_Line, t3);
                            value.m_Hit.m_NormalizedDistance = t3 - num4 / math.max(1f, MathUtils.Length(input.m_Line));
                            value.m_Hit.m_CellIndex = new int2(labelPosition.m_ElementIndex, -1);
                            m_Results.Accumulate(raycastIndex, value);
                        }
                    }
                    else
                    {
                        float3 float8 = MathUtils.Position(labelPosition.m_Curve, t.max);
                        float3 float9 = math.normalizesafe(MathUtils.Tangent(labelPosition.m_Curve, t.max));
                        float3 float10 = new float3(0f - float9.z, 0f, float9.x);
                        quad.c = float8 + float10 * bounds.max.y;
                        quad.d = float8 + float10 * bounds.min.y;
                    }
                    for (int j = 1; j <= 16; j++)
                    {
                        float t4 = math.lerp(t.max, t2.min, (float)j * 0.0625f);
                        float3 float11 = MathUtils.Position(labelPosition.m_Curve, t4);
                        float3 float12 = math.normalizesafe(MathUtils.Tangent(labelPosition.m_Curve, t4));
                        float3 float13 = new float3(0f - float12.z, 0f, float12.x);
                        quad.a = quad.d;
                        quad.b = quad.c;
                        quad.c = float11 + float13 * bounds.max.y;
                        quad.d = float11 + float13 * bounds.min.y;
                        if (MathUtils.Intersect(quad, input.m_Line, out var t5))
                        {
                            float num5 = MathUtils.Size(bounds.y);
                            RaycastResult value2 = default;
                            value2.m_Owner = aggregate;
                            value2.m_Hit.m_HitEntity = value2.m_Owner;
                            value2.m_Hit.m_Position = float5;
                            value2.m_Hit.m_HitPosition = MathUtils.Position(input.m_Line, t5);
                            value2.m_Hit.m_NormalizedDistance = t5 - num5 / math.max(1f, MathUtils.Length(input.m_Line));
                            value2.m_Hit.m_CellIndex = new int2(labelPosition.m_ElementIndex, -1);
                            m_Results.Accumulate(raycastIndex, value2);
                        }
                    }
                    if (num3 > 0f)
                    {
                        float3 float14 = math.normalizesafe(MathUtils.EndTangent(labelPosition.m_Curve));
                        float3 float15 = new float3(0f - float14.z, 0f, float14.x);
                        quad.a = quad.d;
                        quad.b = quad.c;
                        quad.c = labelPosition.m_Curve.d + float14 * num3 + float15 * bounds.min.y;
                        quad.d = labelPosition.m_Curve.d + float14 * num3 + float15 * bounds.max.y;
                        if (MathUtils.Intersect(quad, input.m_Line, out var t6))
                        {
                            float num6 = MathUtils.Size(bounds.y);
                            RaycastResult value3 = default;
                            value3.m_Owner = aggregate;
                            value3.m_Hit.m_HitEntity = value3.m_Owner;
                            value3.m_Hit.m_Position = float5;
                            value3.m_Hit.m_HitPosition = MathUtils.Position(input.m_Line, t6);
                            value3.m_Hit.m_NormalizedDistance = t6 - num6 / math.max(1f, MathUtils.Length(input.m_Line));
                            value3.m_Hit.m_CellIndex = new int2(labelPosition.m_ElementIndex, -1);
                            m_Results.Accumulate(raycastIndex, value3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Drop-in replacement for vanilla Net RaycastLabelsJob.Schedule — reuses stub field inits,
        /// adds ADRNameAggregated lookup, schedules <see cref="AdrRaycastLabelsJob"/>.
        /// </summary>
        /// <summary>
        /// Must take the vanilla stub <b>by value</b> — PerformRaycast IL uses ldloc (not ldloca)
        /// before Schedule, matching IJobParallelForDeferExtensions.Schedule(this T, …).
        /// </summary>
        public static JobHandle ScheduleFromVanillaStub(
            Game.Net.RaycastJobs.RaycastLabelsJob vanillaStub,
            NativeList<RaycastSystem.EntityResult> edges,
            int batch,
            JobHandle deps,
            RaycastSystem system)
        {
            var job = new AdrRaycastLabelsJob
            {
                m_Input = vanillaStub.m_Input,
                m_CameraRight = vanillaStub.m_CameraRight,
                m_Edges = vanillaStub.m_Edges,
                m_AggregatedData = system.GetComponentLookup<Aggregated>(true),
                m_AdrAggregatedData = system.GetComponentLookup<ADRNameAggregated>(true),
                m_LabelExtentsData = system.GetComponentLookup<LabelExtents>(true),
                m_LabelPositions = system.GetBufferLookup<LabelPosition>(true),
                m_Results = vanillaStub.m_Results
            };
            return job.Schedule(edges, batch, deps);
        }
    }
}
