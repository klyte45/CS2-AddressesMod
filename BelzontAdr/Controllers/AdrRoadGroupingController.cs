using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Net;
using Game.Rendering;
using Game.SceneFlow;
using Game.Tools;
using System;
using System.Collections.Generic;
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

            var selected = SelectedAggregation?.Value ?? Entity.Null;
            var showCards = ShowNumberCards?.Value ?? true;
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

            PublishCards(aggregation, rebuildContent: m_CardsDirty);
            m_CardsDirty = false;
        }

        private void PublishCards(Entity aggregation, bool rebuildContent)
        {
            if (!EntityManager.TryGetBuffer(aggregation, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                PushCards(EmptyCards);
                return;
            }

            var camera = ResolveCamera();
            int visibleSegments = CountVisibleSegments(elements, camera);

            // Dense screen: half-km / half-mi markers instead of per-segment house ranges.
            if (visibleSegments > AdrRoadAddressNumbering.DenseSegmentCardThreshold)
            {
                PushCards(BuildMileageCards(aggregation, elements, camera));
                return;
            }

            PushCards(BuildSegmentRangeCards(elements, camera, rebuildContent));
        }

        private int CountVisibleSegments(DynamicBuffer<AggregateElement> elements, Camera camera)
        {
            int visible = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                Entity edge = elements[i].m_Edge;
                if (!EntityManager.TryGetComponent(edge, out Curve curve))
                {
                    continue;
                }

                float3 mid = MathUtils.Position(curve.m_Bezier, 0.5f);
                if (IsWorldVisible(mid, camera))
                {
                    visible++;
                }
            }

            return visible;
        }

        private AdrRoadGroupingSegmentCard[] BuildSegmentRangeCards(
            DynamicBuffer<AggregateElement> elements,
            Camera camera,
            bool rebuildContent)
        {
            AdrRoadGroupingSegmentCard[] previous = SegmentCards.Value;
            bool canReuse = !rebuildContent
                            && previous != null
                            && previous.Length == elements.Length
                            && previous.Length > 0
                            && previous[0].label != null
                            && !previous[0].label.StartsWith("km ", StringComparison.Ordinal)
                            && !previous[0].label.StartsWith("mi ", StringComparison.Ordinal);

            var culture = AdrRoadAddressNumbering.GetUiCulture();
            var cards = new AdrRoadGroupingSegmentCard[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                Entity edge = elements[i].m_Edge;
                AdrRoadGroupingSegmentCard card;

                if (canReuse && previous[i].edge == edge && previous[i].sortKey == i)
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
                        card.label = AdrRoadAddressNumbering.FormatHouseNumberRange(
                            card.numberLow, card.numberHigh, culture);
                    }
                    else
                    {
                        card.label = string.Empty;
                    }
                }

                ProjectCard(ref card, camera);
                cards[i] = card;
            }

            return cards;
        }

        private AdrRoadGroupingSegmentCard[] BuildMileageCards(
            Entity aggregation,
            DynamicBuffer<AggregateElement> elements,
            Camera camera)
        {
            if (!AdrRoadAddressNumbering.TryResolveInverseAggregate(
                    EntityManager, m_AdrMainSystem, m_HighwayRoutesSystem, aggregation, elements,
                    out bool isInverseAggregate))
            {
                return EmptyCards;
            }

            bool freedom = AdrRoadAddressNumbering.IsFreedomUnitSystem();
            float stepMeters = freedom
                ? AdrRoadAddressNumbering.HalfMileMeters
                : AdrRoadAddressNumbering.HalfKilometerMeters;
            float metersPerMajor = freedom ? (AdrRoadAddressNumbering.HalfMileMeters * 2f) : 1000f;
            var culture = AdrRoadAddressNumbering.GetUiCulture();
            var list = new List<AdrRoadGroupingSegmentCard>(32);

            float currentDistance = 0f;
            float nextMarker = 0f;
            int sortKey = 0;

            for (int i = isInverseAggregate ? elements.Length - 1 : 0;
                 isInverseAggregate ? i >= 0 : i < elements.Length;
                 i += isInverseAggregate ? -1 : 1)
            {
                AggregateElement element = elements[i];
                Entity edge = element.m_Edge;
                if (!EntityManager.TryGetComponent(edge, out Curve curve) || curve.m_Length < 0.01f)
                {
                    continue;
                }

                float newDistance = currentDistance + curve.m_Length;
                bool reversed = AdrRoadAddressNumbering.CheckSegmentReversion(
                    EntityManager, elements, isInverseAggregate, i, element);

                while (nextMarker <= newDistance + 0.01f)
                {
                    float local = math.saturate((nextMarker - currentDistance) / curve.m_Length);
                    float curvePos = reversed ? 1f - local : local;
                    float3 pos = MathUtils.Position(curve.m_Bezier, curvePos);
                    float majorUnits = nextMarker / metersPerMajor;

                    var card = new AdrRoadGroupingSegmentCard
                    {
                        edge = edge,
                        sortKey = sortKey++,
                        midX = pos.x,
                        midY = pos.y,
                        midZ = pos.z,
                        label = AdrRoadAddressNumbering.FormatMileageLabel(freedom, majorUnits, culture),
                    };
                    ProjectCard(ref card, camera);
                    list.Add(card);

                    nextMarker += stepMeters;
                }

                currentDistance = newDistance;
            }

            return list.Count == 0 ? EmptyCards : list.ToArray();
        }

        private void PushCards(AdrRoadGroupingSegmentCard[] cards)
        {
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

        private static bool IsWorldVisible(float3 world, Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            Vector3 viewport = camera.WorldToViewportPoint((Vector3)world);
            return viewport.z > 0f
                   && viewport.x > -0.1f && viewport.x < 1.1f
                   && viewport.y > -0.1f && viewport.y < 1.1f;
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
            card.visible = viewport.z > 0f
                           && viewport.x > -0.1f && viewport.x < 1.1f
                           && viewport.y > -0.1f && viewport.y < 1.1f;
        }
    }
}
