using Colossal.Mathematics;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-03: Port of AggregateMeshSystem.UpdateLabelPositionsJob / AddLabels with dual membership.
    /// S1 = AggregateElement (skip ADR-associated edges); S2 = ADRNameAggregateElement.
    /// Re-diff baseline GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public static class AdrRoadNameLabelPlacementJobs
    {
        private const float SpacingUnit = 100f;

#if BURST
        [BurstCompile]
#endif
        public struct UpdateS1LabelPositionsJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LabelExtents> m_LabelExtentsType;
            [ReadOnly] public BufferTypeHandle<AggregateElement> m_AggregateElementType;
            public BufferTypeHandle<LabelPosition> m_LabelPositionType;
            [ReadOnly] public ComponentLookup<Edge> m_EdgeData;
            [ReadOnly] public ComponentLookup<Curve> m_CurveData;
            [ReadOnly] public ComponentLookup<Composition> m_CompositionData;
            [ReadOnly] public ComponentLookup<NetCompositionData> m_NetCompositionData;
            [ReadOnly] public BufferLookup<ConnectedEdge> m_ConnectedEdges;
            [ReadOnly] public ComponentLookup<ADRNameAggregated> m_AdrAggregated;
            [ReadOnly] public ComponentTypeHandle<Temp> m_TempType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<LabelExtents> extents = chunk.GetNativeArray(ref m_LabelExtentsType);
                BufferAccessor<AggregateElement> elements = chunk.GetBufferAccessor(ref m_AggregateElementType);
                BufferAccessor<LabelPosition> positions = chunk.GetBufferAccessor(ref m_LabelPositionType);
                NativeArray<Temp> temps = chunk.GetNativeArray(ref m_TempType);
                // Selection Temp Aggregate (GenerateAggregates) must place along its full membership,
                // including ADR-associated edges — otherwise S2 select hides original (Hidden) and Temp
                // gets zero labels (vanilla parity: labels live on the Temp Aggregate).
                bool includeAdrEdges = temps.Length != 0
                    && (temps[0].m_Flags & (TempFlags.Create | TempFlags.Delete | TempFlags.Select | TempFlags.Modify | TempFlags.Replace)) != 0;
                NativeList<Entity> edgeList = default;
                NativeList<float> redundancyBuffer = default;

                for (int i = 0; i < extents.Length; i++)
                {
                    DynamicBuffer<AggregateElement> aggregateElements = elements[i];
                    DynamicBuffer<LabelPosition> labelPositions = positions[i];
                    labelPositions.Clear();
                    if (aggregateElements.Length <= 0)
                    {
                        continue;
                    }

                    if (!edgeList.IsCreated)
                    {
                        edgeList = new NativeList<Entity>(aggregateElements.Length, Allocator.Temp);
                    }
                    else
                    {
                        edgeList.Clear();
                    }

                    for (int e = 0; e < aggregateElements.Length; e++)
                    {
                        Entity edge = aggregateElements[e].m_Edge;
                        if (includeAdrEdges || !m_AdrAggregated.HasComponent(edge))
                        {
                            edgeList.Add(edge);
                        }
                    }

                    if (edgeList.Length <= 0)
                    {
                        continue;
                    }

                    if (!redundancyBuffer.IsCreated)
                    {
                        redundancyBuffer = new NativeList<float>(32, Allocator.Temp);
                    }

                    PlaceAlongEdges(extents[i], edgeList, labelPositions, ref redundancyBuffer);
                }

                if (edgeList.IsCreated)
                {
                    edgeList.Dispose();
                }
                if (redundancyBuffer.IsCreated)
                {
                    redundancyBuffer.Dispose();
                }
            }

            private void PlaceAlongEdges(
                LabelExtents labelExtents,
                NativeList<Entity> edges,
                DynamicBuffer<LabelPosition> labelPositions,
                ref NativeList<float> redundancyBuffer)
            {
                int startIndex = 0;
                int j = 1;
                Edge edge = m_EdgeData[edges[0]];
                for (; j < edges.Length; j++)
                {
                    Edge edge2 = m_EdgeData[edges[j]];
                    if (edge2.m_Start == edge.m_End || edge2.m_Start == edge.m_Start)
                    {
                        if (m_ConnectedEdges[edge2.m_Start].Length > 2)
                        {
                            AddLabels(startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions);
                            startIndex = j;
                        }
                    }
                    else if (edge2.m_End == edge.m_End || edge2.m_End == edge.m_Start)
                    {
                        if (m_ConnectedEdges[edge2.m_End].Length > 2)
                        {
                            AddLabels(startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions);
                            startIndex = j;
                        }
                    }
                    else
                    {
                        AddLabels(startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions);
                        startIndex = j;
                    }
                    edge = edge2;
                }

                AddLabels(startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions);
                ApplyRedundancy(labelPositions, redundancyBuffer);
                redundancyBuffer.Clear();
            }

            private void AddLabels(
                int startIndex,
                int endIndex,
                LabelExtents labelExtents,
                NativeList<float> redundancyBuffer,
                NativeList<Entity> edges,
                DynamicBuffer<LabelPosition> labelPositions)
            {
                AdrLabelPlacementMath.AddLabels(
                    startIndex, endIndex, labelExtents, redundancyBuffer, edges, labelPositions,
                    ref m_CurveData, ref m_CompositionData, ref m_NetCompositionData);
            }

            private static void ApplyRedundancy(DynamicBuffer<LabelPosition> labelPositions, NativeList<float> redundancyBuffer)
            {
                AdrLabelPlacementMath.ApplyRedundancy(labelPositions, redundancyBuffer);
            }
        }

