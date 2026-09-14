using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Common;
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

            // SIP/tool highway or custom-name writes add Updated — refresh cards while tool stays open.
            if (showCards && EntityManager.HasComponent<Updated>(aggregation))
            {
                m_CardsDirty = true;
            }

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
            if (EntityManager.HasComponent<ADRNameAggregate>(aggregation)
                && EntityManager.TryGetBuffer(aggregation, true, out DynamicBuffer<ADRNameAggregateElement> adrElements)
                && adrElements.Length > 0)
            {
                PublishCardsAdr(aggregation, adrElements, rebuildContent);
                return;
            }

            if (!EntityManager.TryGetBuffer(aggregation, true, out DynamicBuffer<AggregateElement> elements)
                || elements.Length == 0)
            {
                PushCards(EmptyCards);
                return;
            }

            // Vanilla focus: omit edges already owned by an ADR name-aggregate.
            var edges = new List<Entity>(elements.Length);
            for (int i = 0; i < elements.Length; i++)
            {
                Entity edge = elements[i].m_Edge;
                if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, edge, out _))
                {
                    continue;
                }
                edges.Add(edge);
            }

            if (edges.Count == 0)
            {
                PushCards(EmptyCards);
                return;
            }

            var camera = ResolveCamera();
            int visibleSegments = CountVisibleSegments(edges, camera);

            if (visibleSegments > AdrRoadAddressNumbering.DenseSegmentCardThreshold)
            {
                PushCards(BuildMileageCardsFromEdges(edges, camera));
                return;
            }

            PushCards(BuildSegmentRangeCardsFromEdges(edges.Count, i => edges[i], camera, rebuildContent));
        }

        private void PublishCardsAdr(
            Entity aggregation,
            DynamicBuffer<ADRNameAggregateElement> elements,
            bool rebuildContent)
        {
            var camera = ResolveCamera();
            int visibleSegments = CountVisibleSegments(elements, camera);
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
                if (IsEdgeMidVisible(elements[i].m_Edge, camera))
                {
                    visible++;
                }
            }

            return visible;
        }

        private int CountVisibleSegments(List<Entity> edges, Camera camera)
        {
            int visible = 0;
            for (int i = 0; i < edges.Count; i++)
            {
                if (IsEdgeMidVisible(edges[i], camera))
                {
                    visible++;
                }
            }

            return visible;
        }

        private int CountVisibleSegments(DynamicBuffer<ADRNameAggregateElement> elements, Camera camera)
        {
            int visible = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                if (IsEdgeMidVisible(elements[i].m_Edge, camera))
                {
                    visible++;
                }
            }

            return visible;
        }

        private bool IsEdgeMidVisible(Entity edge, Camera camera)
        {
            if (!EntityManager.TryGetComponent(edge, out Curve curve))
            {
                return false;
            }

            float3 mid = MathUtils.Position(curve.m_Bezier, 0.5f);
            return IsWorldVisible(mid, camera);
        }

        private AdrRoadGroupingSegmentCard[] BuildSegmentRangeCards(
            DynamicBuffer<AggregateElement> elements,
            Camera camera,
            bool rebuildContent)
        {
            return BuildSegmentRangeCardsFromEdges(elements.Length, i => elements[i].m_Edge, camera, rebuildContent);
        }

        private AdrRoadGroupingSegmentCard[] BuildSegmentRangeCards(
            DynamicBuffer<ADRNameAggregateElement> elements,
            Camera camera,
            bool rebuildContent)
        {
            return BuildSegmentRangeCardsFromEdges(elements.Length, i => elements[i].m_Edge, camera, rebuildContent);
        }

        private AdrRoadGroupingSegmentCard[] BuildSegmentRangeCardsFromEdges(
            int length,
            Func<int, Entity> edgeAt,
            Camera camera,
            bool rebuildContent)
        {
            AdrRoadGroupingSegmentCard[] previous = SegmentCards.Value;
            bool canReuse = !rebuildContent
                            && previous != null
                            && previous.Length == length
                            && previous.Length > 0
                            && previous[0].label != null
                            && !previous[0].label.StartsWith("km ", StringComparison.Ordinal)
                            && !previous[0].label.StartsWith("mi ", StringComparison.Ordinal);

            var culture = AdrRoadAddressNumbering.GetUiCulture();
            var cards = new AdrRoadGroupingSegmentCard[length];

            for (int i = 0; i < length; i++)
            {
                Entity edge = edgeAt(i);
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
            DynamicBuffer<ADRNameAggregateElement> elements,
            Camera camera)
        {
            var hwCtx = AdrRoadAddressNumbering.HighwayNumberingContext.FromAdrAggregate(
                EntityManager, m_AdrMainSystem, m_HighwayRoutesSystem, aggregation);

            if (!AdrRoadAddressNumbering.TryResolveInverseAggregate(
                    EntityManager, elements, hwCtx.OrientationRef,
                    out bool isInverseAggregate))
            {
                return EmptyCards;
            }

            return BuildMileageCardsCore(
                elements.Length,
                i => elements[i].m_Edge,
                (i, edge) => AdrRoadAddressNumbering.CheckSegmentReversion(
                    EntityManager, elements, isInverseAggregate, i, elements[i]),
                isInverseAggregate,
                hwCtx,
                camera);
        }

        private AdrRoadGroupingSegmentCard[] BuildMileageCards(
            Entity aggregation,
            DynamicBuffer<AggregateElement> elements,
            Camera camera)
        {
            if (!AdrRoadAddressNumbering.TryResolveInverseAggregate(
                    EntityManager, m_AdrMainSystem, elements,
                    out bool isInverseAggregate))
            {
                return EmptyCards;
            }

            return BuildMileageCardsCore(
                elements.Length,
                i => elements[i].m_Edge,
                (i, edge) => AdrRoadAddressNumbering.CheckSegmentReversion(
                    EntityManager, elements, isInverseAggregate, i, elements[i]),
                isInverseAggregate,
                AdrRoadAddressNumbering.HighwayNumberingContext.ForCityZero(m_AdrMainSystem),
                camera);
        }

        private AdrRoadGroupingSegmentCard[] BuildMileageCardsFromEdges(List<Entity> edges, Camera camera)
        {
            if (edges == null || edges.Count == 0
                || !TryResolveInverseFromEdges(edges, out bool isInverseAggregate))
            {
                return EmptyCards;
            }

            return BuildMileageCardsCore(
                edges.Count,
                i => edges[i],
                (i, edge) => CheckSegmentReversionFromEdges(edges, isInverseAggregate, i),
                isInverseAggregate,
                AdrRoadAddressNumbering.HighwayNumberingContext.ForCityZero(m_AdrMainSystem),
                camera);
        }

        private bool TryResolveInverseFromEdges(List<Entity> edges, out bool isInverseAggregate)
        {
            isInverseAggregate = false;
            if (edges.Count == 0
                || !EntityManager.TryGetComponent(edges[0], out Curve e0)
                || !EntityManager.TryGetComponent(edges[edges.Count - 1], out Curve e1))
            {
                return false;
            }

            var refE0 = CheckSegmentReversionFromEdges(edges, false, 0)
                ? e0.m_Bezier.d.xz
                : e0.m_Bezier.a.xz;
            var refE1 = CheckSegmentReversionFromEdges(edges, false, edges.Count - 1)
                ? e1.m_Bezier.a.xz
                : e1.m_Bezier.d.xz;
            var zeroMarker = m_AdrMainSystem.GetZeroMarkerPosition();
            isInverseAggregate = math.distancesq(refE0, zeroMarker) > math.distancesq(refE1, zeroMarker);
            return true;
        }

        private bool CheckSegmentReversionFromEdges(List<Entity> edges, bool isInverseAggregate, int i)
        {
            bool isReversedSegment = false;
            if (i > 0)
            {
                if (EntityManager.TryGetComponent(edges[i], out Edge edge2)
                    && EntityManager.TryGetComponent(edges[i - 1], out Edge edge3)
                    && (edge2.m_End == edge3.m_Start || edge2.m_End == edge3.m_End))
                {
                    isReversedSegment = true;
                }
            }
            else if (i < edges.Count - 1
                     && EntityManager.TryGetComponent(edges[i], out Edge edge4)
                     && EntityManager.TryGetComponent(edges[i + 1], out Edge edge5)
                     && (edge4.m_Start == edge5.m_Start || edge4.m_Start == edge5.m_End))
            {
                isReversedSegment = true;
            }

            if (isInverseAggregate)
            {
                isReversedSegment = !isReversedSegment;
            }

            return isReversedSegment;
        }

        private AdrRoadGroupingSegmentCard[] BuildMileageCardsCore(
            int length,
            Func<int, Entity> edgeAt,
            Func<int, Entity, bool> isReversedAt,
            bool isInverseAggregate,
            AdrRoadAddressNumbering.HighwayNumberingContext hwCtx,
            Camera camera)
        {
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

            for (int i = isInverseAggregate ? length - 1 : 0;
                 isInverseAggregate ? i >= 0 : i < length;
                 i += isInverseAggregate ? -1 : 1)
            {
                Entity edge = edgeAt(i);
                if (!EntityManager.TryGetComponent(edge, out Curve curve) || curve.m_Length < 0.01f)
                {
                    continue;
                }

                float newDistance = currentDistance + curve.m_Length;
                bool reversed = isReversedAt(i, edge);

                while (nextMarker <= newDistance + 0.01f)
                {
                    float local = math.saturate((nextMarker - currentDistance) / curve.m_Length);
                    float curvePos = reversed ? 1f - local : local;
                    float3 pos = MathUtils.Position(curve.m_Bezier, curvePos);
                    float majorUnits = AdrRoadAddressNumbering.ComputeMileageMajorUnits(
                        nextMarker, hwCtx, metersPerMajor);

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
