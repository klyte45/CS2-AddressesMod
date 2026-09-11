using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Net;
using Game.Rendering;
using Game.SceneFlow;
using Game.Tools;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BelzontAdr
{
    public partial class AdrRoadGroupingController : SystemBase, IBelzontBindable
    {
        private const string PREFIX = "roadGrouping.";
        private static readonly AdrRoadGroupingSegmentCard[] EmptyCards = Array.Empty<AdrRoadGroupingSegmentCard>();

        private Action<string, object[]> m_eventCaller;
        private Action<string, Delegate> m_callBinder;
        private bool m_initialized;

        private ToolSystem m_ToolSystem;
        private AdrRoadGroupingTool m_Tool;
        private AdrMainSystem m_AdrMainSystem;
        private AdrHighwayRoutesSystem m_HighwayRoutesSystem;
        private CameraUpdateSystem m_CameraUpdateSystem;

        private Entity m_LastAggregation = Entity.Null;
        private bool m_LastShowNumberCards;
        private bool m_CardsDirty;

        public MultiUIValueBinding<Entity> SelectedAggregation { get; private set; }
        public MultiUIValueBinding<bool> ShowNumberCards { get; private set; }
        public MultiUIValueBinding<AdrRoadGroupingSegmentCard[]> SegmentCards { get; private set; }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
            if (m_callBinder != null) InitValueBindings();
        }

        public void SetupEventBinder(Action<string, Delegate> eventBinder)
        {
        }

        public void SetupCallBinder(Action<string, Delegate> callBinder)
        {
            m_callBinder = callBinder;
            callBinder($"{PREFIX}enableTool", OnEnableTool);
            callBinder($"{PREFIX}disableTool", OnDisableTool);
            if (m_eventCaller != null) InitValueBindings();
        }

        private void InitValueBindings()
        {
            if (m_initialized) return;
            m_initialized = true;

            // Preserve state across UI reloads (BindingsReleased → re-init).
            var selected = SelectedAggregation?.Value ?? Entity.Null;
            var showCards = ShowNumberCards?.Value ?? false;
            var cards = SegmentCards?.Value ?? EmptyCards;

            SelectedAggregation = new(selected, $"{PREFIX}{nameof(SelectedAggregation)}", m_eventCaller, m_callBinder);
            ShowNumberCards = new(showCards, $"{PREFIX}{nameof(ShowNumberCards)}", m_eventCaller, m_callBinder);
            SegmentCards = new(cards, $"{PREFIX}{nameof(SegmentCards)}", m_eventCaller, m_callBinder, isReadOnlyForUI: true);
            SelectedAggregation.OnScreenValueChanged += _ => m_CardsDirty = true;
            ShowNumberCards.OnScreenValueChanged += _ => m_CardsDirty = true;
        }

        private void OnEnableTool()
        {
            World.GetOrCreateSystemManaged<AdrRoadGroupingTool>().Select();
        }

        private void OnDisableTool()
        {
            World.GetOrCreateSystemManaged<AdrRoadGroupingTool>().RequestDisable();
        }

        public void ClearSelection()
        {
            if (SelectedAggregation != null)
            {
                SelectedAggregation.Value = Entity.Null;
            }
        }

        /// <summary>
        /// Call when membership list for the focused aggregation changes (T5); rebuilds ranges immediately.
        /// </summary>
        public void InvalidateSegmentCards()
        {
            m_CardsDirty = true;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_Tool = World.GetOrCreateSystemManaged<AdrRoadGroupingTool>();
            m_AdrMainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
            m_HighwayRoutesSystem = World.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();
            m_CameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            // Same as WEWorldPickerController: allow InitValueBindings to re-run after UI reload.
            GameManager.instance.userInterface.view.Listener.BindingsReleased += () => m_initialized = false;
        }

        protected override void OnUpdate()
        {
            if (!m_initialized || SegmentCards == null)
            {
                return;
            }

            bool toolActive = m_ToolSystem.activeTool == m_Tool;
            Entity aggregation = toolActive && SelectedAggregation != null ? SelectedAggregation.Value : Entity.Null;
            bool showCards = toolActive && ShowNumberCards != null && ShowNumberCards.Value && aggregation != Entity.Null;

            if (aggregation != m_LastAggregation || showCards != m_LastShowNumberCards)
            {
                m_LastAggregation = aggregation;
                m_LastShowNumberCards = showCards;
                m_CardsDirty = true;
            }

            if (!showCards)
            {
                if (m_CardsDirty || (SegmentCards.Value != null && SegmentCards.Value.Length > 0))
                {
                    PushCards(EmptyCards);
                    m_CardsDirty = false;
                }
                return;
            }

            // Ranges rebuild on aggregation/toggle dirty; viewport projection every frame.
            PublishCards(aggregation, rebuildRanges: m_CardsDirty);
            m_CardsDirty = false;
        }

        private void PublishCards(Entity aggregation, bool rebuildRanges)
        {
            if (!EntityManager.TryGetBuffer(aggregation, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                PushCards(EmptyCards);
                return;
            }

            AdrRoadGroupingSegmentCard[] previous = SegmentCards.Value;
            bool canReuseRanges = !rebuildRanges
                                  && previous != null
                                  && previous.Length == elements.Length;

            var cards = new AdrRoadGroupingSegmentCard[elements.Length];
            var camera = ResolveCamera();

            for (int i = 0; i < elements.Length; i++)
            {
                Entity edge = elements[i].m_Edge;
                AdrRoadGroupingSegmentCard card;

                if (canReuseRanges
                    && previous[i].edge == edge
                    && previous[i].sortKey == i)
                {
                    card = previous[i];
                }
                else
                {
                    card = new AdrRoadGroupingSegmentCard
                    {
                        edge = edge,
                        sortKey = i,
                    };

                    if (EntityManager.TryGetComponent(edge, out Curve curve))
                    {
                        float3 mid = MathUtils.Position(curve.m_Bezier, 0.5f);
                        card.midX = mid.x;
                        card.midY = mid.y;
                        card.midZ = mid.z;
                    }

                    if (AdrRoadAddressNumbering.TryGetOddNumberAtCurvePos(
                            EntityManager, m_AdrMainSystem, m_HighwayRoutesSystem, edge, 0f, out int n0)
                        && AdrRoadAddressNumbering.TryGetOddNumberAtCurvePos(
                            EntityManager, m_AdrMainSystem, m_HighwayRoutesSystem, edge, 1f, out int n1))
                    {
                        card.numberLow = math.min(n0, n1);
                        card.numberHigh = math.max(n0, n1);
                    }
                }

                ProjectCard(ref card, camera);
                cards[i] = card;
            }

            PushCards(cards);
        }

        private void PushCards(AdrRoadGroupingSegmentCard[] cards)
        {
            // Always notify UI (array reference equality would skip identical empty pushes).
            SegmentCards.Value = cards ?? EmptyCards;
            SegmentCards.UpdateUIs();
        }

        private Camera ResolveCamera()
        {
            if (m_CameraUpdateSystem.activeViewer?.camera != null)
            {
                return m_CameraUpdateSystem.activeViewer.camera;
            }

            return Camera.main;
        }

        private static void ProjectCard(ref AdrRoadGroupingSegmentCard card, Camera camera)
        {
            if (camera == null)
            {
                card.viewportX = 0.5f;
                card.viewportY = 0.5f;
                card.visible = false;
                return;
            }

            var world = new Vector3(card.midX, card.midY, card.midZ);
            Vector3 viewport = camera.WorldToViewportPoint(world);
            card.viewportX = viewport.x;
            card.viewportY = viewport.y;
            // 120% viewport (10% margin); behind camera is never visible.
            card.visible = viewport.z > 0f
                           && viewport.x > -0.1f && viewport.x < 1.1f
                           && viewport.y > -0.1f && viewport.y < 1.1f;
        }
    }
}
