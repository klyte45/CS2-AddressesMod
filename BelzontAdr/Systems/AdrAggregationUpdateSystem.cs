#if DEBUG
using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Collections;
using Colossal.Mathematics;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using HarmonyLib;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BelzontAdr
{
    public partial class AdrAggregationSystem : GameSystemBase
    {
        private ModificationBarrier2B m_ModificationBarrier;
        private EntityQuery m_ModifiedQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            var onCreate = typeof(AggregateSystem).GetMethod("OnCreate", RedirectorUtils.allFlags);
            var onUpdate = typeof(AggregateSystem).GetMethod("OnUpdate", RedirectorUtils.allFlags);
            var preventDefault = typeof(Redirector).GetMethod("PreventDefault", RedirectorUtils.allFlags);
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"AdrAggregationSystem: Patching {onCreate} with {preventDefault}");
            Redirector.Harmony.Patch(onCreate, new HarmonyMethod(preventDefault));
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"AdrAggregationSystem: Patching {onUpdate} with {preventDefault}");
            Redirector.Harmony.Patch(onUpdate, new HarmonyMethod(preventDefault));



            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier2B>();
            m_ModifiedQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new() {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregated>()
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Created>(),
                        ComponentType.ReadOnly<Updated>(),
                        ComponentType.ReadOnly<Deleted>()
                    },
                    None = new ComponentType[0]
                }
            });
            RequireForUpdate(m_ModifiedQuery);
        }

        protected override void OnUpdate()
        {
            NativeList<ArchetypeChunk> chunks = m_ModifiedQuery.ToArchetypeChunkListAsync(Allocator.TempJob, out JobHandle job);
            var aggJob = new AdrAggregationUpdateJob
            {
                m_AggregatedData = GetComponentLookup<Aggregated>(),
                m_AggregateElements = GetBufferLookup<AggregateElement>(),
                m_Chunks = chunks,
                m_CommandBuffer = m_ModificationBarrier.CreateCommandBuffer(),
                m_ConnectedEdges = GetBufferLookup<ConnectedEdge>(true),
                m_CurveData = GetComponentLookup<Curve>(true),
                m_DeletedData = GetComponentLookup<Deleted>(true),
                m_EdgeData = GetComponentLookup<Edge>(true),
                m_PrefabAggregateData = GetComponentLookup<AggregateNetData>(true),
                m_PrefabGeometryData = GetComponentLookup<NetGeometryData>(true),
                m_PrefabRefData = GetComponentLookup<PrefabRef>(true),
                m_TempData = GetComponentLookup<Temp>(true),
                m_EntityType = GetEntityTypeHandle(),
                m_PlaceableNetData = GetComponentLookup<PlaceableNetData>(true),
                m_RoadData = GetComponentLookup<RoadData>(true),
                m_ElevationData = GetComponentLookup<Elevation>(true),
            };
            JobHandle jobHandle = aggJob.Schedule(JobHandle.CombineDependencies(Dependency, job));
            chunks.Dispose(jobHandle);
            m_ModificationBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

        //    [BurstCompile]
        private struct AdrAggregationUpdateJob : IJob
        {
            public ComponentLookup<Aggregated> m_AggregatedData;
            public BufferLookup<AggregateElement> m_AggregateElements;
            [ReadOnly] public NativeList<ArchetypeChunk> m_Chunks;
            public EntityCommandBuffer m_CommandBuffer;
            [ReadOnly] public BufferLookup<ConnectedEdge> m_ConnectedEdges;
            [ReadOnly] public ComponentLookup<Curve> m_CurveData;
            [ReadOnly] public ComponentLookup<Deleted> m_DeletedData;
            [ReadOnly] public ComponentLookup<Edge> m_EdgeData;
            [ReadOnly] public ComponentLookup<Elevation> m_ElevationData;
            [ReadOnly] public ComponentLookup<AggregateNetData> m_PrefabAggregateData;
            [ReadOnly] public ComponentLookup<NetGeometryData> m_PrefabGeometryData;
            [ReadOnly] public ComponentLookup<PrefabRef> m_PrefabRefData;
            [ReadOnly] public ComponentLookup<PlaceableNetData> m_PlaceableNetData;
            [ReadOnly] public ComponentLookup<RoadData> m_RoadData;
            [ReadOnly] public ComponentLookup<Temp> m_TempData;
            [ReadOnly] public EntityTypeHandle m_EntityType;


            public unsafe void Execute()
            {
                int num = 0;
                for (int i = 0; i < m_Chunks.Length; i++)
                {
                    num += m_Chunks[i].Count;
                }
                NativeParallelHashSet<Entity> edgeSet = new(num, Allocator.Temp);
                NativeParallelHashSet<bool> edgeSetIsHw = new(num, Allocator.Temp);
                NativeParallelHashSet<Entity> emptySet = new(num, Allocator.Temp);
                NativeParallelHashMap<Entity, Entity> updateMap = new(num, Allocator.Temp);

                for (int j = 0; j < m_Chunks.Length; j++)
                {
                    ArchetypeChunk archetypeChunk = m_Chunks[j];
                    bool flag = archetypeChunk.Has<Temp>();
                    if (archetypeChunk.Has<Created>())
                    {
                        NativeArray<Entity> nativeArray = archetypeChunk.GetNativeArray(m_EntityType);
                        for (int k = 0; k < nativeArray.Length; k++)
                        {
                            Entity entity = nativeArray[k];
                            Aggregated aggregated = m_AggregatedData[entity];
                            if (aggregated.m_Aggregate != Entity.Null)
                            {
                                if (flag && !m_TempData.HasComponent(aggregated.m_Aggregate))
                                {
                                    updateMap.TryAdd(aggregated.m_Aggregate, aggregated.m_Aggregate);
                                }
                                else
                                {
                                    m_AggregateElements[aggregated.m_Aggregate].Add(new AggregateElement(entity));
                                    edgeSet.Add(aggregated.m_Aggregate);
                                    edgeSetIsHw.Add(IsHighway(m_PrefabRefData[entity]));
                                }
                            }
                            else
                            {
                                emptySet.Add(entity);
                            }
                        }
                    }
                    else
                    {
                        NativeArray<Entity> nativeArray2 = archetypeChunk.GetNativeArray(m_EntityType);
                        for (int l = 0; l < nativeArray2.Length; l++)
                        {
                            Entity entity2 = nativeArray2[l];
                            Aggregated aggregated2 = m_AggregatedData[entity2];
                            if (aggregated2.m_Aggregate != Entity.Null)
                            {
                                if (flag && !m_TempData.HasComponent(aggregated2.m_Aggregate))
                                {
                                    updateMap.TryAdd(aggregated2.m_Aggregate, aggregated2.m_Aggregate);
                                }
                                else
                                {
                                    edgeSet.Add(aggregated2.m_Aggregate);
                                }
                            }
                        }
                    }
                }
                if (!edgeSet.IsEmpty)
                {
                    NativeArray<Entity> nativeArray3 = edgeSet.ToNativeArray(Allocator.TempJob);
                    NativeArray<bool> nativeArray4 = edgeSetIsHw.ToNativeArray(Allocator.TempJob);
                    edgeSet.Clear();
                    edgeSetIsHw.Clear();
                    for (int m = 0; m < nativeArray3.Length; m++)
                    {
                        ValidateAggregate(nativeArray3[m], edgeSet, emptySet, updateMap);
                    }
                    for (int n = 0; n < nativeArray3.Length; n++)
                    {
                        CombineAggregate(nativeArray3[n], nativeArray4[n], updateMap);
                    }
                    nativeArray3.Dispose();
                    nativeArray4.Dispose();
                }
                if (!emptySet.IsEmpty)
                {
                    NativeArray<Entity> nativeArray4 = emptySet.ToNativeArray(Allocator.TempJob);
                    NativeList<AggregateElement> edgeList = new(32, Allocator.TempJob);
                    for (int num2 = 0; num2 < nativeArray4.Length; num2++)
                    {
                        Entity entity3 = nativeArray4[num2];
                        if (emptySet.Contains(entity3))
                        {
                            emptySet.Remove(entity3);
                            CreateAggregate(entity3, emptySet, edgeList, updateMap);
                        }
                    }
                    edgeList.Dispose();
                    nativeArray4.Dispose();
                }
                if (!updateMap.IsEmpty)
                {
                    for (int num3 = 0; num3 < m_Chunks.Length; num3++)
                    {
                        ArchetypeChunk archetypeChunk2 = m_Chunks[num3];
                        if (archetypeChunk2.Has<Temp>())
                        {
                            NativeArray<Entity> nativeArray5 = archetypeChunk2.GetNativeArray(m_EntityType);
                            for (int num4 = 0; num4 < nativeArray5.Length; num4++)
                            {
                                Entity entity4 = nativeArray5[num4];
                                Aggregated aggregated3 = m_AggregatedData[entity4];
                                Entity entity5;
                                if (aggregated3.m_Aggregate != Entity.Null && updateMap.TryGetValue(aggregated3.m_Aggregate, out entity5) && entity5 != aggregated3.m_Aggregate)
                                {
                                    aggregated3.m_Aggregate = entity5;
                                    m_AggregatedData[entity4] = aggregated3;
                                }
                            }
                        }
                    }
                    foreach (KeyValue<Entity, Entity> keyValue in updateMap)
                    {
                        Entity entity6 = keyValue.Value;
                        if (m_AggregateElements.HasBuffer(entity6))
                        {
                            m_CommandBuffer.AddComponent<BatchesUpdated>(entity6, default);
                        }
                    }
                }

                edgeSet.Dispose();
                edgeSetIsHw.Dispose();
                emptySet.Dispose();
                updateMap.Dispose();
            }


            private void CreateAggregate(Entity startEdge, NativeParallelHashSet<Entity> emptySet, NativeList<AggregateElement> edgeList, NativeParallelHashMap<Entity, Entity> updateMap)
            {
                Entity aggregateType = GetAggregateType(startEdge);
                if (aggregateType == Entity.Null)
                {
                    return;
                }
                Edge edge = m_EdgeData[startEdge];
                bool isHighway = IsHighway(m_PrefabRefData[startEdge]);
                AddElements(startEdge, edge.m_Start, true, ref aggregateType, ref isHighway, emptySet, edgeList);
                CollectionUtils.Reverse(edgeList.AsArray());
                AggregateElement aggregateElement = new(startEdge);
                edgeList.Add(aggregateElement);
                AddElements(startEdge, edge.m_End, false, ref aggregateType, ref isHighway, emptySet, edgeList);
                bool flag = m_TempData.HasComponent(startEdge);
                if (!TryCombine(ref aggregateType, ref isHighway, edgeList, flag, updateMap))
                {
                    AggregateNetData aggregateNetData = m_PrefabAggregateData[aggregateType];
                    Entity entity = m_CommandBuffer.CreateEntity(aggregateNetData.m_Archetype);
                    m_CommandBuffer.SetComponent(entity, new PrefabRef(aggregateType));
                    m_CommandBuffer.AddComponent(entity, new ADRRandomizationData());
                    DynamicBuffer<AggregateElement> dynamicBuffer = m_CommandBuffer.SetBuffer<AggregateElement>(entity);
                    if (flag)
                    {
                        m_CommandBuffer.AddComponent(entity, new Temp(Entity.Null, TempFlags.Create));
                    }
                    for (int i = 0; i < edgeList.Length; i++)
                    {
                        AggregateElement aggregateElement2 = edgeList[i];
                        m_CommandBuffer.SetComponent(aggregateElement2.m_Edge, new Aggregated
                        {
                            m_Aggregate = entity
                        });
                        dynamicBuffer.Add(aggregateElement2);
                    }
                }
                edgeList.Clear();
            }

            private bool TryCombine(ref Entity aggType, ref bool isHighway, NativeList<AggregateElement> edgeList, bool isTemp, NativeParallelHashMap<Entity, Entity> updateMap)
            {
                if (GetStart(edgeList.AsArray(), out Entity startEdge, out Entity startNode, out bool isStartNode)
                    && ShouldCombine(startEdge, startNode, isStartNode, ref aggType, ref isHighway, Entity.Null, isTemp, out Entity mainAgg, out bool flag))
                {
                    DynamicBuffer<AggregateElement> thisAggList = m_AggregateElements[mainAgg];
                    int length = thisAggList.Length;
                    thisAggList.ResizeUninitialized(thisAggList.Length + edgeList.Length);
                    if (flag)
                    {
                        for (int i = length - 1; i >= 0; i--)
                        {
                            thisAggList[edgeList.Length + i] = thisAggList[i];
                        }
                    }
                    for (int j = 0; j < edgeList.Length; j++)
                    {
                        AggregateElement aggregateElement = edgeList[j];
                        m_AggregatedData[aggregateElement.m_Edge] = new Aggregated
                        {
                            m_Aggregate = mainAgg
                        };
                        thisAggList[math.select(length, 0, flag) + edgeList.Length - j - 1] = aggregateElement;
                    }
                    m_CommandBuffer.AddComponent<Updated>(mainAgg);
                    if (GetEnd(edgeList.AsArray(), out Entity startEdge2, out Entity startNode2, out bool isStartNode2)
                        && ShouldCombine(startEdge2, startNode2, isStartNode2, ref aggType, ref isHighway, mainAgg, isTemp, out Entity otherAgg, out bool flag2))
                    {
                        DynamicBuffer<AggregateElement> otherAggList = m_AggregateElements[otherAgg];

                        if (thisAggList.Length >= otherAggList.Length)
                        {
                            MergeAggs(updateMap, mainAgg, flag, thisAggList, otherAgg, flag2, otherAggList, false);
                        }
                        else
                        {
                            MergeAggs(updateMap, otherAgg, flag2, otherAggList, mainAgg, flag, thisAggList, true);
                        }
                    }
                    return true;
                }
                if (GetEnd(edgeList.AsArray(), out Entity startEdge3, out Entity startNode3, out bool isStartNode3)
                    && ShouldCombine(startEdge3, startNode3, isStartNode3, ref aggType, ref isHighway, Entity.Null, isTemp, out Entity existingAgg, out bool flag3))
                {
                    DynamicBuffer<AggregateElement> dynamicBuffer3 = m_AggregateElements[existingAgg];
                    int length2 = dynamicBuffer3.Length;
                    dynamicBuffer3.ResizeUninitialized(dynamicBuffer3.Length + edgeList.Length);
                    if (flag3)
                    {
                        for (int m = length2 - 1; m >= 0; m--)
                        {
                            dynamicBuffer3[edgeList.Length + m] = dynamicBuffer3[m];
                        }
                    }
                    for (int n = 0; n < edgeList.Length; n++)
                    {
                        AggregateElement aggregateElement3 = edgeList[n];
                        m_AggregatedData[aggregateElement3.m_Edge] = new Aggregated
                        {
                            m_Aggregate = existingAgg
                        };
                        dynamicBuffer3[math.select(length2, 0, flag3) + n] = aggregateElement3;
                    }
                    m_CommandBuffer.AddComponent<Updated>(existingAgg);
                    return true;
                }
                return false;
            }

            private void MergeAggs(NativeParallelHashMap<Entity, Entity> updateMap, Entity targAgg, bool targAggIsStart, DynamicBuffer<AggregateElement> targAggList,
                Entity srcAgg, bool srcAggIsStart, DynamicBuffer<AggregateElement> srcAggList, bool inverted)
            {
                var length = targAggList.Length;
                targAggList.ResizeUninitialized(srcAggList.Length + targAggList.Length);
                if (targAggIsStart ^ inverted)
                {
                    for (int k = length - 1; k >= 0; k--)
                    {
                        targAggList[srcAggList.Length + k] = targAggList[k];
                    }
                }
                for (int l = 0; l < srcAggList.Length; l++)
                {
                    AggregateElement aggregateElement2 = srcAggList[l];
                    m_AggregatedData[aggregateElement2.m_Edge] = new Aggregated
                    {
                        m_Aggregate = targAgg
                    };
                    targAggList[math.select(length, 0, targAggIsStart ^ inverted) + math.select(l, srcAggList.Length - l - 1, srcAggIsStart == targAggIsStart ^ inverted)] = aggregateElement2;
                }
                srcAggList.Clear();
                m_CommandBuffer.AddComponent<Deleted>(srcAgg);
                if (updateMap.ContainsKey(srcAgg))
                {
                    updateMap[srcAgg] = targAgg;
                }
            }

            private bool GetBestConnectionEdge(ref Entity connectionTypePrefab, ref bool isHighway, Entity prevEdge, Entity prevNode, bool prevIsStart, out Entity nextEdge, out Entity nextNode, out bool nextIsStart)
            {
                Curve curve = m_CurveData[prevEdge];
                float2 directionAngle;
                float2 x;
                float2 xz;

                var refElevation = GetElevationRef(prevEdge);

                if (prevIsStart)
                {
                    directionAngle = math.normalizesafe(-MathUtils.StartTangent(curve.m_Bezier).xz, default);
                    x = math.normalizesafe(curve.m_Bezier.a.xz - curve.m_Bezier.d.xz, default);
                    xz = curve.m_Bezier.a.xz;
                }
                else
                {
                    directionAngle = math.normalizesafe(MathUtils.EndTangent(curve.m_Bezier).xz, default);
                    x = math.normalizesafe(curve.m_Bezier.d.xz - curve.m_Bezier.a.xz, default);
                    xz = curve.m_Bezier.d.xz;
                }
                DynamicBuffer<ConnectedEdge> connEdges = m_ConnectedEdges[prevNode];
                float num = 2f;
                nextEdge = Entity.Null;
                nextNode = Entity.Null;
                nextIsStart = false;
                for (int i = 0; i < connEdges.Length; i++)
                {
                    ConnectedEdge connectedEdge = connEdges[i];
                    if (!(connectedEdge.m_Edge == prevEdge))
                    {
                        var refElConnected = GetElevationRef(connectedEdge.m_Edge);
                        var connectionEdgeType = GetAggregateType(connectedEdge.m_Edge);
                        var isHighwayConnected = IsHighway(m_PrefabRefData[connectedEdge.m_Edge]);
                        if (isHighway || isHighwayConnected || refElConnected == refElevation)
                        {
                            Edge edge = m_EdgeData[connectedEdge.m_Edge];
                            if (edge.m_Start == prevNode)
                            {
                                if (connectionEdgeType == connectionTypePrefab || isHighway || isHighwayConnected)
                                {
                                    Curve curve2 = m_CurveData[connectedEdge.m_Edge];
                                    float2 float2 = math.normalizesafe(-MathUtils.StartTangent(curve2.m_Bezier).xz, default);
                                    float2 y = math.normalizesafe(curve2.m_Bezier.a.xz - curve2.m_Bezier.d.xz, default);
                                    float2 x2 = curve2.m_Bezier.a.xz - xz;
                                    float num2 = math.abs(math.dot(x2, MathUtils.Right(directionAngle))) + math.abs(math.dot(x2, MathUtils.Right(float2)));
                                    num2 = 0.5f - 0.5f / (1f + num2 * 0.1f);
                                    float num3 = math.dot(directionAngle, float2) + math.dot(x, y) * 0.5f + num2;
                                    if (num3 < num)
                                    {
                                        num = num3;
                                        nextEdge = connectedEdge.m_Edge;
                                        nextNode = edge.m_End;
                                        nextIsStart = false;
                                    }
                                    if (isHighwayConnected && !isHighway)
                                    {
                                        isHighway = isHighwayConnected;
                                        connectionTypePrefab = connectionEdgeType;
                                    }
                                }
                            }
                            else if (edge.m_End == prevNode)
                            {
                                if (connectionEdgeType == connectionTypePrefab || isHighway || isHighwayConnected)
                                {

                                    Curve curve3 = m_CurveData[connectedEdge.m_Edge];
                                    float2 float3 = math.normalizesafe(MathUtils.EndTangent(curve3.m_Bezier).xz, default);
                                    float2 y2 = math.normalizesafe(curve3.m_Bezier.d.xz - curve3.m_Bezier.a.xz, default);
                                    float2 x3 = curve3.m_Bezier.d.xz - xz;
                                    float num4 = math.abs(math.dot(x3, MathUtils.Right(directionAngle))) + math.abs(math.dot(x3, MathUtils.Right(float3)));
                                    num4 = 0.5f - 0.5f / (1f + num4 * 0.1f);
                                    float num5 = math.dot(directionAngle, float3) + math.dot(x, y2) * 0.5f + num4;
                                    if (num5 < num)
                                    {
                                        num = num5;
                                        nextEdge = connectedEdge.m_Edge;
                                        nextNode = edge.m_Start;
                                        nextIsStart = true;
                                    }
                                    if (isHighwayConnected && !isHighway)
                                    {
                                        isHighway = isHighwayConnected;
                                        connectionTypePrefab = connectionEdgeType;
                                    }
                                }
                            }
                        }
                    }
                }
                return nextEdge != Entity.Null;
            }


            private void AddElements(Entity startEdge, Entity startNode, bool isStartNode, ref Entity aggType, ref bool isHighway, NativeParallelHashSet<Entity> emptySet, NativeList<AggregateElement> elements)
            {
                while (GetBestConnectionEdge(ref aggType, ref isHighway, startEdge, startNode, isStartNode, out Entity entity, out Entity entity2, out bool flag)
                    && GetBestConnectionEdge(ref aggType, ref isHighway, entity, startNode, !flag, out Entity lhs, out _, out _)
                    && lhs == startEdge
                    && emptySet.Contains(entity))
                {
                    AggregateElement aggregateElement = new(entity);
                    elements.Add(aggregateElement);
                    emptySet.Remove(entity);
                    startEdge = entity;
                    startNode = entity2;
                    isStartNode = flag;
                }
            }


            private void CombineAggregate(Entity aggregate, bool isHighway, NativeParallelHashMap<Entity, Entity> updateMap)
            {
                DynamicBuffer<AggregateElement> dynamicBuffer = m_AggregateElements[aggregate];
                Entity prefab = m_PrefabRefData[aggregate].m_Prefab;
                bool isTemp = m_TempData.HasComponent(aggregate);
                while (GetStart(dynamicBuffer.AsNativeArray(), out Entity startEdge, out Entity startNode, out bool isStartNode)
                    && ShouldCombine(startEdge, startNode, isStartNode, ref prefab, ref isHighway, aggregate, isTemp, out Entity entity, out bool c))
                {
                    DynamicBuffer<AggregateElement> dynamicBuffer2 = m_AggregateElements[entity];
                    int length = dynamicBuffer.Length;
                    dynamicBuffer.ResizeUninitialized(dynamicBuffer2.Length + dynamicBuffer.Length);
                    for (int i = length - 1; i >= 0; i--)
                    {
                        dynamicBuffer[dynamicBuffer2.Length + i] = dynamicBuffer[i];
                    }
                    for (int j = 0; j < dynamicBuffer2.Length; j++)
                    {
                        AggregateElement aggregateElement = dynamicBuffer2[j];
                        m_AggregatedData[aggregateElement.m_Edge] = new Aggregated
                        {
                            m_Aggregate = aggregate
                        };
                        dynamicBuffer[math.select(j, dynamicBuffer2.Length - j - 1, c)] = aggregateElement;
                    }
                    dynamicBuffer2.Clear();
                    m_CommandBuffer.AddComponent<Deleted>(entity);
                    if (updateMap.ContainsKey(entity))
                    {
                        updateMap[entity] = aggregate;
                    }
                }
                while (GetEnd(dynamicBuffer.AsNativeArray(), out Entity startEdge2, out Entity startNode2, out bool isStartNode2)
                    && ShouldCombine(startEdge2, startNode2, isStartNode2, ref prefab, ref isHighway, aggregate, isTemp, out Entity entity2, out bool c2))
                {
                    DynamicBuffer<AggregateElement> dynamicBuffer3 = m_AggregateElements[entity2];
                    int length2 = dynamicBuffer.Length;
                    dynamicBuffer.ResizeUninitialized(dynamicBuffer3.Length + dynamicBuffer.Length);
                    for (int k = 0; k < dynamicBuffer3.Length; k++)
                    {
                        AggregateElement aggregateElement2 = dynamicBuffer3[k];
                        m_AggregatedData[aggregateElement2.m_Edge] = new Aggregated
                        {
                            m_Aggregate = aggregate
                        };
                        dynamicBuffer[length2 + math.select(k, dynamicBuffer3.Length - k - 1, c2)] = aggregateElement2;
                    }
                    dynamicBuffer3.Clear();
                    m_CommandBuffer.AddComponent<Deleted>(entity2);
                    if (updateMap.ContainsKey(entity2))
                    {
                        updateMap[entity2] = aggregate;
                    }
                }
            }


            private bool GetStart(NativeArray<AggregateElement> elements, out Entity edge, out Entity node, out bool isStart)
            {
                if (elements.Length == 0)
                {
                    edge = Entity.Null;
                    node = Entity.Null;
                    isStart = false;
                    return false;
                }
                if (elements.Length == 1)
                {
                    edge = elements[0].m_Edge;
                    Edge edge2 = m_EdgeData[edge];
                    node = edge2.m_Start;
                    isStart = true;
                    return true;
                }
                edge = elements[0].m_Edge;
                Entity edge3 = elements[1].m_Edge;
                Edge edge4 = m_EdgeData[edge];
                Edge edge5 = m_EdgeData[edge3];
                if (edge4.m_End == edge5.m_Start || edge4.m_End == edge5.m_End)
                {
                    node = edge4.m_Start;
                    isStart = true;
                }
                else
                {
                    node = edge4.m_End;
                    isStart = false;
                }
                return true;
            }


            private bool GetEnd(NativeArray<AggregateElement> elements, out Entity edge, out Entity node, out bool isStart)
            {
                if (elements.Length == 0)
                {
                    edge = Entity.Null;
                    node = Entity.Null;
                    isStart = false;
                    return false;
                }
                if (elements.Length == 1)
                {
                    edge = elements[0].m_Edge;
                    Edge edge2 = m_EdgeData[edge];
                    node = edge2.m_End;
                    isStart = false;
                    return true;
                }
                edge = elements[^1].m_Edge;
                Entity edge3 = elements[^2].m_Edge;
                Edge edge4 = m_EdgeData[edge];
                Edge edge5 = m_EdgeData[edge3];
                if (edge4.m_End == edge5.m_Start || edge4.m_End == edge5.m_End)
                {
                    node = edge4.m_Start;
                    isStart = true;
                }
                else
                {
                    node = edge4.m_End;
                    isStart = false;
                }
                return true;
            }


            private void ValidateAggregate(Entity aggregate, NativeParallelHashSet<Entity> edgeSet, NativeParallelHashSet<Entity> emptySet, NativeParallelHashMap<Entity, Entity> updateMap)
            {
                DynamicBuffer<AggregateElement> elements = m_AggregateElements[aggregate];
                Entity refEdge = Entity.Null;
                Entity aggTypePrefab = m_PrefabRefData[aggregate].m_Prefab;
                ADRAggregationData.ElevationType elRefEdge = 0;
                bool refIsHighway = false;
                for (int i = 0; i < elements.Length; i++)
                {
                    AggregateElement aggregateElement = elements[i];
                    if (!m_DeletedData.HasComponent(aggregateElement.m_Edge))
                    {
                        var aggType = GetAggregateType(aggregateElement.m_Edge);
                        var elCurrent = GetElevationRef(aggregateElement.m_Edge);
                        var isHighway = refIsHighway || IsHighway(m_PrefabRefData[aggregateElement.m_Edge]);
                        if (aggType != aggTypePrefab && ((!isHighway && elCurrent != elRefEdge) || !isHighway))
                        {
                            emptySet.Add(aggregateElement.m_Edge);
                            m_AggregatedData[aggregateElement.m_Edge] = default;
                        }
                        else if (refEdge == Entity.Null)
                        {
                            refEdge = aggregateElement.m_Edge;
                            elRefEdge = elCurrent;
                            refIsHighway = isHighway;
                        }
                        else
                        {
                            edgeSet.Add(aggregateElement.m_Edge);
                        }
                        if (refIsHighway != isHighway)
                        {
                            aggTypePrefab = aggType;
                            refIsHighway = isHighway;
                        }
                    }
                }
                elements.Clear();
                if (refEdge == Entity.Null)
                {
                    m_CommandBuffer.AddComponent<Deleted>(aggregate);
                    if (updateMap.ContainsKey(aggregate))
                    {
                        updateMap[aggregate] = Entity.Null;
                    }
                }
                else
                {
                    Edge edge = m_EdgeData[refEdge];
                    AddElements(refEdge, edge.m_Start, true, ref aggTypePrefab, ref refIsHighway, edgeSet, elements);
                    CollectionUtils.Reverse<AggregateElement>(elements.AsNativeArray());
                    elements.Add(new AggregateElement(refEdge));
                    AddElements(refEdge, edge.m_End, false, ref aggTypePrefab, ref refIsHighway, edgeSet, elements);
                    m_CommandBuffer.AddComponent<Updated>(aggregate);
                }
                if (!edgeSet.IsEmpty)
                {
                    NativeArray<Entity> nativeArray = edgeSet.ToNativeArray(Allocator.Temp);
                    for (int j = 0; j < nativeArray.Length; j++)
                    {
                        Entity entity2 = nativeArray[j];
                        emptySet.Add(entity2);
                        m_AggregatedData[entity2] = default;
                    }
                    nativeArray.Dispose();
                    edgeSet.Clear();
                }
            }


            private bool ShouldCombine(Entity startEdge, Entity startNode, bool isStartNode, ref Entity aggregationType, ref bool isHighway, Entity aggregate, bool isTemp, out Entity otherAggregate, out bool otherIsStart)
            {
                if (GetBestConnectionEdge(ref aggregationType, ref isHighway, startEdge, startNode, isStartNode, out Entity nextEdge, out _, out bool flag)
                    && GetBestConnectionEdge(ref aggregationType, ref isHighway, nextEdge, startNode, !flag, out Entity lhs, out _, out _)
                    && lhs == startEdge
                    && m_AggregatedData.HasComponent(nextEdge))
                {
                    Aggregated aggregated = m_AggregatedData[nextEdge];
                    if (aggregated.m_Aggregate != aggregate
                        && m_AggregateElements.HasBuffer(aggregated.m_Aggregate)
                        && m_TempData.HasComponent(aggregated.m_Aggregate) == isTemp)
                    {
                        DynamicBuffer<AggregateElement> dynamicBuffer = m_AggregateElements[aggregated.m_Aggregate];
                        if (dynamicBuffer[0].m_Edge == nextEdge)
                        {
                            otherAggregate = aggregated.m_Aggregate;
                            otherIsStart = true;
                            return true;
                        }
                        if (dynamicBuffer[^1].m_Edge == nextEdge)
                        {
                            otherAggregate = aggregated.m_Aggregate;
                            otherIsStart = false;
                            return true;
                        }
                    }
                }
                otherAggregate = Entity.Null;
                otherIsStart = false;
                return false;
            }


            private void AddElements(Entity startEdge, Entity startNode, bool isStartNode, ref Entity aggregationType, ref bool isHighway, NativeParallelHashSet<Entity> edgeSet, DynamicBuffer<AggregateElement> elements)
            {
                while (GetBestConnectionEdge(ref aggregationType, ref isHighway, startEdge, startNode, isStartNode, out Entity nextEdge, out Entity nextNode, out bool nextIsStart)
                    && GetBestConnectionEdge(ref aggregationType, ref isHighway, nextEdge, startNode, !nextIsStart, out Entity lhs, out _, out _) && lhs == startEdge && edgeSet.Contains(nextEdge))
                {
                    elements.Add(new AggregateElement(nextEdge));
                    edgeSet.Remove(nextEdge);
                    startEdge = nextEdge;
                    startNode = nextNode;
                    isStartNode = nextIsStart;
                }
            }


            private Entity GetAggregateType(Entity edge)
            {
                PrefabRef prefabRef = m_PrefabRefData[edge];
                return m_PrefabGeometryData[prefabRef.m_Prefab].m_AggregateType;
            }

            private bool IsHighway(PrefabRef type)
            {
                return (m_RoadData[type.m_Prefab].m_Flags & Game.Prefabs.RoadFlags.UseHighwayRules) != 0;
            }

            private ADRAggregationData.ElevationType GetElevationRef(Entity segmentEntity)
            {
                var elevation = m_ElevationData[segmentEntity].m_Elevation;
                return (ADRAggregationData.ElevationType)math.sign(math.trunc((elevation.x + elevation.y) * (.5f / 7.5f)));
            }
        }
    }
}
#endif