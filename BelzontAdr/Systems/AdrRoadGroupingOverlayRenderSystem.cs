using System;
using System.Collections.Generic;
using Colossal.Entities;
using Colossal.Mathematics;
using Game;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace BelzontAdr
{
    /// <summary>
    /// Membership visualization via the <b>exact</b> transport-route GPU path
    /// (<see cref="RouteBufferSystem"/> SegmentData + <see cref="RouteRenderSystem"/> mesh/material/draw),
    /// not OverlayRenderSystem.DrawCurve.
    /// </summary>
    public partial class AdrRoadGroupingOverlayRenderSystem : GameSystemBase
    {
        /// <summary>
        /// Mirror of private unmarked <c>RouteBufferSystem.SegmentData</c>.
        /// Stride must be <c>UnsafeUtility.SizeOf</c> / <c>sizeof</c> — do not force Size=112.
        /// </summary>
        private struct SegmentData
        {
            public float4x4 m_Curve;
            public float3 m_Position;
            public float3 m_SizeFactor;
            public float2 m_Opacity;
            public float2 m_DividedOpacity;
            public float m_Broken;
        }

        private static readonly Color BelongingColor = new(0f, 1f, 0f, 1f);
        private static readonly Color HoverPreviewColor = new(0f, 0.4f, 0f, 1f);

        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private AdrMainSystem m_AdrMainSystem;
        private AdrHighwayRoutesSystem m_HighwayRoutesSystem;
        private AdrRoadGroupingTool m_Tool;
        private AdrRoadGroupingController m_Controller;
        private EntityQuery m_RouteConfigQuery;

        private Mesh m_Mesh;
        private Material m_Material;
        private int m_OriginalRenderQueue;
        private Vector4 m_RouteSize;
        private ComputeBuffer m_SegmentBuffer;
        private ComputeBuffer m_ArgsBuffer;
        private readonly List<uint> m_ArgsArray = new(5);
        private NativeList<SegmentData> m_Segments;
        private Bounds3 m_Bounds;
        private Color m_DrawColor;
        private bool m_HasDraw;

        private static readonly int RouteSegmentBufferId = Shader.PropertyToID("colossal_RouteSegmentBuffer");
        private static readonly int RouteColorId = Shader.PropertyToID("colossal_RouteColor");
        private static readonly int RouteSizeId = Shader.PropertyToID("colossal_RouteSize");

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_AdrMainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
            m_HighwayRoutesSystem = World.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();
            m_Tool = World.GetOrCreateSystemManaged<AdrRoadGroupingTool>();
            m_Controller = World.GetOrCreateSystemManaged<AdrRoadGroupingController>();
            m_RouteConfigQuery = GetEntityQuery(ComponentType.ReadOnly<RouteConfigurationData>());
            m_Segments = new NativeList<SegmentData>(64, Allocator.Persistent);
            m_Bounds = new Bounds3(float.MaxValue, float.MinValue);
            RenderPipelineManager.beginContextRendering += Render;
        }

        protected override void OnDestroy()
        {
            RenderPipelineManager.beginContextRendering -= Render;
            if (m_Segments.IsCreated)
            {
                m_Segments.Dispose();
            }

            if (m_Mesh != null)
            {
                UnityEngine.Object.Destroy(m_Mesh);
            }

            if (m_Material != null)
            {
                UnityEngine.Object.Destroy(m_Material);
            }

            m_SegmentBuffer?.Release();
            m_ArgsBuffer?.Release();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            m_HasDraw = false;
            m_Segments.Clear();
            m_Bounds = new Bounds3(float.MaxValue, float.MinValue);

            if (m_ToolSystem.activeTool != m_Tool)
            {
                return;
            }

            var selected = m_Controller.SelectedAggregation?.Value ?? Entity.Null;
            Entity drawAggregate = selected;
            m_DrawColor = BelongingColor;
            bool drawEndNodes = true;

            if (drawAggregate == Entity.Null)
            {
                if (!TryResolveHoveredAggregate(out drawAggregate))
                {
                    return;
                }

                m_DrawColor = HoverPreviewColor;
                drawEndNodes = false;
            }

            if (!EntityManager.TryGetBuffer(drawAggregate, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                return;
            }

            EnsureMaterial();
            if (m_Material == null)
            {
                return;
            }

            BuildRouteSegments(drawAggregate, elements, drawEndNodes);
            m_HasDraw = m_Segments.Length > 0;
        }

        private void BuildRouteSegments(Entity aggregate, DynamicBuffer<AggregateElement> elements, bool drawEndNodes)
        {
            var nodeDegrees = new NativeHashMap<Entity, int>(elements.Length * 2, Allocator.Temp);
            try
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (!EntityManager.TryGetComponent(elements[i].m_Edge, out Edge edge))
                    {
                        continue;
                    }

                    IncrementDegree(ref nodeDegrees, edge.m_Start);
                    IncrementDegree(ref nodeDegrees, edge.m_End);
                }

                // Same direction source as BuildingUtilsOverrides.GetAddress_Override:
                // isInverseAggregate (ends vs zero/highway marker) XOR reverseCounting cache.
                ResolveNumberingDirection(aggregate, elements, out bool isInverseAggregate, out bool reverseCounting);

                float distance = 0f;
                int prevSegmentIndex = -1;

                for (int i = 0; i < elements.Length; i++)
                {
                    var edgeEntity = elements[i].m_Edge;
                    if (!EntityManager.TryGetComponent(edgeEntity, out Curve curve)
                        || !EntityManager.TryGetComponent(edgeEntity, out Edge edge))
                    {
                        continue;
                    }

                    bool inverted = IsNumberingSegmentInverted(elements, i, edgeEntity, isInverseAggregate, reverseCounting);
                    Bezier4x3 bezier = curve.m_Bezier;
                    if (inverted)
                    {
                        bezier = MathUtils.Invert(bezier);
                    }

                    SnapBezierEndsToNodes(ref bezier, edge, inverted);

                    // Membership overlay: always full end opacity. Route continuity fade
                    // (opacity.x→0 on tangent/gap break) left visible joint gaps after node snaps.
                    float2 opacity = new float2(1f, 1f);

                    float length = MathUtils.Length(bezier);
                    if (length < 0.01f)
                    {
                        continue;
                    }

                    float2 offset = new float2(distance, distance + length);
                    int segmentIndex = AddSegment(bezier, offset, opacity, broken: 0f);
                    distance = offset.y;

                    if (prevSegmentIndex >= 0 && segmentIndex >= 0)
                    {
                        // RouteBufferSystem joint size-factor match between consecutive segments.
                        ref SegmentData prev = ref m_Segments.ElementAt(prevSegmentIndex);
                        ref SegmentData cur = ref m_Segments.ElementAt(segmentIndex);
                        float joint = math.max(prev.m_SizeFactor.z, cur.m_SizeFactor.x);
                        prev.m_SizeFactor.z = joint;
                        cur.m_SizeFactor.x = joint;
                        // Keep DividedOpacity at 1 — do not reintroduce joint fade for overlay.
                    }

                    prevSegmentIndex = segmentIndex;
                }

                DrawNodeMarkers(elements, nodeDegrees, drawEndNodes);

                // RouteBufferSystem saturates opacity after joint pass.
                for (int s = 0; s < m_Segments.Length; s++)
                {
                    ref SegmentData seg = ref m_Segments.ElementAt(s);
                    seg.m_Opacity = math.saturate(seg.m_Opacity);
                }
            }
            finally
            {
                nodeDegrees.Dispose();
            }
        }

        /// <summary>
        /// Absolute Overlay circles at node height.
        /// End / drag handles: AggregateElement chain termini (always when selected — including loops).
        /// Joint pads: same DrawCircle path at half end diameter on multi-edge nodes.
        /// </summary>
        private void DrawNodeMarkers(
            DynamicBuffer<AggregateElement> elements,
            NativeHashMap<Entity, int> nodeDegrees,
            bool drawEndNodes)
        {
            // Highlighted route draw uses size.x *= 4/3; end handles are 2× that, joints half of ends.
            float baseSize = m_RouteSize.x * (4f / 3f);
            if (baseSize < 0.01f)
            {
                return;
            }

            float endSize = baseSize * 2f;
            float jointSize = endSize * 0.5f;

            JobHandle dependencies;
            OverlayRenderSystem.Buffer buffer = m_OverlayRenderSystem.GetBuffer(out dependencies);
            dependencies.Complete();

            var endHandles = new NativeHashSet<Entity>(4, Allocator.Temp);
            try
            {
                if (drawEndNodes && elements.Length > 0)
                {
                    endHandles.Add(GetChainStartNode(elements));
                    endHandles.Add(GetChainEndNode(elements));
                    foreach (Entity endEntity in endHandles)
                    {
                        if (endEntity != Entity.Null && EntityManager.TryGetComponent(endEntity, out Node endNode))
                        {
                            buffer.DrawCircle(m_DrawColor, endNode.m_Position, endSize);
                        }
                    }
                }

                foreach (var pair in nodeDegrees)
                {
                    if (endHandles.Contains(pair.Key) || !EntityManager.TryGetComponent(pair.Key, out Node node))
                    {
                        continue;
                    }

                    if (pair.Value >= 2)
                    {
                        buffer.DrawCircle(m_DrawColor, node.m_Position, jointSize);
                    }
                    else if (drawEndNodes && pair.Value == 1)
                    {
                        // Extra dead-end not captured as buffer terminus.
                        buffer.DrawCircle(m_DrawColor, node.m_Position, endSize);
                    }
                }
            }
            finally
            {
                endHandles.Dispose();
            }
        }

        /// <summary>Buffer-forward chain start node (topology <c>IsInverted</c> on first element).</summary>
        private Entity GetChainStartNode(DynamicBuffer<AggregateElement> elements)
        {
            Entity edgeEntity = elements[0].m_Edge;
            if (!EntityManager.TryGetComponent(edgeEntity, out Edge edge))
            {
                return Entity.Null;
            }

            return IsSegmentInverted(elements, 0, edgeEntity) ? edge.m_End : edge.m_Start;
        }

        /// <summary>Buffer-forward chain end node (topology <c>IsInverted</c> on last element).</summary>
        private Entity GetChainEndNode(DynamicBuffer<AggregateElement> elements)
        {
            int last = elements.Length - 1;
            Entity edgeEntity = elements[last].m_Edge;
            if (!EntityManager.TryGetComponent(edgeEntity, out Edge edge))
            {
                return Entity.Null;
            }

            return IsSegmentInverted(elements, last, edgeEntity) ? edge.m_Start : edge.m_End;
        }

        /// <summary>Exact copy of <c>RouteBufferSystem.AddSegment</c> curve matrix packing (non-shared path).</summary>
        private int AddSegment(Bezier4x3 curve, float2 offset, float2 opacity, float broken)
        {
            float3 mid = math.lerp(curve.a, curve.d, 0.5f);
            var value = new SegmentData
            {
                m_Curve = new float4x4
                {
                    c0 = new float4(curve.a - mid, offset.x),
                    c1 = new float4(curve.b - mid, offset.x + math.distance(curve.a, curve.b)),
                    c2 = new float4(curve.c - mid, offset.y - math.distance(curve.c, curve.d)),
                    c3 = new float4(curve.d - mid, offset.y)
                },
                m_Position = mid,
                m_SizeFactor = new float3(1f, 1f, 1f),
                m_Opacity = opacity,
                m_DividedOpacity = new float2(1f, 1f),
                m_Broken = broken
            };
            int index = m_Segments.Length;
            m_Segments.Add(value);
            m_Bounds |= MathUtils.Bounds(curve);
            return index;
        }

        private void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (!m_HasDraw || m_Segments.Length == 0 || m_Material == null)
            {
                return;
            }

            EnsureMesh();
            EnsureArgsBuffer();
            UploadSegments();

            uint indexCount = m_Mesh.GetIndexCount(0);
            uint indexStart = m_Mesh.GetIndexStart(0);
            uint baseVertex = m_Mesh.GetBaseVertex(0);

            m_ArgsArray.Clear();
            m_ArgsArray.Add(indexCount);
            m_ArgsArray.Add((uint)m_Segments.Length);
            m_ArgsArray.Add(indexStart);
            m_ArgsArray.Add(baseVertex);
            m_ArgsArray.Add(0u);
            m_ArgsBuffer.SetData(m_ArgsArray, 0, 0, m_ArgsArray.Count);

            // Highlighted route style (opaque + slightly wider).
            var color = m_DrawColor;
            color.a = 1f;
            Vector4 size = m_RouteSize;
            size.x *= 1.3333334f;
            m_Material.renderQueue = m_OriginalRenderQueue + 1;
            m_Material.SetBuffer(RouteSegmentBufferId, m_SegmentBuffer);
            m_Material.SetColor(RouteColorId, color);
            m_Material.SetVector(RouteSizeId, size);

            Bounds bounds = RenderingUtils.ToBounds(m_Bounds);
            bounds.Expand(size.x);

            foreach (Camera camera in cameras)
            {
                if (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView)
                {
                    Graphics.DrawMeshInstancedIndirect(
                        m_Mesh,
                        0,
                        m_Material,
                        bounds,
                        m_ArgsBuffer,
                        0,
                        null,
                        ShadowCastingMode.Off,
                        receiveShadows: false,
                        0,
                        camera);
                }
            }
        }

        private unsafe void UploadSegments()
        {
            int count = m_Segments.Length;
            int stride = UnsafeUtility.SizeOf<SegmentData>();
            if (m_SegmentBuffer != null && (m_SegmentBuffer.count != count || m_SegmentBuffer.stride != stride))
            {
                m_SegmentBuffer.Release();
                m_SegmentBuffer = null;
            }

            if (m_SegmentBuffer == null)
            {
                // Exact Game path: new ComputeBuffer(length, sizeof(SegmentData)) — default buffer type.
                m_SegmentBuffer = new ComputeBuffer(count, stride);
                m_SegmentBuffer.name = "ADR RoadGrouping route segment buffer";
            }

            NativeArray<SegmentData> data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<SegmentData>(
                m_Segments.GetUnsafeReadOnlyPtr(), count, Allocator.None);
            m_SegmentBuffer.SetData(data);
        }

        private void EnsureArgsBuffer()
        {
            if (m_ArgsBuffer != null)
            {
                return;
            }

            m_ArgsBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);
            m_ArgsBuffer.name = "ADR RoadGrouping route args buffer";
        }

        private void EnsureMaterial()
        {
            if (m_Material != null || m_RouteConfigQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var config = m_PrefabSystem.GetSingletonPrefab<RouteConfigurationPrefab>(m_RouteConfigQuery);
            RoutePrefab routePrefab = config.m_CarPathVisualization;
            if (routePrefab?.m_Material == null)
            {
                return;
            }

            m_Material = new Material(routePrefab.m_Material);
            m_Material.name = "ADR RoadGrouping Routes (" + routePrefab.name + ")";
            m_OriginalRenderQueue = m_Material.renderQueue;
            m_RouteSize = new Vector4(routePrefab.m_Width, routePrefab.m_Width * 0.25f, routePrefab.m_SegmentLength, 0f);
        }

        /// <summary>Exact copy of <c>RouteRenderSystem.EnsureMesh</c>.</summary>
        private void EnsureMesh()
        {
            if (m_Mesh != null)
            {
                return;
            }

            Vector3[] array = new Vector3[68];
            Vector2[] array2 = new Vector2[array.Length];
            int[] array3 = new int[192];
            int num = 0;
            int num2 = 0;
            for (int i = 0; i <= 16; i++)
            {
                float num3 = (float)i / 16f;
                array[num] = new Vector3(-1f, 0f, num3);
                array2[num] = new Vector2(0f, num3);
                num++;
                array[num] = new Vector3(1f, 0f, num3);
                array2[num] = new Vector2(0f, num3);
                num++;
                if (i != 0)
                {
                    array3[num2++] = num - 4;
                    array3[num2++] = num - 3;
                    array3[num2++] = num - 2;
                    array3[num2++] = num - 2;
                    array3[num2++] = num - 3;
                    array3[num2++] = num - 1;
                }
            }

            for (int j = 0; j <= 16; j++)
            {
                float num4 = (float)j / 16f;
                array[num] = new Vector3(0f, -1f, num4);
                array2[num] = new Vector2(1f, num4);
                num++;
                array[num] = new Vector3(0f, 1f, num4);
                array2[num] = new Vector2(1f, num4);
                num++;
                if (j != 0)
                {
                    array3[num2++] = num - 4;
                    array3[num2++] = num - 3;
                    array3[num2++] = num - 2;
                    array3[num2++] = num - 2;
                    array3[num2++] = num - 3;
                    array3[num2++] = num - 1;
                }
            }

            m_Mesh = new Mesh();
            m_Mesh.name = "ADR RoadGrouping route segment";
            m_Mesh.vertices = array;
            m_Mesh.uv = array2;
            m_Mesh.triangles = array3;
        }

        private bool TryResolveHoveredAggregate(out Entity aggregate)
        {
            aggregate = Entity.Null;
            var hovered = m_Tool.HoveredEdge;
            return hovered != Entity.Null
                   && EntityManager.TryGetComponent(hovered, out Aggregated aggregated)
                   && (aggregate = aggregated.m_Aggregate) != Entity.Null;
        }

        /// <summary>
        /// Same direction flips as <c>BuildingUtilsOverrides.GetAddress_Override</c> /
        /// <c>CheckSegmentReversion(..., isInverseAggregate)</c> plus highway
        /// <c>ADRHighwayAggregationCacheData.reverseCounting</c>.
        /// </summary>
        private bool IsNumberingSegmentInverted(
            DynamicBuffer<AggregateElement> elements,
            int index,
            Entity edge,
            bool isInverseAggregate,
            bool reverseCounting)
        {
            bool inverted = IsSegmentInverted(elements, index, edge);
            if (isInverseAggregate)
            {
                inverted = !inverted;
            }

            if (reverseCounting)
            {
                inverted = !inverted;
            }

            return inverted;
        }

        /// <summary>
        /// ADR house-number growth direction (same as GetAddress override):
        /// compare oriented chain ends to highway <c>refStartPoint</c> or city zero marker;
        /// optional <c>reverseCounting</c> from highway cache.
        /// </summary>
        private void ResolveNumberingDirection(
            Entity aggregate,
            DynamicBuffer<AggregateElement> elements,
            out bool isInverseAggregate,
            out bool reverseCounting)
        {
            isInverseAggregate = false;
            reverseCounting = false;
            if (elements.Length == 0)
            {
                return;
            }

            if (EntityManager.TryGetComponent(aggregate, out ADRHighwayAggregationCacheData cache))
            {
                reverseCounting = cache.reverseCounting;
            }

            if (!EntityManager.TryGetComponent(elements[0].m_Edge, out Curve e0)
                || !EntityManager.TryGetComponent(elements[elements.Length - 1].m_Edge, out Curve e1))
            {
                return;
            }

            // Topology-only orientation (isInverseAggregate=false), matching GetAddress override.
            bool inv0 = IsSegmentInverted(elements, 0, elements[0].m_Edge);
            bool inv1 = IsSegmentInverted(elements, elements.Length - 1, elements[elements.Length - 1].m_Edge);
            float2 refE0 = inv0 ? e0.m_Bezier.d.xz : e0.m_Bezier.a.xz;
            float2 refE1 = inv1 ? e1.m_Bezier.a.xz : e1.m_Bezier.d.xz;

            float2 zeroMarker = m_AdrMainSystem.GetZeroMarkerPosition();
            if (EntityManager.TryGetComponent(aggregate, out ADRHighwayAggregationData hwDataInfo)
                && hwDataInfo.highwayDataId != default
                && m_HighwayRoutesSystem.TryGetHighwayData(hwDataInfo.highwayDataId, out var hwData))
            {
                zeroMarker = hwData.refStartPoint;
            }

            isInverseAggregate = math.distancesq(refE0, zeroMarker) > math.distancesq(refE1, zeroMarker);
        }

        /// <summary>
        /// Same topology rule as <c>AggregateMeshSystem.IsInverted</c> /
        /// ADR <c>CheckSegmentReversion</c> with <c>isInverseAggregate == false</c>.
        /// </summary>
        private bool IsSegmentInverted(DynamicBuffer<AggregateElement> elements, int index, Entity edge)
        {
            if (index > 0)
            {
                if (!EntityManager.TryGetComponent(elements[index - 1].m_Edge, out Edge prev)
                    || !EntityManager.TryGetComponent(edge, out Edge cur))
                {
                    return false;
                }

                if (cur.m_End == prev.m_Start)
                {
                    return true;
                }

                return cur.m_End == prev.m_End;
            }

            if (index < elements.Length - 1)
            {
                if (!EntityManager.TryGetComponent(edge, out Edge cur)
                    || !EntityManager.TryGetComponent(elements[index + 1].m_Edge, out Edge next))
                {
                    return false;
                }

                if (cur.m_Start == next.m_Start)
                {
                    return true;
                }

                return cur.m_Start == next.m_End;
            }

            return false;
        }

        /// <summary>
        /// Force bezier a/d onto Edge node centers after orientation so consecutive
        /// AggregateElement segments share exact join points.
        /// </summary>
        private void SnapBezierEndsToNodes(ref Bezier4x3 bezier, Edge edge, bool inverted)
        {
            Entity nodeA = inverted ? edge.m_End : edge.m_Start;
            Entity nodeD = inverted ? edge.m_Start : edge.m_End;
            if (EntityManager.TryGetComponent(nodeA, out Node start))
            {
                bezier.a = start.m_Position;
            }

            if (EntityManager.TryGetComponent(nodeD, out Node end))
            {
                bezier.d = end.m_Position;
            }
        }

        private static void IncrementDegree(ref NativeHashMap<Entity, int> degrees, Entity node)
        {
            if (node == Entity.Null)
            {
                return;
            }

            degrees.TryGetValue(node, out int count);
            degrees[node] = count + 1;
        }
    }
}
