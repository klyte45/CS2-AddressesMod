using System.Collections.Generic;
using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.PSI.Common;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Notifications;
using Game.Prefabs;
using Game.PSI;
using Game.Tools;
using Game.UI.Localization;
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

        /// <summary>
        /// At most one member edge pinned against endpoint-drag removal.
        /// Double-click a member toggles / replaces it.
        /// </summary>
        public Entity FixedMembershipEdge { get; private set; }

        /// <summary>Hover edge while endpoint-dragging (may be null on node-only hits).</summary>
        public Entity DragHoverEdge => m_DragHoverEdge;

        /// <summary>True when the live drag path passed destination/path validation.</summary>
        public bool IsDragDestinationValid => m_DragPathValid;

        private ProxyAction m_ApplyAction;
        private ProxyAction m_CancelAction;
        private ProxyAction m_ToggleAction;
        private AdrRoadGroupingController m_Controller;
        private AdrRoadGroupingOverlayRenderSystem m_Overlay;
        private AdrNameAggregateMembershipSystem m_Membership;

        private bool m_Dragging;
        private Entity m_DragStartNode;
        private Entity m_DragHoverNode;
        private Entity m_DragHoverEdge;
        private bool m_DragPathValid;
        private AdrNetKindJoinResult m_DragRejectReason;
        private Entity m_PendingFixEdge;
        private float m_PendingFixTime;
        private const float DoubleClickSeconds = 0.35f;
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
            m_Membership = World.GetOrCreateSystemManaged<AdrNameAggregateMembershipSystem>();
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
            ClearFixedMembership();
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
            ClearFixedMembership();
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
                ClearFixedMembership();
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

            // Tool focus is ADR-only — coerce leftover vanilla Aggregate selections.
            if (selected != Entity.Null && !m_Membership.IsAdrNameAggregate(selected))
            {
                if (EntityManager.HasComponent<Aggregate>(selected)
                    && m_Membership.TryForkFromVanillaSeed(selected, out Entity coercedAdr))
                {
                    selected = coercedAdr;
                    m_Controller.SelectedAggregation.Value = coercedAdr;
                    m_Controller.InvalidateSegmentCards();
                }
                else
                {
                    selected = Entity.Null;
                    m_Controller.ClearSelection();
                }
            }

            // With an ADR focus, still hover non-Aggregated nets so unsupported kinds can show red reject wash.
            if (collide
                && HoveredEdge == Entity.Null
                && selected != Entity.Null
                && m_Membership.IsAdrNameAggregate(selected)
                && TryResolveAnyEdge(entity, out Entity anyEdge))
            {
                HoveredEdge = anyEdge;
            }

            if (selected == Entity.Null)
            {
                ClearFixedMembership();
            }
            else if (FixedMembershipEdge != Entity.Null
                     && !IsMemberEdge(FixedMembershipEdge, selected))
            {
                ClearFixedMembership();
            }

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
                    m_DragHoverEdge = ResolveHoverEdge(entity);
                    HoveredDragHandle = m_DragHoverNode;
                    RefreshLivePathPreview(selected);
                }
                else
                {
                    // No valid drop under cursor (UI / empty space) — cancel preview; release will no-op.
                    InvalidateDragDestination();
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
                    && m_Membership.TryEnsureAdrForEdge(eligibleEdge, out Entity adrFocus))
                {
                    m_Controller.SelectedAggregation.Value = adrFocus;
                    m_Controller.InvalidateSegmentCards();
                }

                return inputDeps;
            }

            // Group focused: dedicated Ctrl+Apply binding toggles; plain Apply starts end-handle drag
            // or double-clicks a member to pin/unpin fixed membership.
            if (m_ToggleAction.WasPressedThisFrame())
            {
                ClearPendingFixClick();
                if (eligibleEdge != Entity.Null
                    && EntityManager.HasComponent<Edge>(entity))
                {
                    ApplyMembershipToggle(eligibleEdge, selected);
                }

                return inputDeps;
            }

            if (!m_ApplyAction.WasPressedThisFrame())
            {
                return inputDeps;
            }

            if (HoveredDragHandle != Entity.Null)
            {
                ClearPendingFixClick();
                BeginEndpointDrag(selected, HoveredDragHandle);
                return inputDeps;
            }

            if (eligibleEdge != Entity.Null
                && EntityManager.HasComponent<Edge>(eligibleEdge)
                && IsMemberEdge(eligibleEdge, selected)
                && IsDoubleClickOn(eligibleEdge))
            {
                ToggleFixedMembership(eligibleEdge, selected);
            }

            return inputDeps;
        }

        private bool IsDoubleClickOn(Entity edge)
        {
            float now = UnityEngine.Time.unscaledTime;
            if (edge == m_PendingFixEdge && now - m_PendingFixTime <= DoubleClickSeconds)
            {
                ClearPendingFixClick();
                return true;
            }

            m_PendingFixEdge = edge;
            m_PendingFixTime = now;
            return false;
        }

        private void ClearPendingFixClick()
        {
            m_PendingFixEdge = Entity.Null;
            m_PendingFixTime = 0f;
        }

        private void RefreshLivePathPreview(Entity selectedAggregate)
        {
            m_PathEdges.Clear();
            m_PreviewAddEdges.Clear();
            m_PreviewRemoveEdges.Clear();
            m_DragPathValid = false;
            m_DragRejectReason = AdrNetKindJoinResult.Ok;

            if (selectedAggregate == Entity.Null
                || m_DragStartNode == Entity.Null
                || m_DragHoverNode == Entity.Null
                || m_DragHoverNode == m_DragStartNode)
            {
                return;
            }

            RefreshMembershipSet(selectedAggregate);

            // Dropping onto a non-joinable edge fails the whole drag (no partial shrink/extend).
            if (!TryValidateDragDestinationEdge(selectedAggregate, m_DragHoverEdge, out m_DragRejectReason))
            {
                return;
            }

            if (!TryBuildPreferredPath(m_DragStartNode, m_DragHoverNode, selectedAggregate, m_PathEdges))
            {
                return;
            }

            if (!TryValidateDragPathAdds(selectedAggregate, m_PathEdges, out m_DragRejectReason))
            {
                m_PathEdges.Clear();
                return;
            }

            ClassifyPathMembership(m_PathEdges, m_PreviewAddEdges, m_PreviewRemoveEdges);
            m_DragPathValid = true;
        }

        private void BeginEndpointDrag(Entity selectedAggregate, Entity handleNode)
        {
            m_Dragging = true;
            m_DragStartNode = handleNode;
            m_DragHoverNode = handleNode;
            m_DragHoverEdge = Entity.Null;
            m_DragPathValid = false;
            m_DragRejectReason = AdrNetKindJoinResult.Ok;
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
            bool pathValid = m_DragPathValid;
            AdrNetKindJoinResult reject = m_DragRejectReason;
            // Snapshot preview lists before ClearDrag wipes them.
            var addEdges = new List<Entity>(m_PreviewAddEdges);
            var removeEdges = new List<Entity>(m_PreviewRemoveEdges);
            ClearDrag(keepMembership: true);

            if (selectedAggregate == Entity.Null
                || startNode == Entity.Null
                || hoverNode == Entity.Null
                || hoverNode == startNode)
            {
                m_MembershipEdges.Clear();
                return;
            }

            if (!pathValid)
            {
                if (reject != AdrNetKindJoinResult.Ok)
                {
                    NotifyNetKindReject(reject);
                }
                else
                {
                    LogUtils.DoInfoLog(
                        "RoadGrouping endpoint drag aborted: could not reach drop destination from node {0}:{1} to {2}:{3}",
                        startNode.Index,
                        startNode.Version,
                        hoverNode.Index,
                        hoverNode.Version);
                }

                m_MembershipEdges.Clear();
                return;
            }

            // Atomic apply — ApplyPathMembership aborts entirely if any add would fail.
            ApplyPathMembership(selectedAggregate, addEdges, removeEdges);
            m_MembershipEdges.Clear();
        }

        private void InvalidateDragDestination()
        {
            m_DragHoverNode = Entity.Null;
            m_DragHoverEdge = Entity.Null;
            m_DragPathValid = false;
            m_DragRejectReason = AdrNetKindJoinResult.Ok;
            m_PathEdges.Clear();
            m_PreviewAddEdges.Clear();
            m_PreviewRemoveEdges.Clear();
        }

        private void ClearDrag(bool keepMembership = false)
        {
            m_Dragging = false;
            m_DragStartNode = Entity.Null;
            m_DragHoverNode = Entity.Null;
            m_DragHoverEdge = Entity.Null;
            m_DragPathValid = false;
            m_DragRejectReason = AdrNetKindJoinResult.Ok;
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
            if (aggregate == Entity.Null)
            {
                return;
            }

            if (m_Membership.IsAdrNameAggregate(aggregate)
                && EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<ADRNameAggregateElement> adrElements))
            {
                for (int i = 0; i < adrElements.Length; i++)
                {
                    m_MembershipEdges.Add(adrElements[i].m_Edge);
                }
                return;
            }

            if (!EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements))
            {
                return;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                // Vanilla focus excludes edges already claimed by an ADR name-aggregate.
                if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, elements[i].m_Edge, out _))
                {
                    continue;
                }
                m_MembershipEdges.Add(elements[i].m_Edge);
            }
        }

        private void ApplyMembershipToggle(Entity edge, Entity selected)
        {
            Entity working = selected;
            if (!m_Membership.IsAdrNameAggregate(working))
            {
                // Select normally materializes ADR; keep a seed fallback if focus is still vanilla.
                if (!m_Membership.TryForkFromVanillaSeed(working, out Entity adrAggregate))
                {
                    return;
                }

                working = adrAggregate;
                m_Controller.SelectedAggregation.Value = adrAggregate;
            }

            bool currentlyMember = IsMemberEdge(edge, working);
            if (!currentlyMember)
            {
                AdrNetKindJoinResult kindJoin = m_Membership.EvaluateNetKindJoin(working, edge);
                if (kindJoin != AdrNetKindJoinResult.Ok)
                {
                    NotifyNetKindReject(kindJoin);
                    return;
                }
            }

            bool ok = currentlyMember
                ? m_Membership.RemoveMember(working, edge)
                : m_Membership.AddMember(working, edge);
            if (!ok)
            {
                return;
            }

            if (!m_Membership.IsAdrNameAggregate(working))
            {
                ClearFixedMembership();
                m_Controller.ClearSelection();
            }
            else
            {
                m_Controller.InvalidateSegmentCards();
            }
        }

        private void ApplyPathMembership(Entity selected, List<Entity> addEdges, List<Entity> removeEdges)
        {
            if ((addEdges == null || addEdges.Count == 0)
                && (removeEdges == null || removeEdges.Count == 0))
            {
                return;
            }

            Entity working = selected;

            if (!m_Membership.IsAdrNameAggregate(working))
            {
                if (!m_Membership.TryForkFromVanillaSeed(working, out Entity adrAggregate))
                {
                    return;
                }

                working = adrAggregate;
                m_Controller.SelectedAggregation.Value = adrAggregate;
            }

            // All-or-nothing: if any would-add fails kind join, apply neither adds nor removes.
            if (addEdges != null)
            {
                for (int i = 0; i < addEdges.Count; i++)
                {
                    AdrNetKindJoinResult kindJoin = m_Membership.EvaluateNetKindJoin(working, addEdges[i]);
                    if (kindJoin != AdrNetKindJoinResult.Ok)
                    {
                        NotifyNetKindReject(kindJoin);
                        return;
                    }
                }
            }

            if (removeEdges != null)
            {
                for (int i = 0; i < removeEdges.Count; i++)
                {
                    m_Membership.RemoveMember(working, removeEdges[i]);
                    if (!m_Membership.IsAdrNameAggregate(working))
                    {
                        ClearFixedMembership();
                        m_Controller.ClearSelection();
                        return;
                    }
                }
            }

            if (addEdges != null && m_Membership.IsAdrNameAggregate(working))
            {
                for (int i = 0; i < addEdges.Count; i++)
                {
                    if (!m_Membership.AddMember(working, addEdges[i]))
                    {
                        // Unexpected after preflight — treat as failed drag (membership may be partial).
                        LogUtils.DoWarnLog(
                            "RoadGrouping endpoint drag: AddMember failed after preflight for edge {0}:{1}",
                            addEdges[i].Index,
                            addEdges[i].Version);
                        NotifyNetKindReject(AdrNetKindJoinResult.Mismatch);
                        return;
                    }
                }
            }

            if (!m_Membership.IsAdrNameAggregate(working))
            {
                ClearFixedMembership();
                m_Controller.ClearSelection();
                return;
            }

            m_Membership.ResortMembers(working);
            m_Controller.InvalidateSegmentCards();
        }

        private void ToggleFixedMembership(Entity edge, Entity aggregate)
        {
            if (FixedMembershipEdge == edge)
            {
                ClearFixedMembership();
                return;
            }

            FixedMembershipEdge = edge;
        }

        public void ClearFixedMembership()
        {
            FixedMembershipEdge = Entity.Null;
            ClearPendingFixClick();
        }

        private void NotifyNetKindReject(AdrNetKindJoinResult result)
        {
            if (result == AdrNetKindJoinResult.Ok)
            {
                return;
            }

            string textKey = result == AdrNetKindJoinResult.Unclassified
                ? "K45::ADR.vuio[roadGrouping.kindUnclassified]"
                : "K45::ADR.vuio[roadGrouping.kindMismatch]";
            const string titleKey = "K45::ADR.vuio[roadGrouping.toolOptions.title]";
            const string notificationId = "K45_ADR_roadGrouping_kindReject";

            NotificationSystem.Pop(
                notificationId,
                4f,
                LocalizedString.Id(titleKey),
                LocalizedString.Id(textKey),
                progressState: ProgressState.Warning);

            LogUtils.DoInfoLog(
                "RoadGrouping net-kind reject: {0} (notification {1})",
                result,
                textKey);
        }

        private bool IsMemberEdge(Entity edge, Entity aggregate)
        {
            if (edge == Entity.Null || aggregate == Entity.Null)
            {
                return false;
            }

            if (m_Membership.IsAdrNameAggregate(aggregate))
            {
                if (EntityManager.TryGetComponent(edge, out ADRNameAggregated link)
                    && link.m_Aggregate == aggregate)
                {
                    return true;
                }

                if (!EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<ADRNameAggregateElement> adrElements))
                {
                    return false;
                }

                for (int i = 0; i < adrElements.Length; i++)
                {
                    if (adrElements[i].m_Edge == edge)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (!EntityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements))
            {
                return false;
            }

            // ADR-linked edges are not part of the vanilla Aggregate focus set.
            if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, edge, out _))
            {
                return false;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].m_Edge == edge)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Split path into would-add (non-members) and would-remove (members).
        /// Mixed paths happen when the new tip pivots through the current Aggregate
        /// (walk back along the tip, then hop onto a different direction) — both lists apply.
        /// Fixed membership edge is never removed.
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
                    if (edge != FixedMembershipEdge)
                    {
                        removeOut.Add(edge);
                    }
                }
                else
                {
                    addOut.Add(edge);
                }
            }
        }

        /// <summary>
        /// Drop target edge must already be a member or be joinable by kind.
        /// Null edge (node-only hover) is allowed — path validation still applies.
        /// </summary>
        private bool TryValidateDragDestinationEdge(
            Entity aggregate,
            Entity hoverEdge,
            out AdrNetKindJoinResult reject)
        {
            reject = AdrNetKindJoinResult.Ok;
            if (hoverEdge == Entity.Null || !EntityManager.Exists(hoverEdge))
            {
                return true;
            }

            if (IsMemberEdge(hoverEdge, aggregate) || m_MembershipEdges.Contains(hoverEdge))
            {
                return true;
            }

            reject = m_Membership.EvaluateNetKindJoin(aggregate, hoverEdge);
            return reject == AdrNetKindJoinResult.Ok;
        }

        /// <summary>
        /// Every non-member edge on the would-apply path must be joinable; otherwise the drag fails wholly.
        /// </summary>
        private bool TryValidateDragPathAdds(
            Entity aggregate,
            List<Entity> pathEdges,
            out AdrNetKindJoinResult reject)
        {
            reject = AdrNetKindJoinResult.Ok;
            if (pathEdges == null)
            {
                return true;
            }

            for (int i = 0; i < pathEdges.Count; i++)
            {
                Entity edge = pathEdges[i];
                if (m_MembershipEdges.Contains(edge) || IsMemberEdge(edge, aggregate))
                {
                    continue;
                }

                reject = m_Membership.EvaluateNetKindJoin(aggregate, edge);
                if (reject != AdrNetKindJoinResult.Ok)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 0-1 BFS: cost 0 when next edge is already in focused membership (or shares prefer Aggregate); cost 1 on hop.
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
                    // Fixed pin blocks traversal so endpoint drag cannot shrink past it.
                    if (FixedMembershipEdge != Entity.Null && edgeEntity == FixedMembershipEdge)
                    {
                        continue;
                    }

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

                    bool inFocusedMembership = m_MembershipEdges.Contains(edgeEntity);
                    int stepCost = inFocusedMembership || (edgeAgg != Entity.Null && edgeAgg == prefer) ? 0 : 1;
                    int nextCost = nodeCost + stepCost;
                    if (m_PathCost.TryGetValue(next, out int existing) && existing <= nextCost)
                    {
                        continue;
                    }

                    m_PathCost[next] = nextCost;
                    m_CameFromEdge[next] = edgeEntity;
                    m_CameFromNode[next] = node;
                    m_PreferAggregateAtNode[next] = inFocusedMembership
                        ? selectedAggregate
                        : (edgeAgg != Entity.Null ? edgeAgg : prefer);

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

        private Entity ResolveHoverEdge(Entity hitEntity)
        {
            // Only an explicit edge hit is a destination segment. Node hits leave edge null;
            // path-add validation still rejects unjoinable edges on the walk.
            return TryResolveAnyEdge(hitEntity, out Entity edge) ? edge : Entity.Null;
        }

        private bool TryResolveAnyEdge(Entity hit, out Entity edge)
        {
            edge = Entity.Null;
            if (EntityManager.HasComponent<Edge>(hit))
            {
                edge = hit;
                return true;
            }

            return false;
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
