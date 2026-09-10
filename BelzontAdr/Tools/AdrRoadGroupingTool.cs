using Belzont.Interfaces;
using Colossal.Entities;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Notifications;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using Unity.Jobs;

namespace BelzontAdr
{
    public partial class AdrRoadGroupingTool : IBelzontToolSystem
    {
        public const string TOOL_ID = "K45_ADR_AdrRoadGroupingTool";

        public override string toolID => TOOL_ID;

        public bool IsSelected => m_ToolSystem.activeTool == this;

        public Entity HoveredEdge { get; private set; }

        private ProxyAction m_ApplyAction;
        private ProxyAction m_CancelAction;
        private AdrRoadGroupingController m_Controller;

        protected override void OnCreateWithBarrier()
        {
            Enabled = false;
            m_ApplyAction = AdrModData.Instance.GetAction(AdrModData.kActionApplyMouse);
            m_CancelAction = AdrModData.Instance.GetAction(AdrModData.kActionCancelMouse);
            m_Controller = World.GetOrCreateSystemManaged<AdrRoadGroupingController>();
        }

        public override PrefabBase GetPrefab() => null;

        public override bool TrySetPrefab(PrefabBase prefab) => false;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            m_ApplyAction.shouldBeEnabled = true;
            m_CancelAction.shouldBeEnabled = true;
            cancelAction.shouldBeEnabled = true; // Escape (Tool "Cancel")
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            m_ApplyAction.shouldBeEnabled = false;
            m_CancelAction.shouldBeEnabled = false;
            cancelAction.shouldBeEnabled = false;
            HoveredEdge = Entity.Null;
            m_Controller.ClearSelection();
        }

        public void Select()
        {
            m_ToolSystem.activeTool = this;
        }

        public void RequestDisable()
        {
            m_Controller.ClearSelection();
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        /// <summary>
        /// RMB / Escape: clear selection if focused; otherwise leave the tool.
        /// </summary>
        private void HandleCancel()
        {
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

            bool collide = GetRaycastResult(out Entity entity, out RaycastHit _);
            Entity eligibleEdge = Entity.Null;
            if (collide && TryResolveAggregatedEdge(entity, out eligibleEdge, out _))
            {
                HoveredEdge = eligibleEdge;
            }
            else
            {
                HoveredEdge = Entity.Null;
            }

            if (!InputManager.instance.mouseOverUI
                && m_ApplyAction.WasPressedThisFrame()
                && m_Controller.SelectedAggregation.Value == Entity.Null
                && eligibleEdge != Entity.Null
                && EntityManager.TryGetComponent(eligibleEdge, out Aggregated aggregated))
            {
                m_Controller.SelectedAggregation.Value = aggregated.m_Aggregate;
            }

            return inputDeps;
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
