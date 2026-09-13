using Colossal.Entities;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.UI;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace BelzontAdr
{
    public enum AdrNameAggregateForkEdit : byte
    {
        AddMember = 0,
        RemoveMember = 1
    }

    public partial class AdrNameAggregateMembershipSystem : SystemBase
    {
        public static AdrNameAggregateMembershipSystem Instance { get; private set; }

        private NameSystem m_nameSystem;
        private readonly Queue<Action> m_runOnUpdate = new();

        protected override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
            m_nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            while (m_runOnUpdate.Count > 0)
            {
                m_runOnUpdate.Dequeue().Invoke();
            }
        }

        public bool IsAdrNameAggregate(Entity aggregate)
            => aggregate != Entity.Null
               && EntityManager.Exists(aggregate)
               && EntityManager.HasComponent<ADRNameAggregate>(aggregate);

        public Entity CreateAggregate()
        {
            var aggregate = EntityManager.CreateEntity();
            EntityManager.AddComponent<ADRNameAggregate>(aggregate);
            EntityManager.AddBuffer<ADRNameAggregateElement>(aggregate);
            EnsureHighwayData(aggregate);
            return aggregate;
        }

        /// <summary>
        /// Always-present authored highway fields on ADR aggregates (defaults = inactive).
        /// Safe to call on existing entities loaded before HW-01.
        /// </summary>
        public void EnsureHighwayData(Entity aggregate)
        {
            if (!IsAdrNameAggregate(aggregate)
                || EntityManager.HasComponent<ADRNameAggregateHighwayData>(aggregate))
            {
                return;
            }

            EntityManager.AddComponentData(aggregate, ADRNameAggregateHighwayData.CreateDefault());
        }

        /// <summary>
        /// Attach Label* + PrefabRef so LBL-02 TMP bake can own this ADR aggregate as an S2 label owner.
        /// </summary>
        private void EnsureLabelOwnerComponents(Entity adrAggregate, Entity prefabSource)
        {
            var labelSystem = World.GetExistingSystemManaged<AdrRoadNameLabelMeshSystem>();
            labelSystem?.EnsureRoadNameLabelComponents(adrAggregate, prefabSource);
        }

        public Entity CreateAggregate(Entity[] edges)
        {
            var aggregate = CreateAggregate();
            if (edges == null)
            {
                return aggregate;
            }
            for (int i = 0; i < edges.Length; i++)
            {
                AddMemberCore(aggregate, edges[i], insertIndex: -1, allowSteal: true, out _);
            }
            ResortMembers(aggregate);
            Entity prefabSource = Entity.Null;
            if (edges.Length > 0
                && EntityManager.TryGetComponent(edges[0], out Aggregated aggregated)
                && aggregated.m_Aggregate != Entity.Null)
            {
                prefabSource = aggregated.m_Aggregate;
            }
            EnsureLabelOwnerComponents(aggregate, prefabSource);
            return aggregate;
        }

        public bool TryForkFromVanilla(
            Entity vanillaAggregate,
            AdrNameAggregateForkEdit edit,
            Entity edge,
            out Entity adrAggregate,
            int insertIndex = -1)
        {
            adrAggregate = Entity.Null;

            if (vanillaAggregate == Entity.Null
                || !EntityManager.Exists(vanillaAggregate)
                || !EntityManager.HasComponent<Aggregate>(vanillaAggregate)
                || IsAdrNameAggregate(vanillaAggregate)
                || edge == Entity.Null
                || !EntityManager.Exists(edge)
                || !EntityManager.TryGetBuffer(vanillaAggregate, true, out DynamicBuffer<AggregateElement> vanillaElements)
                || vanillaElements.Length == 0)
            {
                return false;
            }

            bool edgeInVanilla = false;
            bool edgeInAdr = AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, edge, out _);
            for (int i = 0; i < vanillaElements.Length; i++)
            {
                if (vanillaElements[i].m_Edge == edge)
                {
                    edgeInVanilla = true;
                    break;
                }
            }

            switch (edit)
            {
                case AdrNameAggregateForkEdit.AddMember:
                    // Remainder edges are already seeded; only allow add when stealing from another ADR
                    // or attaching an edge from outside this vanilla Aggregate.
                    if (edgeInVanilla && !edgeInAdr)
                    {
                        return false;
                    }
                    break;
                case AdrNameAggregateForkEdit.RemoveMember:
                    // Can only remove a remainder (non-ADR) edge that is on this vanilla Aggregate.
                    if (!edgeInVanilla || edgeInAdr || vanillaElements.Length <= 1)
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            if (!TryForkFromVanillaSeed(vanillaAggregate, out adrAggregate))
            {
                return false;
            }

            Entity stolenFrom = Entity.Null;
            bool applied = edit switch
            {
                AdrNameAggregateForkEdit.AddMember => AddMemberCore(
                    adrAggregate, edge, insertIndex, allowSteal: true, out stolenFrom),
                AdrNameAggregateForkEdit.RemoveMember => RemoveMember(adrAggregate, edge),
                _ => false
            };

            if (!applied || !IsAdrNameAggregate(adrAggregate))
            {
                if (IsAdrNameAggregate(adrAggregate))
                {
                    DestroyAggregate(adrAggregate);
                }
                adrAggregate = Entity.Null;
                return false;
            }

            if (edit == AdrNameAggregateForkEdit.AddMember
                || (edit == AdrNameAggregateForkEdit.RemoveMember && IsAdrNameAggregate(adrAggregate)))
            {
                ResortMembers(adrAggregate);
                EnsureNameDirty(adrAggregate);
            }

            if (stolenFrom != Entity.Null && IsAdrNameAggregate(stolenFrom))
            {
                ResortMembers(stolenFrom);
                EnsureNameDirty(stolenFrom);
            }

            return true;
        }

        /// <summary>
        /// Materialize an ADR name-aggregate from the vanilla Aggregate's remainder
        /// (edges not already linked to an ADR). No membership edit.
        /// Used on Road Grouping segment select and as the base for edit forks.
        /// </summary>
        public bool TryForkFromVanillaSeed(Entity vanillaAggregate, out Entity adrAggregate)
        {
            adrAggregate = Entity.Null;

            if (vanillaAggregate == Entity.Null
                || !EntityManager.Exists(vanillaAggregate)
                || !EntityManager.HasComponent<Aggregate>(vanillaAggregate)
                || IsAdrNameAggregate(vanillaAggregate)
                || !EntityManager.TryGetBuffer(vanillaAggregate, true, out DynamicBuffer<AggregateElement> vanillaElements)
                || vanillaElements.Length == 0)
            {
                return false;
            }

            // Snapshot before structural changes — CreateEntity / EnsureNameDirty invalidate buffer handles.
            var edgesToSeed = new List<Entity>(vanillaElements.Length);
            for (int i = 0; i < vanillaElements.Length; i++)
            {
                Entity memberEdge = vanillaElements[i].m_Edge;
                if (memberEdge == Entity.Null || !EntityManager.Exists(memberEdge))
                {
                    continue;
                }
                if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, memberEdge, out _))
                {
                    continue;
                }
                edgesToSeed.Add(memberEdge);
            }

            if (edgesToSeed.Count == 0)
            {
                return false;
            }

            if (!EntityManager.TryGetComponent(vanillaAggregate, out ADRRandomizationData vanillaSeed))
            {
                vanillaSeed = new ADRRandomizationData();
                vanillaSeed.Redraw();
                EntityManager.AddComponentData(vanillaAggregate, vanillaSeed);
            }

            adrAggregate = CreateAggregate();
            EnsureLabelOwnerComponents(adrAggregate, vanillaAggregate);
            var adrSeed = new ADRRandomizationData();
            adrSeed.CopySeedFrom(vanillaSeed);
            EntityManager.AddComponentData(adrAggregate, adrSeed);

            vanillaSeed.Redraw();
            EntityManager.SetComponentData(vanillaAggregate, vanillaSeed);
            EnsureNameDirty(vanillaAggregate);
            EnsureNameDirty(adrAggregate);

            for (int i = 0; i < edgesToSeed.Count; i++)
            {
                AddMemberCore(adrAggregate, edgesToSeed[i], insertIndex: -1, allowSteal: false, out _);
            }

            if (!EntityManager.TryGetBuffer(adrAggregate, true, out DynamicBuffer<ADRNameAggregateElement> seeded)
                || seeded.Length == 0)
            {
                DestroyAggregate(adrAggregate);
                adrAggregate = Entity.Null;
                return false;
            }

            // NameSystem.SetCustomName needs EndFrameBarrier ECB — not allowed during ToolUpdate.
            Entity vanillaForName = vanillaAggregate;
            Entity adrForName = adrAggregate;
            m_runOnUpdate.Enqueue(() =>
            {
                if (!EntityManager.Exists(vanillaForName) || !EntityManager.Exists(adrForName))
                {
                    return;
                }
                TransferCustomNameToAdr(vanillaForName, adrForName);
            });

            if (seeded.Length > 1)
            {
                ResortMembers(adrAggregate);
            }

            return true;
        }

        /// <summary>
        /// Resolve the ADR name-aggregate for a road edge: existing link, or materialize from the
        /// edge's vanilla Aggregate remainder (ensuring <paramref name="edge"/> is a member).
        /// Never returns a vanilla Aggregate entity.
        /// </summary>
        public bool TryEnsureAdrForEdge(Entity edge, out Entity adrAggregate)
        {
            adrAggregate = Entity.Null;
            if (edge == Entity.Null || !EntityManager.Exists(edge))
            {
                return false;
            }

            if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, edge, out adrAggregate))
            {
                return true;
            }

            if (!EntityManager.TryGetComponent(edge, out Aggregated aggregated)
                || aggregated.m_Aggregate == Entity.Null
                || !EntityManager.Exists(aggregated.m_Aggregate)
                || !EntityManager.HasComponent<Aggregate>(aggregated.m_Aggregate))
            {
                return false;
            }

            Entity vanilla = aggregated.m_Aggregate;
            if (!TryForkFromVanillaSeed(vanilla, out adrAggregate))
            {
                return false;
            }

            if (!TryGetMemberIndex(adrAggregate, edge, out _))
            {
                AddMember(adrAggregate, edge);
            }

            return IsAdrNameAggregate(adrAggregate);
        }

        public void DestroyAggregate(Entity aggregate)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return;
            }

            if (EntityManager.TryGetBuffer(aggregate, false, out DynamicBuffer<ADRNameAggregateElement> buffer))
            {
                for (int i = buffer.Length - 1; i >= 0; i--)
                {
                    ClearEdgeLink(buffer[i].m_Edge, aggregate);
                }
                buffer.Clear();
            }

            EntityManager.DestroyEntity(aggregate);
        }

        public bool AddMember(Entity aggregate, Entity edge, int insertIndex = -1)
        {
            if (!AddMemberCore(aggregate, edge, insertIndex, allowSteal: true, out Entity stolenFrom))
            {
                return false;
            }

            ResortMembers(aggregate);
            EnsureNameDirty(aggregate);

            if (stolenFrom != Entity.Null && IsAdrNameAggregate(stolenFrom))
            {
                ResortMembers(stolenFrom);
                EnsureNameDirty(stolenFrom);
            }

            return true;
        }

        private bool AddMemberCore(
            Entity aggregate,
            Entity edge,
            int insertIndex,
            bool allowSteal,
            out Entity stolenFrom)
        {
            stolenFrom = Entity.Null;

            if (!IsAdrNameAggregate(aggregate) || edge == Entity.Null || !EntityManager.Exists(edge))
            {
                return false;
            }

            if (EntityManager.TryGetComponent(edge, out ADRNameAggregated existing))
            {
                if (existing.m_Aggregate == aggregate)
                {
                    return true;
                }

                if (IsAdrNameAggregate(existing.m_Aggregate))
                {
                    if (!allowSteal)
                    {
                        return false;
                    }

                    stolenFrom = existing.m_Aggregate;
                    if (!RemoveMember(existing.m_Aggregate, edge, resort: false))
                    {
                        // Link pointed at an ADR but buffer lacked the edge — force-clear and continue.
                        if (EntityManager.HasComponent<ADRNameAggregated>(edge))
                        {
                            EntityManager.RemoveComponent<ADRNameAggregated>(edge);
                        }
                    }

                    if (!IsAdrNameAggregate(aggregate))
                    {
                        stolenFrom = Entity.Null;
                        return false;
                    }
                }
                else
                {
                    EntityManager.RemoveComponent<ADRNameAggregated>(edge);
                }
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            int existingIndex = IndexOfEdge(buffer, edge);
            if (existingIndex >= 0)
            {
                ApplyEdgeLink(edge, aggregate, buffer[existingIndex]);
                return true;
            }

            var element = new ADRNameAggregateElement(edge);
            if (insertIndex < 0 || insertIndex >= buffer.Length)
            {
                buffer.Add(element);
            }
            else
            {
                buffer.Insert(insertIndex, element);
            }
            ApplyEdgeLink(edge, aggregate, element);
            return true;
        }

        public bool RemoveMember(Entity aggregate, Entity edge)
            => RemoveMember(aggregate, edge, resort: true);

        private bool RemoveMember(Entity aggregate, Entity edge, bool resort)
        {
            if (!IsAdrNameAggregate(aggregate) || edge == Entity.Null)
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            int index = IndexOfEdge(buffer, edge);
            if (index < 0)
            {
                return false;
            }

            buffer.RemoveAt(index);
            ClearEdgeLink(edge, aggregate);
            if (buffer.Length == 0)
            {
                DestroyAggregate(aggregate);
                return true;
            }

            if (resort)
            {
                ResortMembers(aggregate);
                EnsureNameDirty(aggregate);
            }

            return true;
        }

        public bool RemoveMemberAt(Entity aggregate, int index)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (index < 0 || index >= buffer.Length)
            {
                return false;
            }

            var edge = buffer[index].m_Edge;
            buffer.RemoveAt(index);
            ClearEdgeLink(edge, aggregate);
            if (buffer.Length == 0)
            {
                DestroyAggregate(aggregate);
                return true;
            }

            ResortMembers(aggregate);
            EnsureNameDirty(aggregate);
            return true;
        }

        public bool MoveMember(Entity aggregate, int fromIndex, int toIndex)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (fromIndex < 0 || fromIndex >= buffer.Length || toIndex < 0 || toIndex >= buffer.Length || fromIndex == toIndex)
            {
                return fromIndex == toIndex && fromIndex >= 0 && fromIndex < buffer.Length;
            }

            var element = buffer[fromIndex];
            buffer.RemoveAt(fromIndex);
            if (toIndex > fromIndex)
            {
                toIndex--;
            }
            if (toIndex >= buffer.Length)
            {
                buffer.Add(element);
            }
            else
            {
                buffer.Insert(toIndex, element);
            }
            return true;
        }

        public bool ReorderMembers(Entity aggregate, Entity[] orderedEdges)
        {
            if (!IsAdrNameAggregate(aggregate) || orderedEdges == null)
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (buffer.Length != orderedEdges.Length)
            {
                return false;
            }

            var rebuilt = new ADRNameAggregateElement[orderedEdges.Length];
            for (int i = 0; i < orderedEdges.Length; i++)
            {
                int src = IndexOfEdge(buffer, orderedEdges[i]);
                if (src < 0)
                {
                    return false;
                }
                rebuilt[i] = buffer[src];
            }

            buffer.Clear();
            for (int i = 0; i < rebuilt.Length; i++)
            {
                buffer.Add(rebuilt[i]);
                ApplyEdgeLink(rebuilt[i].m_Edge, aggregate, rebuilt[i]);
            }
            return true;
        }

        public bool SetMemberFlags(Entity aggregate, Entity edge, bool preventPropagation, bool numberOverrideActive, int numberOverride)
        {
            if (!IsAdrNameAggregate(aggregate) || edge == Entity.Null)
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            int index = IndexOfEdge(buffer, edge);
            if (index < 0)
            {
                return false;
            }

            var element = buffer[index];
            element.preventPropagation = preventPropagation;
            element.numberOverrideActive = numberOverrideActive;
            element.numberOverride = numberOverride;
            buffer[index] = element;
            ApplyEdgeLink(edge, aggregate, element);
            return true;
        }

        public bool SetMemberFlagsAt(Entity aggregate, int index, bool preventPropagation, bool numberOverrideActive, int numberOverride)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (index < 0 || index >= buffer.Length)
            {
                return false;
            }

            var element = buffer[index];
            element.preventPropagation = preventPropagation;
            element.numberOverrideActive = numberOverrideActive;
            element.numberOverride = numberOverride;
            buffer[index] = element;
            ApplyEdgeLink(element.m_Edge, aggregate, element);
            return true;
        }

        public bool TryGetMemberIndex(Entity aggregate, Entity edge, out int index)
        {
            index = -1;
            if (!IsAdrNameAggregate(aggregate) || edge == Entity.Null)
            {
                return false;
            }
            index = IndexOfEdge(EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate, true), edge);
            return index >= 0;
        }

        private void ApplyEdgeLink(Entity edge, Entity aggregate, ADRNameAggregateElement element)
        {
            var link = new ADRNameAggregated
            {
                m_Aggregate = aggregate,
                preventPropagation = element.preventPropagation,
                numberOverrideActive = element.numberOverrideActive,
                numberOverride = element.numberOverride
            };
            if (EntityManager.HasComponent<ADRNameAggregated>(edge))
            {
                EntityManager.SetComponentData(edge, link);
            }
            else
            {
                EntityManager.AddComponentData(edge, link);
            }
        }

        private void ClearEdgeLink(Entity edge, Entity aggregate)
        {
            if (edge == Entity.Null || !EntityManager.Exists(edge))
            {
                return;
            }
            if (!EntityManager.TryGetComponent(edge, out ADRNameAggregated link))
            {
                return;
            }
            if (link.m_Aggregate != aggregate)
            {
                return;
            }
            EntityManager.RemoveComponent<ADRNameAggregated>(edge);
        }

        private void TransferCustomNameToAdr(Entity vanillaAggregate, Entity adrAggregate)
        {
            if (!m_nameSystem.TryGetCustomName(vanillaAggregate, out string customName)
                || string.IsNullOrWhiteSpace(customName))
            {
                return;
            }

            // ADR uses the same vanilla CustomName component via NameSystem.
            m_nameSystem.SetCustomName(adrAggregate, customName);
            // Clear original so leftover vanilla edges use generated S2 (CustomName removed).
            m_nameSystem.SetCustomName(vanillaAggregate, string.Empty);
        }

        /// <summary>
        /// Rebuild buffer order: connected islands as straight as possible; islands sorted by
        /// min member Entity.Index; each island oriented so the min-Index member lies in the first half.
        /// At multi-edge nodes (crossings), the next edge is the unused candidate with nearest prior buffer order.
        /// </summary>
        public void ResortMembers(Entity aggregate)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (buffer.Length <= 1)
            {
                return;
            }

            var byEdge = new Dictionary<Entity, ADRNameAggregateElement>(buffer.Length);
            var originalOrder = new Dictionary<Entity, int>(buffer.Length);
            var edges = new List<Entity>(buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                var element = buffer[i];
                if (element.m_Edge == Entity.Null || !EntityManager.Exists(element.m_Edge))
                {
                    continue;
                }
                if (byEdge.ContainsKey(element.m_Edge))
                {
                    continue;
                }
                byEdge[element.m_Edge] = element;
                originalOrder[element.m_Edge] = i;
                edges.Add(element.m_Edge);
            }

            if (edges.Count <= 1)
            {
                return;
            }

            var nodeToEdges = new Dictionary<Entity, List<Entity>>();
            for (int i = 0; i < edges.Count; i++)
            {
                if (!EntityManager.TryGetComponent(edges[i], out Edge netEdge))
                {
                    continue;
                }
                AddIncident(nodeToEdges, netEdge.m_Start, edges[i]);
                AddIncident(nodeToEdges, netEdge.m_End, edges[i]);
            }

            var neighbors = new Dictionary<Entity, List<Entity>>(edges.Count);
            for (int i = 0; i < edges.Count; i++)
            {
                neighbors[edges[i]] = new List<Entity>(2);
            }

            foreach (var pair in nodeToEdges)
            {
                var atNode = pair.Value;
                for (int i = 0; i < atNode.Count; i++)
                {
                    for (int j = i + 1; j < atNode.Count; j++)
                    {
                        AddUnique(neighbors[atNode[i]], atNode[j]);
                        AddUnique(neighbors[atNode[j]], atNode[i]);
                    }
                }
            }

            var remaining = new HashSet<Entity>(edges);
            var islands = new List<List<Entity>>();
            while (remaining.Count > 0)
            {
                Entity seed = Entity.Null;
                foreach (var e in remaining)
                {
                    seed = e;
                    break;
                }

                var component = new List<Entity>();
                var queue = new Queue<Entity>();
                queue.Enqueue(seed);
                remaining.Remove(seed);
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    component.Add(cur);
                    if (!neighbors.TryGetValue(cur, out var nexts))
                    {
                        continue;
                    }
                    for (int i = 0; i < nexts.Count; i++)
                    {
                        if (remaining.Remove(nexts[i]))
                        {
                            queue.Enqueue(nexts[i]);
                        }
                    }
                }

                islands.Add(OrderIslandStraight(component, neighbors, nodeToEdges, originalOrder));
            }

            islands.Sort((a, b) => MinEdgeIndex(a).CompareTo(MinEdgeIndex(b)));

            buffer.Clear();
            for (int i = 0; i < islands.Count; i++)
            {
                var island = islands[i];
                for (int j = 0; j < island.Count; j++)
                {
                    var edge = island[j];
                    var element = byEdge[edge];
                    buffer.Add(element);
                    ApplyEdgeLink(edge, aggregate, element);
                }
            }
        }

        private List<Entity> OrderIslandStraight(
            List<Entity> component,
            Dictionary<Entity, List<Entity>> neighbors,
            Dictionary<Entity, List<Entity>> nodeToEdges,
            Dictionary<Entity, int> originalOrder)
        {
            if (component.Count <= 1)
            {
                return component;
            }

            var islandSet = new HashSet<Entity>(component);
            var ordered = new List<Entity>(component.Count);
            var used = new HashSet<Entity>();

            while (used.Count < component.Count)
            {
                Entity startEdge = PickIslandStartEdge(component, used, nodeToEdges, islandSet, originalOrder);
                if (startEdge == Entity.Null)
                {
                    break;
                }

                Entity cursorNode = PickWalkStartNode(startEdge, nodeToEdges, islandSet, used);
                Entity current = startEdge;
                while (current != Entity.Null)
                {
                    ordered.Add(current);
                    used.Add(current);

                    if (!EntityManager.TryGetComponent(current, out Edge netEdge))
                    {
                        break;
                    }

                    Entity nextNode = netEdge.m_Start == cursorNode ? netEdge.m_End : netEdge.m_Start;
                    Entity nextEdge = PickNextEdgeAtNode(current, nextNode, nodeToEdges, islandSet, used, originalOrder);
                    if (nextEdge == Entity.Null)
                    {
                        break;
                    }

                    cursorNode = nextNode;
                    current = nextEdge;
                }
            }

            OrientIslandByMinIndexHalf(ordered);
            return ordered;
        }

        private Entity PickIslandStartEdge(
            List<Entity> component,
            HashSet<Entity> used,
            Dictionary<Entity, List<Entity>> nodeToEdges,
            HashSet<Entity> islandSet,
            Dictionary<Entity, int> originalOrder)
        {
            Entity bestEndpoint = Entity.Null;
            int bestEndpointOrder = int.MaxValue;
            Entity bestAny = Entity.Null;
            int bestAnyOrder = int.MaxValue;

            for (int i = 0; i < component.Count; i++)
            {
                var edge = component[i];
                if (used.Contains(edge))
                {
                    continue;
                }

                int order = GetOriginalOrder(originalOrder, edge);
                if (order < bestAnyOrder)
                {
                    bestAnyOrder = order;
                    bestAny = edge;
                }

                if (!EntityManager.TryGetComponent(edge, out Edge netEdge))
                {
                    continue;
                }

                if (IsIslandEndpointNode(netEdge.m_Start, nodeToEdges, islandSet, used)
                    || IsIslandEndpointNode(netEdge.m_End, nodeToEdges, islandSet, used))
                {
                    if (order < bestEndpointOrder)
                    {
                        bestEndpointOrder = order;
                        bestEndpoint = edge;
                    }
                }
            }

            return bestEndpoint != Entity.Null ? bestEndpoint : bestAny;
        }

        private bool IsIslandEndpointNode(
            Entity node,
            Dictionary<Entity, List<Entity>> nodeToEdges,
            HashSet<Entity> islandSet,
            HashSet<Entity> used)
        {
            if (node == Entity.Null || !nodeToEdges.TryGetValue(node, out var atNode))
            {
                return false;
            }

            int live = 0;
            for (int i = 0; i < atNode.Count; i++)
            {
                var edge = atNode[i];
                if (islandSet.Contains(edge) && !used.Contains(edge))
                {
                    live++;
                }
            }

            return live == 1;
        }

        private Entity PickWalkStartNode(
            Entity startEdge,
            Dictionary<Entity, List<Entity>> nodeToEdges,
            HashSet<Entity> islandSet,
            HashSet<Entity> used)
        {
            if (!EntityManager.TryGetComponent(startEdge, out Edge netEdge))
            {
                return Entity.Null;
            }

            if (IsIslandEndpointNode(netEdge.m_Start, nodeToEdges, islandSet, used))
            {
                return netEdge.m_Start;
            }

            if (IsIslandEndpointNode(netEdge.m_End, nodeToEdges, islandSet, used))
            {
                return netEdge.m_End;
            }

            // Loop / mid-chain seed: start walking from Start node.
            return netEdge.m_Start;
        }

        private Entity PickNextEdgeAtNode(
            Entity currentEdge,
            Entity node,
            Dictionary<Entity, List<Entity>> nodeToEdges,
            HashSet<Entity> islandSet,
            HashSet<Entity> used,
            Dictionary<Entity, int> originalOrder)
        {
            if (node == Entity.Null || !nodeToEdges.TryGetValue(node, out var atNode))
            {
                return Entity.Null;
            }

            int currentOrder = GetOriginalOrder(originalOrder, currentEdge);
            Entity best = Entity.Null;
            int bestDistance = int.MaxValue;
            int bestOrder = int.MaxValue;
            for (int i = 0; i < atNode.Count; i++)
            {
                var cand = atNode[i];
                if (!islandSet.Contains(cand) || used.Contains(cand) || cand == currentEdge)
                {
                    continue;
                }

                int candOrder = GetOriginalOrder(originalOrder, cand);
                int distance = candOrder >= currentOrder ? candOrder - currentOrder : currentOrder - candOrder;
                // Crossing / multi-edge node: prefer nearest original buffer order to the current edge.
                if (distance < bestDistance
                    || (distance == bestDistance && candOrder < bestOrder))
                {
                    bestDistance = distance;
                    bestOrder = candOrder;
                    best = cand;
                }
            }

            return best;
        }

        private static int GetOriginalOrder(Dictionary<Entity, int> originalOrder, Entity edge)
        {
            return originalOrder != null && originalOrder.TryGetValue(edge, out int order)
                ? order
                : edge.Index;
        }

        private static void OrientIslandByMinIndexHalf(List<Entity> ordered)
        {
            if (ordered == null || ordered.Count <= 1)
            {
                return;
            }

            int minIndex = ordered[0].Index;
            int minPos = 0;
            for (int i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].Index < minIndex)
                {
                    minIndex = ordered[i].Index;
                    minPos = i;
                }
            }

            // Keep orientation when min Index is in the first half; reverse otherwise.
            if (minPos >= ordered.Count / 2)
            {
                ordered.Reverse();
            }
        }

        private static int MinEdgeIndex(List<Entity> edges)
        {
            int min = int.MaxValue;
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Index < min)
                {
                    min = edges[i].Index;
                }
            }
            return min;
        }

        private static void AddIncident(Dictionary<Entity, List<Entity>> nodeToEdges, Entity node, Entity edge)
        {
            if (node == Entity.Null)
            {
                return;
            }
            if (!nodeToEdges.TryGetValue(node, out var list))
            {
                list = new List<Entity>(2);
                nodeToEdges[node] = list;
            }
            AddUnique(list, edge);
        }

        private static void AddUnique(List<Entity> list, Entity edge)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == edge)
                {
                    return;
                }
            }
            list.Add(edge);
        }

        private void EnsureNameDirty(Entity entity)
        {
            if (!EntityManager.HasComponent<BatchesUpdated>(entity))
            {
                EntityManager.AddComponent<BatchesUpdated>(entity);
            }
            if (!EntityManager.HasComponent<Updated>(entity))
            {
                EntityManager.AddComponent<Updated>(entity);
            }
        }

        private static int IndexOfEdge(DynamicBuffer<ADRNameAggregateElement> buffer, Entity edge)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].m_Edge == edge)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
