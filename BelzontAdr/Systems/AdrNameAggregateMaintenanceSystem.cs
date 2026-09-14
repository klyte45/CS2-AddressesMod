using Colossal.Entities;
using Game.Common;
using Game.Net;
using Game.Tools;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Keeps ADR membership coherent with net tools the way vanilla keeps <see cref="Aggregated"/>:
    /// temps/replacements copy membership from <see cref="Temp.m_Original"/> (or clear it when the
    /// original had none); only truly new Created edges may be auto-enrolled (bridging chains /
    /// gap-fill). Preview must not steal membership off live originals into the ADR buffer.
    /// </summary>
    public partial class AdrNameAggregateMaintenanceSystem : SystemBase
    {
        private EntityQuery m_tempEdgesQuery;
        private EntityQuery m_createdTempEdgesQuery;
        private EntityQuery m_createdPermanentEdgesQuery;
        private EntityQuery m_adrAggregatesQuery;
        private AdrNameAggregateMembershipSystem m_membership;
        private AdrRoadGroupingController m_roadGroupingController;

        /// <summary>
        /// Created edges that had a non-null <see cref="Temp.m_Original"/> (replacements/modifies).
        /// Survives Temp strip for the rest of the Created lifetime so they are not treated as new.
        /// </summary>
        private readonly HashSet<Entity> m_knownReplacements = new HashSet<Entity>();

        protected override void OnCreate()
        {
            base.OnCreate();
            m_membership = World.GetOrCreateSystemManaged<AdrNameAggregateMembershipSystem>();
            m_roadGroupingController = World.GetOrCreateSystemManaged<AdrRoadGroupingController>();

            m_tempEdgesQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Edge>(),
                    ComponentType.ReadOnly<Temp>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>()
                }
            });

            m_createdTempEdgesQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Edge>(),
                    ComponentType.ReadOnly<Created>(),
                    ComponentType.ReadOnly<Temp>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>()
                }
            });

            m_createdPermanentEdgesQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Edge>(),
                    ComponentType.ReadOnly<Created>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                }
            });

            m_adrAggregatesQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ADRNameAggregate>(),
                    ComponentType.ReadOnly<ADRNameAggregateElement>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<Deleted>()
                }
            });
        }

        protected override void OnUpdate()
        {
            // Replacement/modify temps: copy ADR from original, or strip if original had none.
            SyncTempEdgesFromOriginal();
            PruneKnownReplacements();

            if (m_adrAggregatesQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var oldToNew = BuildAppliedOldToNewMap();
            bool hadAppliedRemap = oldToNew.Count > 0;
            bool hasCreatedPermanent = !m_createdPermanentEdgesQuery.IsEmptyIgnoreFilter;
            bool netTopologyChanged = hadAppliedRemap || hasCreatedPermanent;

            HashSet<Entity> absorbedChains = hasCreatedPermanent
                ? AbsorbCreatedBridgingChains()
                : null;

            using var aggregates = m_adrAggregatesQuery.ToEntityArray(Allocator.Temp);
            bool anyChanged = false;

            for (int a = 0; a < aggregates.Length; a++)
            {
                Entity aggregate = aggregates[a];
                if (!m_membership.IsAdrNameAggregate(aggregate))
                {
                    continue;
                }

                // Backfill highway component on ADR aggregates from saves predating HW-01.
                m_membership.EnsureHighwayData(aggregate);
                // Backfill stored AdrNetKind from first member when None (pre-NET-01 saves).
                m_membership.EnsureNetKind(aggregate);

                bool touched = RemapAndDropDeadMembers(aggregate, oldToNew, out bool changed);
                if (!m_membership.IsAdrNameAggregate(aggregate))
                {
                    anyChanged = true;
                    continue;
                }

                if (absorbedChains != null && absorbedChains.Contains(aggregate))
                {
                    changed = true;
                }

                if ((netTopologyChanged || touched) && GapFillNewEdgesOnly(aggregate))
                {
                    changed = true;
                }

                if (changed && m_membership.IsAdrNameAggregate(aggregate))
                {
                    m_membership.ResortMembers(aggregate);
                    MarkDirty(aggregate);
                    anyChanged = true;
                }
            }

            if (anyChanged)
            {
                m_roadGroupingController.InvalidateSegmentCards();
            }
        }

        /// <summary>
        /// For every temp edge with an original: if the original is in an ADR, copy the link;
        /// if not, ensure the temp/replacement is not in any ADR. Marks replacements so they
        /// are never treated as "really new" after Temp is stripped.
        /// </summary>
        private void SyncTempEdgesFromOriginal()
        {
            if (m_tempEdgesQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            using var temps = m_tempEdgesQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < temps.Length; i++)
            {
                Entity tempEdge = temps[i];
                if (!EntityManager.TryGetComponent(tempEdge, out Temp temp)
                    || temp.m_Original == Entity.Null
                    || temp.m_Original == tempEdge
                    || !EntityManager.Exists(temp.m_Original))
                {
                    continue;
                }

                m_knownReplacements.Add(tempEdge);

                Entity original = temp.m_Original;
                if (EntityManager.TryGetComponent(original, out ADRNameAggregated sourceLink)
                    && sourceLink.m_Aggregate != Entity.Null
                    && m_membership.IsAdrNameAggregate(sourceLink.m_Aggregate))
                {
                    if (EntityManager.TryGetComponent(tempEdge, out ADRNameAggregated existing)
                        && existing.m_Aggregate == sourceLink.m_Aggregate
                        && existing.preventPropagation == sourceLink.preventPropagation
                        && existing.numberOverrideActive == sourceLink.numberOverrideActive
                        && existing.numberOverride == sourceLink.numberOverride)
                    {
                        continue;
                    }

                    if (EntityManager.HasComponent<ADRNameAggregated>(tempEdge))
                    {
                        EntityManager.SetComponentData(tempEdge, sourceLink);
                    }
                    else
                    {
                        EntityManager.AddComponentData(tempEdge, sourceLink);
                    }
                }
                else
                {
                    ClearAdrMembershipFromEdge(tempEdge);
                }
            }
        }

        private void PruneKnownReplacements()
        {
            if (m_knownReplacements.Count == 0)
            {
                return;
            }

            m_knownReplacements.RemoveWhere(edge =>
                edge == Entity.Null
                || !EntityManager.Exists(edge)
                || EntityManager.HasComponent<Deleted>(edge)
                || !EntityManager.HasComponent<Created>(edge));
        }

        private bool IsReallyNewEdge(Entity edge)
        {
            if (edge == Entity.Null
                || !EntityManager.Exists(edge)
                || EntityManager.HasComponent<Deleted>(edge)
                || !EntityManager.HasComponent<Created>(edge)
                || m_knownReplacements.Contains(edge))
            {
                return false;
            }

            if (EntityManager.TryGetComponent(edge, out Temp temp))
            {
                return temp.m_Original == Entity.Null;
            }

            // Permanent Created and never seen as a Temp-with-original replacement.
            return true;
        }

        private void ClearAdrMembershipFromEdge(Entity edge)
        {
            if (!EntityManager.TryGetComponent(edge, out ADRNameAggregated link)
                || link.m_Aggregate == Entity.Null)
            {
                if (EntityManager.HasComponent<ADRNameAggregated>(edge))
                {
                    EntityManager.RemoveComponent<ADRNameAggregated>(edge);
                }
                return;
            }

            if (m_membership.IsAdrNameAggregate(link.m_Aggregate))
            {
                m_membership.RemoveMember(link.m_Aggregate, edge);
            }

            if (EntityManager.HasComponent<ADRNameAggregated>(edge))
            {
                EntityManager.RemoveComponent<ADRNameAggregated>(edge);
            }
        }

        /// <summary>
        /// Enroll a whole Created chain that bridges two propagating members of the same ADR.
        /// Candidates must be really new (not replacements of an original).
        /// </summary>
        private HashSet<Entity> AbsorbCreatedBridgingChains()
        {
            var dirtyAggregates = new HashSet<Entity>();
            using var created = m_createdPermanentEdgesQuery.ToEntityArray(Allocator.Temp);

            var candidates = new List<Entity>(created.Length);
            var candidateSet = new HashSet<Entity>();
            for (int i = 0; i < created.Length; i++)
            {
                Entity edge = created[i];
                if (!IsReallyNewEdge(edge)
                    || !EntityManager.HasComponent<Edge>(edge)
                    || AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, edge, out _))
                {
                    continue;
                }

                if (candidateSet.Add(edge))
                {
                    candidates.Add(edge);
                }
            }

            if (candidates.Count == 0)
            {
                return dirtyAggregates;
            }

            var nodeToCandidates = new Dictionary<Entity, List<Entity>>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!EntityManager.TryGetComponent(candidates[i], out Edge netEdge))
                {
                    continue;
                }

                AddIncident(nodeToCandidates, netEdge.m_Start, candidates[i]);
                AddIncident(nodeToCandidates, netEdge.m_End, candidates[i]);
            }

            var neighbors = new Dictionary<Entity, List<Entity>>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                neighbors[candidates[i]] = new List<Entity>(2);
            }

            foreach (var pair in nodeToCandidates)
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

            var remaining = new HashSet<Entity>(candidates);
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
                    Entity cur = queue.Dequeue();
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

                if (TryAbsorbCreatedComponent(component, candidateSet, nodeToCandidates, out Entity adr))
                {
                    dirtyAggregates.Add(adr);
                }
            }

            return dirtyAggregates;
        }

        private bool TryAbsorbCreatedComponent(
            List<Entity> component,
            HashSet<Entity> candidateSet,
            Dictionary<Entity, List<Entity>> nodeToCandidates,
            out Entity adrAggregate)
        {
            adrAggregate = Entity.Null;

            var componentSet = new HashSet<Entity>(component);
            var endNodes = new List<Entity>();
            foreach (var pair in nodeToCandidates)
            {
                Entity node = pair.Key;
                int degree = 0;
                var atNode = pair.Value;
                for (int i = 0; i < atNode.Count; i++)
                {
                    if (componentSet.Contains(atNode[i]))
                    {
                        degree++;
                    }
                }

                if (degree == 1)
                {
                    endNodes.Add(node);
                }
            }

            if (endNodes.Count < 2)
            {
                return false;
            }

            var adrEndNodes = new Dictionary<Entity, HashSet<Entity>>();
            for (int n = 0; n < endNodes.Count; n++)
            {
                Entity endNode = endNodes[n];
                if (!EntityManager.TryGetBuffer(endNode, true, out DynamicBuffer<ConnectedEdge> connected))
                {
                    continue;
                }

                for (int c = 0; c < connected.Length; c++)
                {
                    Entity other = connected[c].m_Edge;
                    if (other == Entity.Null
                        || candidateSet.Contains(other)
                        || !EntityManager.TryGetComponent(other, out ADRNameAggregated link)
                        || link.m_Aggregate == Entity.Null
                        || link.preventPropagation
                        || !m_membership.IsAdrNameAggregate(link.m_Aggregate))
                    {
                        continue;
                    }

                    if (!adrEndNodes.TryGetValue(link.m_Aggregate, out var ends))
                    {
                        ends = new HashSet<Entity>();
                        adrEndNodes[link.m_Aggregate] = ends;
                    }

                    ends.Add(endNode);
                }
            }

            Entity chosen = Entity.Null;
            foreach (var pair in adrEndNodes)
            {
                if (pair.Value.Count >= 2)
                {
                    chosen = pair.Key;
                    break;
                }
            }

            if (chosen == Entity.Null)
            {
                return false;
            }

            bool addedAny = false;
            for (int i = 0; i < component.Count; i++)
            {
                if (m_membership.AddMember(chosen, component[i]))
                {
                    addedAny = true;
                }
            }

            if (!addedAny)
            {
                return false;
            }

            adrAggregate = chosen;
            return true;
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

            if (!list.Contains(edge))
            {
                list.Add(edge);
            }
        }

        private static void AddUnique(List<Entity> list, Entity value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }

        private Dictionary<Entity, List<Entity>> BuildAppliedOldToNewMap()
        {
            var map = new Dictionary<Entity, List<Entity>>();
            if (m_createdTempEdgesQuery.IsEmptyIgnoreFilter)
            {
                return map;
            }

            using var created = m_createdTempEdgesQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < created.Length; i++)
            {
                Entity neu = created[i];
                if (!EntityManager.TryGetComponent(neu, out Temp temp)
                    || temp.m_Original == Entity.Null
                    || temp.m_Original == neu
                    || !EntityManager.Exists(temp.m_Original)
                    || !EntityManager.HasComponent<Deleted>(temp.m_Original))
                {
                    continue;
                }

                m_knownReplacements.Add(neu);

                if (!map.TryGetValue(temp.m_Original, out var list))
                {
                    list = new List<Entity>(2);
                    map[temp.m_Original] = list;
                }

                if (!list.Contains(neu))
                {
                    list.Add(neu);
                }
            }

            return map;
        }

        private bool RemapAndDropDeadMembers(
            Entity aggregate,
            Dictionary<Entity, List<Entity>> oldToNew,
            out bool changed)
        {
            changed = false;
            bool droppedDead = false;
            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            if (buffer.Length == 0)
            {
                m_membership.DestroyAggregate(aggregate);
                changed = true;
                return true;
            }

            var snapshot = new List<ADRNameAggregateElement>(buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                snapshot.Add(buffer[i]);
            }

            var rebuilt = new List<ADRNameAggregateElement>(snapshot.Count);
            var seen = new HashSet<Entity>();

            for (int i = 0; i < snapshot.Count; i++)
            {
                var element = snapshot[i];
                Entity oldEdge = element.m_Edge;
                if (oldEdge == Entity.Null)
                {
                    droppedDead = true;
                    changed = true;
                    continue;
                }

                if (oldToNew.TryGetValue(oldEdge, out var successors) && successors.Count > 0)
                {
                    int mappedCount = 0;
                    for (int s = 0; s < successors.Count; s++)
                    {
                        Entity neu = successors[s];
                        if (neu == Entity.Null
                            || !EntityManager.Exists(neu)
                            || EntityManager.HasComponent<Deleted>(neu)
                            || !seen.Add(neu))
                        {
                            continue;
                        }

                        var mapped = element;
                        mapped.m_Edge = neu;
                        rebuilt.Add(mapped);
                        ApplyLink(neu, aggregate, mapped);
                        mappedCount++;
                        changed = true;
                    }

                    if (mappedCount > 0)
                    {
                        if (EntityManager.Exists(oldEdge)
                            && EntityManager.TryGetComponent(oldEdge, out ADRNameAggregated oldLink)
                            && oldLink.m_Aggregate == aggregate)
                        {
                            EntityManager.RemoveComponent<ADRNameAggregated>(oldEdge);
                        }
                        continue;
                    }
                }

                bool dead = !EntityManager.Exists(oldEdge) || EntityManager.HasComponent<Deleted>(oldEdge);
                if (dead)
                {
                    if (EntityManager.Exists(oldEdge)
                        && EntityManager.TryGetComponent(oldEdge, out ADRNameAggregated deadLink)
                        && deadLink.m_Aggregate == aggregate)
                    {
                        EntityManager.RemoveComponent<ADRNameAggregated>(oldEdge);
                    }
                    droppedDead = true;
                    changed = true;
                    continue;
                }

                if (!seen.Add(oldEdge))
                {
                    continue;
                }

                rebuilt.Add(element);
                ApplyLink(oldEdge, aggregate, element);
            }

            if (!changed && rebuilt.Count == snapshot.Count)
            {
                return false;
            }

            buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
            buffer.Clear();
            for (int i = 0; i < rebuilt.Count; i++)
            {
                buffer.Add(rebuilt[i]);
            }

            if (buffer.Length == 0)
            {
                m_membership.DestroyAggregate(aggregate);
                return true;
            }

            return droppedDead;
        }

        /// <summary>
        /// Gap-fill only really new Created edges between propagating members — never re-add
        /// long-lived excluded segments.
        /// </summary>
        private bool GapFillNewEdgesOnly(Entity aggregate)
        {
            if (!m_membership.IsAdrNameAggregate(aggregate))
            {
                return false;
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate, true);
            if (buffer.Length < 2)
            {
                return false;
            }

            var membership = new HashSet<Entity>();
            var flagsByEdge = new Dictionary<Entity, ADRNameAggregateElement>();
            var vanillaAggregates = new HashSet<Entity>();

            for (int i = 0; i < buffer.Length; i++)
            {
                Entity edge = buffer[i].m_Edge;
                if (edge == Entity.Null)
                {
                    continue;
                }
                membership.Add(edge);
                flagsByEdge[edge] = buffer[i];
                if (EntityManager.TryGetComponent(edge, out Aggregated aggregated)
                    && aggregated.m_Aggregate != Entity.Null)
                {
                    vanillaAggregates.Add(aggregated.m_Aggregate);
                }
            }

            bool added = false;
            foreach (Entity vanilla in vanillaAggregates)
            {
                if (!EntityManager.TryGetBuffer(vanilla, true, out DynamicBuffer<AggregateElement> vanillaElements)
                    || vanillaElements.Length < 3)
                {
                    continue;
                }

                var memberIndexes = new List<int>();
                for (int i = 0; i < vanillaElements.Length; i++)
                {
                    Entity edge = vanillaElements[i].m_Edge;
                    if (edge != Entity.Null && membership.Contains(edge))
                    {
                        memberIndexes.Add(i);
                    }
                }

                for (int m = 0; m < memberIndexes.Count - 1; m++)
                {
                    int ia = memberIndexes[m];
                    int ib = memberIndexes[m + 1];
                    if (ib <= ia + 1)
                    {
                        continue;
                    }

                    Entity edgeA = vanillaElements[ia].m_Edge;
                    Entity edgeB = vanillaElements[ib].m_Edge;
                    if (!flagsByEdge.TryGetValue(edgeA, out var flagsA)
                        || !flagsByEdge.TryGetValue(edgeB, out var flagsB)
                        || flagsA.preventPropagation
                        || flagsB.preventPropagation)
                    {
                        continue;
                    }

                    for (int g = ia + 1; g < ib; g++)
                    {
                        Entity gapEdge = vanillaElements[g].m_Edge;
                        if (gapEdge == Entity.Null
                            || !EntityManager.Exists(gapEdge)
                            || EntityManager.HasComponent<Deleted>(gapEdge)
                            || EntityManager.HasComponent<Temp>(gapEdge)
                            || membership.Contains(gapEdge)
                            || !IsReallyNewEdge(gapEdge)
                            || AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, gapEdge, out _))
                        {
                            continue;
                        }

                        if (m_membership.AddMember(aggregate, gapEdge))
                        {
                            membership.Add(gapEdge);
                            added = true;
                        }
                    }
                }
            }

            return added;
        }

        private void ApplyLink(Entity edge, Entity aggregate, ADRNameAggregateElement element)
        {
            if (edge == Entity.Null || !EntityManager.Exists(edge))
            {
                return;
            }

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

        private void MarkDirty(Entity entity)
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
    }
}