#if BURST
        [BurstCompile]
#endif
        public struct UpdateS2LabelPositionsJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LabelExtents> m_LabelExtentsType;
            [ReadOnly] public BufferTypeHandle<ADRNameAggregateElement> m_AdrElementType;
            public BufferTypeHandle<LabelPosition> m_LabelPositionType;
            [ReadOnly] public ComponentLookup<Edge> m_EdgeData;
            [ReadOnly] public ComponentLookup<Curve> m_CurveData;
            [ReadOnly] public ComponentLookup<Composition> m_CompositionData;
            [ReadOnly] public ComponentLookup<NetCompositionData> m_NetCompositionData;
            [ReadOnly] public BufferLookup<ConnectedEdge> m_ConnectedEdges;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<LabelExtents> extents = chunk.GetNativeArray(ref m_LabelExtentsType);
                BufferAccessor<ADRNameAggregateElement> elements = chunk.GetBufferAccessor(ref m_AdrElementType);
                BufferAccessor<LabelPosition> positions = chunk.GetBufferAccessor(ref m_LabelPositionType);
                NativeList<Entity> edgeList = default;
                NativeList<float> redundancyBuffer = default;

                for (int i = 0; i < extents.Length; i++)
                {
                    DynamicBuffer<ADRNameAggregateElement> adrElements = elements[i];
                    DynamicBuffer<LabelPosition> labelPositions = positions[i];
                    labelPositions.Clear();
                    if (adrElements.Length <= 0)
                    {
                        continue;
                    }

                    if (!edgeList.IsCreated)
                    {
                        edgeList = new NativeList<Entity>(adrElements.Length, Allocator.Temp);
                    }
                    else
                    {
                        edgeList.Clear();
                    }

                    for (int e = 0; e < adrElements.Length; e++)
                    {
                        edgeList.Add(adrElements[e].m_Edge);
                    }

                    if (!redundancyBuffer.IsCreated)
                    {
                        redundancyBuffer = new NativeList<float>(32, Allocator.Temp);
                    }

                    PlaceAlongEdges(extents[i], edgeList, labelPositions, ref redundancyBuffer);
                }

                if (edgeList.IsCreated)
                {
                    edgeList.Dispose();
                }
                if (redundancyBuffer.IsCreated)
                {
                    redundancyBuffer.Dispose();
                }
            }

            private void PlaceAlongEdges(
                LabelExtents labelExtents,
                NativeList<Entity> edges,
                DynamicBuffer<LabelPosition> labelPositions,
                ref NativeList<float> redundancyBuffer)
            {
                int startIndex = 0;
                int j = 1;
                Edge edge = m_EdgeData[edges[0]];
                for (; j < edges.Length; j++)
                {
                    Edge edge2 = m_EdgeData[edges[j]];
                    if (edge2.m_Start == edge.m_End || edge2.m_Start == edge.m_Start)
                    {
                        if (m_ConnectedEdges[edge2.m_Start].Length > 2)
                        {
                            AdrLabelPlacementMath.AddLabels(
                                startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions,
                                ref m_CurveData, ref m_CompositionData, ref m_NetCompositionData);
                            startIndex = j;
                        }
                    }
                    else if (edge2.m_End == edge.m_End || edge2.m_End == edge.m_Start)
                    {
                        if (m_ConnectedEdges[edge2.m_End].Length > 2)
                        {
                            AdrLabelPlacementMath.AddLabels(
                                startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions,
                                ref m_CurveData, ref m_CompositionData, ref m_NetCompositionData);
                            startIndex = j;
                        }
                    }
                    else
                    {
                        AdrLabelPlacementMath.AddLabels(
                            startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions,
                            ref m_CurveData, ref m_CompositionData, ref m_NetCompositionData);
                        startIndex = j;
                    }
                    edge = edge2;
                }

                AdrLabelPlacementMath.AddLabels(
                    startIndex, j, labelExtents, redundancyBuffer, edges, labelPositions,
                    ref m_CurveData, ref m_CompositionData, ref m_NetCompositionData);
                AdrLabelPlacementMath.ApplyRedundancy(labelPositions, redundancyBuffer);
                redundancyBuffer.Clear();
            }
        }

        /// <summary>Shared AddLabels / redundancy math from vanilla AggregateMeshSystem.</summary>
        public static class AdrLabelPlacementMath
        {
            public static void AddLabels(
                int startIndex,
                int endIndex,
                LabelExtents labelExtents,
                NativeList<float> redundancyBuffer,
                NativeList<Entity> edges,
                DynamicBuffer<LabelPosition> labelPositions,
                ref ComponentLookup<Curve> curveData,
                ref ComponentLookup<Composition> compositionData,
                ref ComponentLookup<NetCompositionData> netCompositionData)
            {
                if (endIndex <= startIndex)
                {
                    return;
                }

                float num = 0f;
                for (int i = startIndex; i < endIndex; i++)
                {
                    Entity edge = edges[i];
                    Curve curve = curveData[edge];
                    Composition composition = compositionData[edge];
                    float num2 = math.sqrt(math.max(1f, netCompositionData[composition.m_Edge].m_Width));
                    num += curve.m_Length / num2;
                }

                float num3 = SpacingUnit;
                int num4 = math.clamp((int)math.round(num / num3), 1, endIndex - startIndex);
                num3 = num / num4;
                float value = num3 / SpacingUnit;
                float num5 = 0f;
                float num6 = 0f;
                int num7 = -1;
                LabelPosition elem = default;

                for (int j = startIndex; j < endIndex; j++)
                {
                    Entity edge2 = edges[j];
                    Curve curve2 = curveData[edge2];
                    Composition composition2 = compositionData[edge2];
                    float num8 = math.sqrt(math.max(1f, netCompositionData[composition2.m_Edge].m_Width));
                    float num9 = num5 + curve2.m_Length / num8;
                    if (num7 != -1 && num9 - num3 > num3 - num5)
                    {
                        Entity edge3 = edges[num7];
                        Curve curve3 = curveData[edge3];
                        Composition composition3 = compositionData[edge3];
                        NetCompositionData netCompositionData1 = netCompositionData[composition3.m_Edge];
                        elem.m_Curve = curve3.m_Bezier;
                        elem.m_ElementIndex = j;
                        elem.m_HalfLength = curve3.m_Length * 0.5f;
                        elem.m_MaxScale = netCompositionData1.m_Width * 0.5f
                            / math.max(1f, math.max(0f - labelExtents.m_Bounds.min.y, labelExtents.m_Bounds.max.y));
                        elem.m_IsUnderground = (netCompositionData1.m_Flags.m_General & CompositionFlags.General.Tunnel) != 0;
                        labelPositions.Add(elem);
                        redundancyBuffer.Add(in value);
                        num5 -= num3;
                        num9 -= num3;
                        num6 = 0f;
                        num7 = -1;
                    }

                    float num10 = math.lerp(num5, num9, 0.5f);
                    float num11 = curve2.m_Length * num8 / math.max(1f, num3 + math.abs(num10 - num3 * 0.5f));
                    num5 = num9;
                    if (num11 > num6)
                    {
                        num6 = num11;
                        num7 = j;
                    }
                }

                if (num7 != -1)
                {
                    Entity edge4 = edges[num7];
                    Curve curve4 = curveData[edge4];
                    Composition composition4 = compositionData[edge4];
                    NetCompositionData netCompositionData2 = netCompositionData[composition4.m_Edge];
                    LabelPosition elem2 = default;
                    elem2.m_Curve = curve4.m_Bezier;
                    elem2.m_ElementIndex = num7;
                    elem2.m_HalfLength = curve4.m_Length * 0.5f;
                    elem2.m_MaxScale = netCompositionData2.m_Width * 0.5f
                        / math.max(1f, math.max(0f - labelExtents.m_Bounds.min.y, labelExtents.m_Bounds.max.y));
                    elem2.m_IsUnderground = (netCompositionData2.m_Flags.m_General & CompositionFlags.General.Tunnel) != 0;
                    labelPositions.Add(elem2);
                    redundancyBuffer.Add(in value);
                }
            }

            public static void ApplyRedundancy(DynamicBuffer<LabelPosition> labelPositions, NativeList<float> redundancyBuffer)
            {
                float num = 0f;
                for (int k = 0; k < redundancyBuffer.Length; k++)
                {
                    num += redundancyBuffer[k];
                }

                float num2 = (math.max(1f, math.round(num)) - num) * 0.5f;
                float num3 = 0f;
                for (int l = 0; l < redundancyBuffer.Length; l++)
                {
                    float num4 = redundancyBuffer[l];
                    num2 += num4;
                    if (num2 < 0.5f)
                    {
                        LabelPosition value = labelPositions[l];
                        num3 += num4;
                        if (num3 < 0.25f)
                        {
                            value.m_MaxScale *= 0.25f;
                        }
                        else
                        {
                            value.m_MaxScale *= 0.5f;
                            num3 = num4 < 0.25f ? num3 - 0.5f : 0f;
                        }
                        labelPositions[l] = value;
                    }
                    else
                    {
                        num2 -= 1f;
                        num3 = 0f;
                    }
                }
            }
        }
    }
}
