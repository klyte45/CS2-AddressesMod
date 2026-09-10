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

        private static readonly Color BelongingColor = new(0f, 0x88 / 255f, 0f, 1f);
        private static readonly Color HighlightColor = new(0f, 0xBB / 255f, 0f, 1f);
        private static readonly Color HoverPreviewColor = new(0f, 0x44 / 255f, 0f, 1f);
        private static readonly Color RemovePreviewColor = new(1f, 0x88 / 255f, 0f, 1f); // #FF8800 would-remove
        private static readonly Color FixedMembershipColor = new(1f, 1f, 0f, 1f); // #FFFF00 fixed pin
        private static readonly Color EndHandleHoverColor = new(0f, 1f, 0f, 1f);

        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private AdrMainSystem m_AdrMainSystem;
        private AdrHighwayRoutesSystem m_HighwayRoutesSystem;
        private AdrRoadGroupingTool m_Tool;
        private AdrRoadGroupingController m_Controller;
        private EntityQuery m_RouteConfigQuery;

        private Mesh m_Mesh;
        private Material m_BelongingMaterial;
        private Material m_PreviewMaterial;
        private Material m_RemoveMaterial;
        private Material m_FixedMaterial;
        private int m_OriginalRenderQueue;
        private Vector4 m_RouteSize;
        private ComputeBuffer m_BelongingBuffer;
        private ComputeBuffer m_PreviewBuffer;
        private ComputeBuffer m_RemoveBuffer;
        private ComputeBuffer m_FixedBuffer;
        private ComputeBuffer m_BelongingArgsBuffer;
        private ComputeBuffer m_PreviewArgsBuffer;
        private ComputeBuffer m_RemoveArgsBuffer;
        private ComputeBuffer m_FixedArgsBuffer;
        private readonly List<uint> m_ArgsArray = new(5);
        private NativeList<SegmentData> m_BelongingSegments;
        private NativeList<SegmentData> m_PreviewSegments;
        private NativeList<SegmentData> m_RemoveSegments;
        private NativeList<SegmentData> m_FixedSegments;
        private Bounds3 m_BelongingBounds;
        private Bounds3 m_PreviewBounds;
        private Bounds3 m_RemoveBounds;
        private Bounds3 m_FixedBounds;
        private bool m_HasBelonging;
        private bool m_HasPreview;
        private bool m_HasRemove;
        private bool m_HasFixed;
        private Color m_PreviewDrawColor = HoverPreviewColor;
        private Entity m_HoveredEndHandle;

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
            m_BelongingSegments = new NativeList<SegmentData>(64, Allocator.Persistent);
            m_PreviewSegments = new NativeList<SegmentData>(64, Allocator.Persistent);
            m_RemoveSegments = new NativeList<SegmentData>(64, Allocator.Persistent);
            m_FixedSegments = new NativeList<SegmentData>(4, Allocator.Persistent);
            m_BelongingBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_PreviewBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_RemoveBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_FixedBounds = new Bounds3(float.MaxValue, float.MinValue);
            RenderPipelineManager.beginContextRendering += Render;
        }

        protected override void OnDestroy()
        {
            RenderPipelineManager.beginContextRendering -= Render;
            if (m_BelongingSegments.IsCreated)
            {
                m_BelongingSegments.Dispose();
            }

            if (m_PreviewSegments.IsCreated)
            {
                m_PreviewSegments.Dispose();
            }

            if (m_RemoveSegments.IsCreated)
            {
                m_RemoveSegments.Dispose();
            }

            if (m_FixedSegments.IsCreated)
            {
                m_FixedSegments.Dispose();
            }

            if (m_Mesh != null)
            {
                UnityEngine.Object.Destroy(m_Mesh);
            }

            if (m_BelongingMaterial != null)
            {
                UnityEngine.Object.Destroy(m_BelongingMaterial);
            }

            if (m_PreviewMaterial != null)
            {
                UnityEngine.Object.Destroy(m_PreviewMaterial);
            }

            if (m_RemoveMaterial != null)
            {
                UnityEngine.Object.Destroy(m_RemoveMaterial);
            }

            if (m_FixedMaterial != null)
            {
                UnityEngine.Object.Destroy(m_FixedMaterial);
            }

            m_BelongingBuffer?.Release();
            m_PreviewBuffer?.Release();
            m_RemoveBuffer?.Release();
            m_FixedBuffer?.Release();
            m_BelongingArgsBuffer?.Release();
            m_PreviewArgsBuffer?.Release();
            m_RemoveArgsBuffer?.Release();
            m_FixedArgsBuffer?.Release();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            m_HasBelonging = false;
            m_HasPreview = false;
            m_HasRemove = false;
            m_HasFixed = false;
            m_BelongingSegments.Clear();
            m_PreviewSegments.Clear();
            m_RemoveSegments.Clear();
            m_FixedSegments.Clear();
            m_BelongingBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_PreviewBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_RemoveBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_FixedBounds = new Bounds3(float.MaxValue, float.MinValue);
            m_HoveredEndHandle = Entity.Null;
            m_PreviewDrawColor = HoverPreviewColor;

            if (m_ToolSystem.activeTool != m_Tool)
            {
                return;
            }

            EnsureMaterial();
            if (m_BelongingMaterial == null)
            {
                return;
            }

            var selected = m_Controller.SelectedAggregation?.Value ?? Entity.Null;
            m_HoveredEndHandle = m_Tool.HoveredDragHandle;

            if (selected == Entity.Null)
            {
                // Pick preview: hovered Aggregate in hover-preview green, no end handles.
                if (!TryResolveHoveredAggregate(out Entity hoverAggregate)
                    || !EntityManager.TryGetBuffer(hoverAggregate, true, out DynamicBuffer<AggregateElement> hoverElements)
                    || hoverElements.Length == 0)
                {
                    return;
                }

                m_PreviewDrawColor = HoverPreviewColor;
                BuildRouteSegments(hoverAggregate, hoverElements, m_PreviewSegments, ref m_PreviewBounds, drawEndNodes: false);
                m_HasPreview = m_PreviewSegments.Length > 0;
                return;
            }

            if (!EntityManager.TryGetBuffer(selected, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                return;
            }

            // Belonging membership + end handles.
            BuildRouteSegments(selected, elements, m_BelongingSegments, ref m_BelongingBounds, drawEndNodes: true);
            m_HasBelonging = m_BelongingSegments.Length > 0;

            BuildFixedMembershipOverlay();
            // Preview: drag add (highlight) + remove (orange), or hovered non-member.
            BuildHighlightPreview(selected);
        }

        private void BuildFixedMembershipOverlay()
        {
            Entity fixedEdge = m_Tool.FixedMembershipEdge;
            if (fixedEdge == Entity.Null)
            {
                return;
            }

            AddEdgeSegment(fixedEdge, m_FixedSegments, ref m_FixedBounds);
            if (m_FixedSegments.Length == 0)
            {
                return;
            }

            SaturateOpacities(m_FixedSegments);
            m_HasFixed = true;
            DrawFixedMembershipMarker(fixedEdge);
        }

        private void DrawFixedMembershipMarker(Entity edgeEntity)
        {
            if (!EntityManager.TryGetComponent(edgeEntity, out Curve curve))
            {
                return;
            }

            float3 mid = MathUtils.Position(curve.m_Bezier, 0.5f);
            float size = EndHandleDiameter;
            if (size < 0.01f)
            {
                size = 8f;
            }

            JobHandle dependencies;
            OverlayRenderSystem.Buffer buffer = m_OverlayRenderSystem.GetBuffer(out dependencies);
            dependencies.Complete();
            buffer.DrawCustomMesh(
                FixedMembershipColor,
                mid,
                size,
                size,
                OverlayRenderSystem.CustomMeshType.Plane);
        }

        /// <summary>
        /// Preview layer: endpoint-drag would-add / would-remove, else hovered non-member.
        /// </summary>
        private void BuildHighlightPreview(Entity selected)
        {
            if (m_Tool.IsEndpointDragging)
            {
                var addPath = m_Tool.PreviewAddEdges;
                var removePath = m_Tool.PreviewRemoveEdges;
                if ((addPath == null || addPath.Count == 0)
                    && (removePath == null || removePath.Count == 0))
                {
                    return;
                }

                if (addPath != null)
                {
                    for (int i = 0; i < addPath.Count; i++)
                    {
                        AddEdgeSegment(addPath[i], m_PreviewSegments, ref m_PreviewBounds);
                    }

                    if (m_PreviewSegments.Length > 0)
                    {
                        SaturateOpacities(m_PreviewSegments);
                        m_PreviewDrawColor = HighlightColor;
                        m_HasPreview = true;
                    }
                }

                if (removePath != null)
                {
                    for (int i = 0; i < removePath.Count; i++)
                    {
                        AddEdgeSegment(removePath[i], m_RemoveSegments, ref m_RemoveBounds);
                    }

                    if (m_RemoveSegments.Length > 0)
                    {
                        SaturateOpacities(m_RemoveSegments);
                        m_HasRemove = true;
                    }
                }

                return;
            }

            // Prefer end-handle hover feedback over non-member segment wash.
            if (m_HoveredEndHandle != Entity.Null)
            {
                return;
            }

            Entity hoveredEdge = m_Tool.HoveredEdge;
            if (hoveredEdge == Entity.Null
                || !EntityManager.TryGetComponent(hoveredEdge, out Aggregated aggregated)
                || aggregated.m_Aggregate == selected
                || aggregated.m_Aggregate == Entity.Null)
            {
                return;
            }

            m_PreviewDrawColor = HoverPreviewColor;
            AddEdgeSegment(hoveredEdge, m_PreviewSegments, ref m_PreviewBounds);
            SaturateOpacities(m_PreviewSegments);
            m_HasPreview = m_PreviewSegments.Length > 0;
        }

        private void BuildRouteSegments(
            Entity aggregate,
            DynamicBuffer<AggregateElement> elements,
            NativeList<SegmentData> dest,
            ref Bounds3 bounds,
            bool drawEndNodes)
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
                    int segmentIndex = AddSegment(dest, ref bounds, bezier, offset, opacity, broken: 0f);
                    distance = offset.y;

                    if (prevSegmentIndex >= 0 && segmentIndex >= 0)
                    {
                        // RouteBufferSystem joint size-factor match between consecutive segments.
                        ref SegmentData prev = ref dest.ElementAt(prevSegmentIndex);
                        ref SegmentData cur = ref dest.ElementAt(segmentIndex);
                        float joint = math.max(prev.m_SizeFactor.z, cur.m_SizeFactor.x);
                        prev.m_SizeFactor.z = joint;
                        cur.m_SizeFactor.x = joint;
                        // Keep DividedOpacity at 1 — do not reintroduce joint fade for overlay.
                    }

                    prevSegmentIndex = segmentIndex;
                }

                if (drawEndNodes)
                {
                    DrawNodeMarkers(elements, nodeDegrees, drawEndNodes: true);
                }

                SaturateOpacities(dest);
            }
            finally
            {
                nodeDegrees.Dispose();
            }
        }

        private void AddEdgeSegment(Entity edgeEntity, NativeList<SegmentData> dest, ref Bounds3 bounds)
        {
            if (!EntityManager.TryGetComponent(edgeEntity, out Curve curve)
                || !EntityManager.TryGetComponent(edgeEntity, out Edge edge))
            {
                return;
            }

            Bezier4x3 bezier = curve.m_Bezier;
            SnapBezierEndsToNodes(ref bezier, edge, inverted: false);
            float length = MathUtils.Length(bezier);
            if (length < 0.01f)
            {
                return;
            }

            AddSegment(dest, ref bounds, bezier, new float2(0f, length), new float2(1f, 1f), broken: 0f);
        }

        private static void SaturateOpacities(NativeList<SegmentData> segments)
        {
            for (int s = 0; s < segments.Length; s++)
            {
                ref SegmentData seg = ref segments.ElementAt(s);
                seg.m_Opacity = math.saturate(seg.m_Opacity);
            }
        }

        /// <summary>
        /// World diameter of selected end / drag-handle circles (matches Overlay <c>DrawCircle</c>).
        /// Fallback when route material size is not ready yet.
        /// </summary>
        public float EndHandleDiameter
        {
            get
            {
                float baseSize = m_RouteSize.x * (4f / 3f);
                return baseSize < 0.01f ? 8f : baseSize * 2f;
            }
        }

        /// <summary>
        /// Nodes that show full-size drag circles for the selected Aggregate
        /// (chain termini + any leftover degree-1 nodes).
        /// </summary>
        public void CollectDragHandleNodes(Entity aggregate, HashSet<Entity> dest)
        {
            if (dest == null
                || aggregate == Entity.Null
                || !EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                return;
            }

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

                Entity start = GetChainStartNode(elements);
                Entity end = GetChainEndNode(elements);
                if (start != Entity.Null)
                {
                    dest.Add(start);
                }

                if (end != Entity.Null)
                {
                    dest.Add(end);
                }

                foreach (var pair in nodeDegrees)
                {
                    if (pair.Value == 1)
                    {
                        dest.Add(pair.Key);
                    }
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
        /// Hovered end-handle uses a brighter, larger circle.
        /// </summary>
        private void DrawNodeMarkers(
            DynamicBuffer<AggregateElement> elements,
            NativeHashMap<Entity, int> nodeDegrees,
            bool drawEndNodes)
        {
            float endSize = EndHandleDiameter;
            if (endSize < 0.01f)
            {
                return;
            }

            float jointSize = endSize * 0.5f;
            float hoverSize = endSize * 1.35f;

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
                            bool hovered = endEntity == m_HoveredEndHandle;
                            buffer.DrawCircle(
                                hovered ? EndHandleHoverColor : HighlightColor,
                                endNode.m_Position,
                                hovered ? hoverSize : endSize);
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
                        buffer.DrawCircle(BelongingColor, node.m_Position, jointSize);
                    }
                    else if (drawEndNodes && pair.Value == 1)
                    {
                        // Extra dead-end not captured as buffer terminus.
                        bool hovered = pair.Key == m_HoveredEndHandle;
                        buffer.DrawCircle(
                            hovered ? EndHandleHoverColor : HighlightColor,
                            node.m_Position,
                            hovered ? hoverSize : endSize);
                    }
                }

                // Dragging toward a non-handle node: still show hover feedback at that tip.
                if (m_HoveredEndHandle != Entity.Null
                    && !endHandles.Contains(m_HoveredEndHandle)
                    && (!nodeDegrees.TryGetValue(m_HoveredEndHandle, out int deg) || deg != 1)
                    && EntityManager.TryGetComponent(m_HoveredEndHandle, out Node hoverNode))
                {
                    buffer.DrawCircle(EndHandleHoverColor, hoverNode.m_Position, hoverSize);
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
        private static int AddSegment(
            NativeList<SegmentData> dest,
            ref Bounds3 bounds,
            Bezier4x3 curve,
            float2 offset,
            float2 opacity,
            float broken)
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
            int index = dest.Length;
            dest.Add(value);
            bounds |= MathUtils.Bounds(curve);
            return index;
        }

        private void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if ((!m_HasBelonging && !m_HasPreview && !m_HasRemove && !m_HasFixed) || m_BelongingMaterial == null)
            {
                return;
            }

            EnsureMesh();

            if (m_HasBelonging)
            {
                DrawLayer(
                    cameras,
                    m_BelongingSegments,
                    ref m_BelongingBuffer,
                    ref m_BelongingArgsBuffer,
                    m_BelongingBounds,
                    m_BelongingMaterial,
                    BelongingColor);
            }

            if (m_HasFixed && m_FixedMaterial != null)
            {
                DrawLayer(
                    cameras,
                    m_FixedSegments,
                    ref m_FixedBuffer,
                    ref m_FixedArgsBuffer,
                    m_FixedBounds,
                    m_FixedMaterial,
                    FixedMembershipColor);
            }

            if (m_HasRemove && m_RemoveMaterial != null)
            {
                DrawLayer(
                    cameras,
                    m_RemoveSegments,
                    ref m_RemoveBuffer,
                    ref m_RemoveArgsBuffer,
                    m_RemoveBounds,
                    m_RemoveMaterial,
                    RemovePreviewColor);
            }

            if (m_HasPreview && m_PreviewMaterial != null)
            {
                DrawLayer(
                    cameras,
                    m_PreviewSegments,
                    ref m_PreviewBuffer,
                    ref m_PreviewArgsBuffer,
                    m_PreviewBounds,
                    m_PreviewMaterial,
                    m_PreviewDrawColor);
            }
        }

        private void DrawLayer(
            List<Camera> cameras,
            NativeList<SegmentData> segments,
            ref ComputeBuffer buffer,
            ref ComputeBuffer argsBuffer,
            Bounds3 layerBounds,
            Material material,
            Color layerColor)
        {
            if (segments.Length == 0)
            {
                return;
            }

            UploadSegments(segments, ref buffer);
            EnsureArgsBuffer(ref argsBuffer);

            uint indexCount = m_Mesh.GetIndexCount(0);
            uint indexStart = m_Mesh.GetIndexStart(0);
            uint baseVertex = m_Mesh.GetBaseVertex(0);

            m_ArgsArray.Clear();
            m_ArgsArray.Add(indexCount);
            m_ArgsArray.Add((uint)segments.Length);
            m_ArgsArray.Add(indexStart);
            m_ArgsArray.Add(baseVertex);
            m_ArgsArray.Add(0u);
            argsBuffer.SetData(m_ArgsArray, 0, 0, m_ArgsArray.Count);

            // Highlighted route style (opaque + slightly wider).
            var color = layerColor;
            color.a = 1f;
            Vector4 size = m_RouteSize;
            size.x *= 1.3333334f;
            material.renderQueue = m_OriginalRenderQueue + 1;
            material.SetBuffer(RouteSegmentBufferId, buffer);
            material.SetColor(RouteColorId, color);
            material.SetVector(RouteSizeId, size);

            Bounds bounds = RenderingUtils.ToBounds(layerBounds);
            bounds.Expand(size.x);

            foreach (Camera camera in cameras)
            {
                if (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView)
                {
                    Graphics.DrawMeshInstancedIndirect(
                        m_Mesh,
                        0,
                        material,
                        bounds,
                        argsBuffer,
                        0,
                        null,
                        ShadowCastingMode.Off,
                        receiveShadows: false,
                        0,
                        camera);
                }
            }
        }

        private unsafe void UploadSegments(NativeList<SegmentData> segments, ref ComputeBuffer buffer)
        {
            int count = segments.Length;
            int stride = UnsafeUtility.SizeOf<SegmentData>();
            if (buffer != null && (buffer.count != count || buffer.stride != stride))
            {
                buffer.Release();
                buffer = null;
            }

            if (buffer == null)
            {
                // Exact Game path: new ComputeBuffer(length, sizeof(SegmentData)) — default buffer type.
                buffer = new ComputeBuffer(count, stride);
                buffer.name = "ADR RoadGrouping route segment buffer";
            }

            NativeArray<SegmentData> data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<SegmentData>(
                segments.GetUnsafeReadOnlyPtr(), count, Allocator.None);
            buffer.SetData(data);
        }

        private static void EnsureArgsBuffer(ref ComputeBuffer argsBuffer)
        {
            if (argsBuffer != null)
            {
                return;
            }

            argsBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);
            argsBuffer.name = "ADR RoadGrouping route args buffer";
        }

        private void EnsureMaterial()
        {
            if (m_BelongingMaterial != null || m_RouteConfigQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var config = m_PrefabSystem.GetSingletonPrefab<RouteConfigurationPrefab>(m_RouteConfigQuery);
            RoutePrefab routePrefab = config.m_CarPathVisualization;
            if (routePrefab?.m_Material == null)
            {
                return;
            }

            m_BelongingMaterial = new Material(routePrefab.m_Material);
            m_BelongingMaterial.name = "ADR RoadGrouping Routes belonging (" + routePrefab.name + ")";
            m_PreviewMaterial = new Material(routePrefab.m_Material);
            m_PreviewMaterial.name = "ADR RoadGrouping Routes preview (" + routePrefab.name + ")";
            m_RemoveMaterial = new Material(routePrefab.m_Material);
            m_RemoveMaterial.name = "ADR RoadGrouping Routes remove (" + routePrefab.name + ")";
            m_FixedMaterial = new Material(routePrefab.m_Material);
            m_FixedMaterial.name = "ADR RoadGrouping Routes fixed (" + routePrefab.name + ")";
            m_OriginalRenderQueue = m_BelongingMaterial.renderQueue;
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
