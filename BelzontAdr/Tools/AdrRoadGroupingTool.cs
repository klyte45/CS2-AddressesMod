using System.Collections.Generic;
using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Notifications;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BelzontAdr
{
    public partial class AdrRoadGroupingTool : IBelzontToolSystem
    {
        public const string TOOL_ID = "K45_ADR_AdrRoadGroupingTool";

        public override string toolID => TOOL_ID;

        public bool IsSelected => m_ToolSystem.activeTool == this;

        public Entity HoveredEdge { get; private set; }

        /// <summary>Terminal end-handle under cursor while a group is focused (or while dragging).</summary>
        public Entity HoveredDragHandle { get; private set; }

        public bool IsEndpointDragging => m_Dragging;

        /// <summary>Live preferred path edges while endpoint-dragging (for overlay preview).</summary>
        public IReadOnlyList<Entity> PreviewPathEdges => m_PathEdges;

        /// <summary>Path edges not in focused membership — would be added on release.</summary>
        public IReadOnlyList<Entity> PreviewAddEdges => m_PreviewAddEdges;

        /// <summary>Path edges already in focused membership — would be removed on release (tip shrink / pivot).</summary>
        public IReadOnlyList<Entity> PreviewRemoveEdges => m_PreviewRemoveEdges;

        private ProxyAction m_ApplyAction;
        private ProxyAction m_CancelAction;
        private ProxyAction m_ToggleAction;
        private AdrRoadGroupingController m_Controller;
        private AdrRoadGroupingOverlayRenderSystem m_Overlay;

        private bool m_Dragging;
        private Entity m_DragStartNode;
        private Entity m_DragHoverNode;
        private readonly HashSet<Entity> m_DragHandles = new();
        private readonly HashSet<Entity> m_MembershipEdges = new();
        private readonly List<Entity> m_PathEdges = new();
        private readonly List<Entity> m_PreviewAddEdges = new();
        private readonly List<Entity> m_PreviewRemoveEdges = new();
        private readonly HashSet<Entity> m_LoggedEdges = new();
        private readonly Dictionary<Entity, Entity> m_CameFromEdge = new();
        private readonly Dictionary<Entity, Entity> m_CameFromNode = new();
        private readonly Dictionary<Entity, Entity> m_PreferAggregateAtNode = new();
        private readonly Dictionary<Entity, int> m_PathCost = new();
        private readonly LinkedList<Entity> m_PathFrontier = new();

        protected override void OnCreateWithBarrier()
        {
            Enabled = false;
            m_ApplyAction = AdrModData.Instance.GetAction(AdrModData.kActionApplyMouse);
            m_CancelAction = AdrModData.Instance.GetAction(AdrModData.kActionCancelMouse);
            m_ToggleAction = AdrModData.Instance.GetAction(AdrModData.kActionToggleMembership);
            m_Controller = World.GetOrCreateSystemManaged<AdrRoadGroupingController>();
            m_Overlay = World.GetOrCreateSystemManaged<AdrRoadGroupingOverlayRenderSystem>();
        }

        public override PrefabBase GetPrefab() => null;

        public override bool TrySetPrefab(PrefabBase prefab) => false;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            m_ApplyAction.shouldBeEnabled = true;
            m_CancelAction.shouldBeEnabled = true;
            m_ToggleAction.shouldBeEnabled = true;
            cancelAction.shouldBeEnabled = true; // Escape (Tool "Cancel")
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            m_ApplyAction.shouldBeEnabled = false;
            m_CancelAction.shouldBeEnabled = false;
            m_ToggleAction.shouldBeEnabled = false;
            cancelAction.shouldBeEnabled = false;
            HoveredEdge = Entity.Null;
            HoveredDragHandle = Entity.Null;
            ClearDrag();
            m_Controller.ClearSelection();
        }

        public void Select()
        {
            m_ToolSystem.activeTool = this;
        }

        public void RequestDisable()
        {
            ClearDrag();
            m_Controller.ClearSelection();
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        /// <summary>
        /// RMB / Escape: clear selection if focused; otherwise leave the tool.
        /// </summary>
        private void HandleCancel()
        {
            if (m_Dragging)
            {
                ClearDrag();
                return;
            }

            if (m_Controller.SelectedAggregation != null
                && m_Controller.SelectedAggregation.Value != Entity.Null)
            {
                m_Controller.ClearSelection();
                return;
            }

            RequestDisable();
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground;
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.raycastFlags = RaycastFlags.SubElements | RaycastFlags.IgnoreSecondary;
            m_ToolRaycastSystem.netLayerMask = (Layer)~0u;
            m_ToolRaycastSystem.iconLayerMask = IconLayerMask.None;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_CancelAction.WasPressedThisFrame() || cancelAction.WasPressedThisFrame())
            {
                HandleCancel();
                return inputDeps;
            }

            bool collide = GetRaycastResult(out Entity entity, out RaycastHit hit);
            Entity eligibleEdge = Entity.Null;
            if (collide && TryResolveAggregatedEdge(entity, out eligibleEdge, out _))
            {
                HoveredEdge = eligibleEdge;
            }
            else
            {
                HoveredEdge = Entity.Null;
            }

            Entity selected = m_Controller.SelectedAggregation?.Value ?? Entity.Null;
            bool overUi = InputManager.instance.mouseOverUI;
            HoveredDragHandle = Entity.Null;

            if (selected != Entity.Null && !overUi && collide)
            {
                if (TryHitDragHandle(selected, entity, hit, out Entity hoverHandle))
                {
                    HoveredDragHandle = hoverHandle;
                }
            }

            if (m_Dragging)
            {
                if (!overUi && collide)
                {
                    m_DragHoverNode = ResolveHoverNode(entity, hit);
                    HoveredDragHandle = m_DragHoverNode;
                    RefreshLivePathPreview(selected);
                }
                else
                {
                    m_PathEdges.Clear();
                    m_PreviewAddEdges.Clear();
                    m_PreviewRemoveEdges.Clear();
                }

                if (m_ApplyAction.WasReleasedThisFrame() || !m_ApplyAction.IsPressed())
                {
                    FinishEndpointDrag(selected);
                }

                return inputDeps;
            }

            if (overUi)
            {
                return inputDeps;
            }

            if (selected == Entity.Null)
            {
                if (m_ApplyAction.WasPressedThisFrame()
                    && eligibleEdge != Entity.Null
                    && EntityManager.TryGetComponent(eligibleEdge, out Aggregated aggregated))
                {
                    m_Controller.SelectedAggregation.Value = aggregated.m_Aggregate;
                }

                return inputDeps;
            }

            // Group focused: dedicated Ctrl+Apply binding toggles; plain Apply starts end-handle drag.
            if (m_ToggleAction.WasPressedThisFrame())
            {
                if (eligibleEdge != Entity.Null
                    && EntityManager.HasComponent<Edge>(entity))
                {
                    LogToggleStub(eligibleEdge, selected);
                }

                return inputDeps;
            }

            if (!m_ApplyAction.WasPressedThisFrame())
            {
                return inputDeps;
            }

            if (HoveredDragHandle != Entity.Null)
            {
                BeginEndpointDrag(selected, HoveredDragHandle);
            }

            return inputDeps;
        }

        private void RefreshLivePathPreview(Entity selectedAggregate)
        {
            m_PathEdges.Clear();
            m_PreviewAddEdges.Clear();
            m_PreviewRemoveEdges.Clear();
            if (selectedAggregate == Entity.Null
                || m_DragStartNode == Entity.Null
                || m_DragHoverNode == Entity.Null
                || m_DragHoverNode == m_DragStartNode)
            {
                return;
            }

            if (!TryBuildPreferredPath(m_DragStartNode, m_DragHoverNode, selectedAggregate, m_PathEdges))
            {
                return;
            }

            ClassifyPathMembership(m_PathEdges, m_PreviewAddEdges, m_PreviewRemoveEdges);
        }

        private void BeginEndpointDrag(Entity selectedAggregate, Entity handleNode)
        {
            m_Dragging = true;
            m_DragStartNode = handleNode;
            m_DragHoverNode = handleNode;
            RefreshMembershipSet(selectedAggregate);
            LogUtils.DoInfoLog(
                "RoadGrouping endpoint drag start: aggregate={0}:{1} node={2}:{3}",
                selectedAggregate.Index,
                selectedAggregate.Version,
                handleNode.Index,
                handleNode.Version);
        }

        private void FinishEndpointDrag(Entity selectedAggregate)
        {
            Entity hoverNode = m_DragHoverNode;
            Entity startNode = m_DragStartNode;
            ClearDrag(keepMembership: true);

            if (selectedAggregate == Entity.Null
                || startNode == Entity.Null
                || hoverNode == Entity.Null
                || hoverNode == startNode)
            {
                m_MembershipEdges.Clear();
                return;
            }

            if (!TryBuildPreferredPath(startNode, hoverNode, selectedAggregate, m_PathEdges))
            {
                LogUtils.DoInfoLog(
                    "RoadGrouping endpoint drag: no path from node {0}:{1} to {2}:{3}",
                    startNode.Index,
                    startNode.Version,
                    hoverNode.Index,
                    hoverNode.Version);
                m_MembershipEdges.Clear();
                return;
            }

            ClassifyPathMembership(m_PathEdges, m_PreviewAddEdges, m_PreviewRemoveEdges);
            EmitPathMembershipLogs(selectedAggregate, m_PreviewAddEdges, m_PreviewRemoveEdges);
            m_MembershipEdges.Clear();
            m_PreviewAddEdges.Clear();
            m_PreviewRemoveEdges.Clear();
        }

        private void ClearDrag(bool keepMembership = false)
        {
            m_Dragging = false;
            m_DragStartNode = Entity.Null;
            m_DragHoverNode = Entity.Null;
            m_DragHandles.Clear();
            m_PathEdges.Clear();
            m_PreviewAddEdges.Clear();
            m_PreviewRemoveEdges.Clear();
            m_CameFromEdge.Clear();
            m_CameFromNode.Clear();
            m_PreferAggregateAtNode.Clear();
            m_PathCost.Clear();
            m_PathFrontier.Clear();
            if (!keepMembership)
            {
                m_MembershipEdges.Clear();
            }
        }

        private void RefreshMembershipSet(Entity aggregate)
        {
            m_MembershipEdges.Clear();
            if (!EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements))
            {
                return;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                m_MembershipEdges.Add(elements[i].m_Edge);
            }
        }

        private void LogToggleStub(Entity edge, Entity aggregate)
        {
            bool currentlyMember = false;
            if (EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements))
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i].m_Edge == edge)
                    {
                        currentlyMember = true;
                        break;
                    }
                }
            }

            LogUtils.DoInfoLog(
                "RoadGrouping membership toggle stub: edge={0}:{1} aggregate={2}:{3} would={4}",
                edge.Index,
                edge.Version,
                aggregate.Index,
                aggregate.Version,
                currentlyMember ? "remove" : "add");
        }

        /// <summary>
        /// Split path into would-add (non-members) and would-remove (members).
        /// Mixed paths happen when the new tip pivots through the current Aggregate
        /// (walk back along the tip, then hop onto a different direction) — both lists apply.
        /// </summary>
        private void ClassifyPathMembership(
            List<Entity> pathEdges,
            List<Entity> addOut,
            List<Entity> removeOut)
        {
            addOut.Clear();
            removeOut.Clear();
            m_LoggedEdges.Clear();
            for (int i = 0; i < pathEdges.Count; i++)
            {
                Entity edge = pathEdges[i];
                if (!m_LoggedEdges.Add(edge))
                {
                    continue;
                }

                if (m_MembershipEdges.Contains(edge))
                {
                    removeOut.Add(edge);
                }
                else
                {
                    addOut.Add(edge);
                }
            }
        }

        private void EmitPathMembershipLogs(
            Entity aggregate,
            List<Entity> addEdges,
            List<Entity> removeEdges)
        {
            for (int i = 0; i < removeEdges.Count; i++)
            {
                Entity edge = removeEdges[i];
                LogUtils.DoInfoLog(
                    "RoadGrouping endpoint drag would-remove: edge={0}:{1} aggregate={2}:{3}",
                    edge.Index,
                    edge.Version,
                    aggregate.Index,
                    aggregate.Version);
            }

            for (int i = 0; i < addEdges.Count; i++)
            {
                Entity edge = addEdges[i];
                LogUtils.DoInfoLog(
                    "RoadGrouping endpoint drag would-add: edge={0}:{1} aggregate={2}:{3}",
                    edge.Index,
                    edge.Version,
                    aggregate.Index,
                    aggregate.Version);
            }
        }

        /// <summary>
        /// 0-1 BFS: cost 0 when next edge shares previous step Aggregate; cost 1 on hop.
        /// </summary>
        private bool TryBuildPreferredPath(
            Entity startNode,
            Entity targetNode,
            Entity selectedAggregate,
            List<Entity> pathEdgesOut)
        {
            pathEdgesOut.Clear();
            m_CameFromEdge.Clear();
            m_CameFromNode.Clear();
            m_PreferAggregateAtNode.Clear();
            m_PathCost.Clear();
            m_PathFrontier.Clear();

            m_PathCost[startNode] = 0;
            m_PreferAggregateAtNode[startNode] = selectedAggregate;
            m_PathFrontier.AddLast(startNode);

            while (m_PathFrontier.Count > 0)
            {
                Entity node = m_PathFrontier.First.Value;
                m_PathFrontier.RemoveFirst();
                if (node == targetNode)
                {
                    break;
                }

                if (!EntityManager.TryGetBuffer(node, true, out DynamicBuffer<ConnectedEdge> connected))
                {
                    continue;
                }

                int nodeCost = m_PathCost[node];
                Entity prefer = m_PreferAggregateAtNode[node];

                for (int i = 0; i < connected.Length; i++)
                {
                    Entity edgeEntity = connected[i].m_Edge;
                    if (!EntityManager.TryGetComponent(edgeEntity, out Edge edge))
                    {
                        continue;
                    }

                    Entity next = edge.m_Start == node ? edge.m_End : edge.m_Start;
                    if (next == Entity.Null || next == node)
                    {
                        continue;
                    }

                    Entity edgeAgg = Entity.Null;
                    if (EntityManager.TryGetComponent(edgeEntity, out Aggregated aggregated))
                    {
                        edgeAgg = aggregated.m_Aggregate;
                    }

                    int stepCost = edgeAgg != Entity.Null && edgeAgg == prefer ? 0 : 1;
                    int nextCost = nodeCost + stepCost;
                    if (m_PathCost.TryGetValue(next, out int existing) && existing <= nextCost)
                    {
                        continue;
                    }

                    m_PathCost[next] = nextCost;
                    m_CameFromEdge[next] = edgeEntity;
                    m_CameFromNode[next] = node;
                    m_PreferAggregateAtNode[next] = edgeAgg != Entity.Null ? edgeAgg : prefer;

                    if (stepCost == 0)
                    {
                        m_PathFrontier.AddFirst(next);
                    }
                    else
                    {
                        m_PathFrontier.AddLast(next);
                    }
                }
            }

            if (!m_CameFromEdge.ContainsKey(targetNode) && startNode != targetNode)
            {
                return false;
            }

            Entity cursor = targetNode;
            while (cursor != startNode)
            {
                if (!m_CameFromEdge.TryGetValue(cursor, out Entity edge)
                    || !m_CameFromNode.TryGetValue(cursor, out Entity prev))
                {
                    pathEdgesOut.Clear();
                    return false;
                }

                pathEdgesOut.Add(edge);
                cursor = prev;
            }

            pathEdgesOut.Reverse();
            return true;
        }

        private bool TryHitDragHandle(Entity selectedAggregate, Entity hitEntity, RaycastHit hit, out Entity handleNode)
        {
            handleNode = Entity.Null;
            m_DragHandles.Clear();
            m_Overlay.CollectDragHandleNodes(selectedAggregate, m_DragHandles);
            if (m_DragHandles.Count == 0)
            {
                return false;
            }

            float radius = m_Overlay.EndHandleDiameter * 0.5f;
            float radiusSq = radius * radius;
            float3 probe = hit.m_HitPosition;

            // Prefer an explicit node hit that is a drag handle.
            if (EntityManager.HasComponent<Node>(hitEntity) && m_DragHandles.Contains(hitEntity))
            {
                handleNode = hitEntity;
                return true;
            }

            Entity best = Entity.Null;
            float bestDistSq = float.MaxValue;
            foreach (Entity candidate in m_DragHandles)
            {
                if (!EntityManager.TryGetComponent(candidate, out Node node))
                {
                    continue;
                }

                float distSq = math.distancesq(probe.xz, node.m_Position.xz);
                if (distSq <= radiusSq && distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = candidate;
                }
            }

            if (best == Entity.Null)
            {
                return false;
            }

            handleNode = best;
            return true;
        }

        private Entity ResolveHoverNode(Entity hitEntity, RaycastHit hit)
        {
            if (EntityManager.HasComponent<Node>(hitEntity))
            {
                return hitEntity;
            }

            if (EntityManager.TryGetComponent(hitEntity, out Edge edge))
            {
                return CloserNode(edge, hit.m_HitPosition);
            }

            if (TryResolveAggregatedEdge(hitEntity, out Entity edgeEntity, out _)
                && EntityManager.TryGetComponent(edgeEntity, out Edge resolved))
            {
                return CloserNode(resolved, hit.m_HitPosition);
            }

            return Entity.Null;
        }

        private Entity CloserNode(Edge edge, float3 probe)
        {
            bool hasStart = EntityManager.TryGetComponent(edge.m_Start, out Node startNode);
            bool hasEnd = EntityManager.TryGetComponent(edge.m_End, out Node endNode);
            if (!hasStart && !hasEnd)
            {
                return Entity.Null;
            }

            if (!hasStart)
            {
                return edge.m_End;
            }

            if (!hasEnd)
            {
                return edge.m_Start;
            }

            float startDist = math.distancesq(probe.xz, startNode.m_Position.xz);
            float endDist = math.distancesq(probe.xz, endNode.m_Position.xz);
            return startDist <= endDist ? edge.m_Start : edge.m_End;
        }

        private bool TryResolveAggregatedEdge(Entity hit, out Entity edge, out Entity aggregate)
        {
            edge = Entity.Null;
            aggregate = Entity.Null;

            if (EntityManager.HasComponent<Edge>(hit))
            {
                edge = hit;
            }
            else if (EntityManager.HasComponent<Node>(hit)
                     && EntityManager.TryGetBuffer(hit, true, out DynamicBuffer<ConnectedEdge> connected)
                     && connected.Length > 0)
            {
                for (int i = 0; i < connected.Length; i++)
                {
                    var candidate = connected[i].m_Edge;
                    if (EntityManager.TryGetComponent(candidate, out Aggregated agg))
                    {
                        edge = candidate;
                        aggregate = agg.m_Aggregate;
                        return true;
                    }
                }
                edge = connected[0].m_Edge;
            }
            else
            {
                return false;
            }

            if (EntityManager.TryGetComponent(edge, out Aggregated aggregated))
            {
                aggregate = aggregated.m_Aggregate;
                return true;
            }

            edge = Entity.Null;
            return false;
        }
    }
}
