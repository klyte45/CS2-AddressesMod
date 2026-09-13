using Colossal.Entities;
using Game.Net;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-05: Shared S1/S2 edge membership for SIP RoadSection and DefaultTool H1 highlight.
    /// S1 = vanilla Aggregate remainder (skip ADR-associated edges).
    /// S2 = full ADRNameAggregateElement list.
    /// Re-diff baseline Game.dll GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public static class AdrNameAggregateMembershipHelper
    {
        public static bool IsAdrAssociated(EntityManager entityManager, Entity edge)
            => edge != Entity.Null
               && entityManager.Exists(edge)
               && entityManager.HasComponent<ADRNameAggregated>(edge);

        public static bool IsS2Owner(EntityManager entityManager, Entity entity)
            => entity != Entity.Null
               && entityManager.Exists(entity)
               && entityManager.HasComponent<ADRNameAggregate>(entity)
               && entityManager.HasBuffer<ADRNameAggregateElement>(entity);

        public static bool IsS1Owner(EntityManager entityManager, Entity entity)
            => entity != Entity.Null
               && entityManager.Exists(entity)
               && entityManager.HasComponent<Aggregate>(entity)
               && entityManager.HasBuffer<AggregateElement>(entity)
               && !entityManager.HasComponent<ADRNameAggregate>(entity);

        public static bool HasS1Remainder(EntityManager entityManager, Entity aggregate)
        {
            if (!entityManager.TryGetBuffer(aggregate, true, out DynamicBuffer<AggregateElement> elements))
            {
                return false;
            }
            for (int i = 0; i < elements.Length; i++)
            {
                if (!IsAdrAssociated(entityManager, elements[i].m_Edge))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// In-place: keep only edges without <see cref="ADRNameAggregated"/> (S1 highlight / SIP remainder).
        /// </summary>
        public static void FilterAggregateElementToS1Remainder(EntityManager entityManager, DynamicBuffer<AggregateElement> buffer)
        {
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                if (IsAdrAssociated(entityManager, buffer[i].m_Edge))
                {
                    buffer.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Replace <paramref name="dest"/> with AggregateElement entries from ADR membership (S2).
        /// </summary>
        public static void FillAggregateElementFromAdr(
            EntityManager entityManager,
            Entity adrAggregate,
            DynamicBuffer<AggregateElement> dest)
        {
            dest.Clear();
            if (!entityManager.TryGetBuffer(adrAggregate, true, out DynamicBuffer<ADRNameAggregateElement> adrElements))
            {
                return;
            }
            for (int i = 0; i < adrElements.Length; i++)
            {
                Entity edge = adrElements[i].m_Edge;
                if (edge != Entity.Null && entityManager.Exists(edge))
                {
                    dest.Add(new AggregateElement(edge));
                }
            }
        }

        public static bool TryGetFirstAdrEdgePrefab(
            EntityManager entityManager,
            Entity adrAggregate,
            out Entity edgePrefab)
        {
            edgePrefab = Entity.Null;
            if (!entityManager.TryGetBuffer(adrAggregate, true, out DynamicBuffer<ADRNameAggregateElement> adrElements)
                || adrElements.Length == 0)
            {
                return false;
            }
            Entity edge = adrElements[0].m_Edge;
            if (!entityManager.TryGetComponent(edge, out Game.Prefabs.PrefabRef prefabRef))
            {
                return false;
            }
            edgePrefab = prefabRef.m_Prefab;
            return edgePrefab != Entity.Null;
        }
    }
}
