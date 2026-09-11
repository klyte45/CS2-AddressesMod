using Colossal.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrNameAggregateMembershipSystem : SystemBase
    {
        public static AdrNameAggregateMembershipSystem Instance { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnDestroy();
        }

        protected override void OnUpdate() { }

        public bool IsAdrNameAggregate(Entity aggregate)
            => aggregate != Entity.Null
               && EntityManager.Exists(aggregate)
               && EntityManager.HasComponent<ADRNameAggregate>(aggregate);

        public Entity CreateAggregate()
        {
            var aggregate = EntityManager.CreateEntity();
            EntityManager.AddComponent<ADRNameAggregate>(aggregate);
            EntityManager.AddBuffer<ADRNameAggregateElement>(aggregate);
            return aggregate;
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
                AddMember(aggregate, edges[i]);
            }
            return aggregate;
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
                    RemoveMember(existing.m_Aggregate, edge);
                    if (!IsAdrNameAggregate(aggregate))
                    {
                        return false;
                    }
                }
                else
                {
                    EntityManager.RemoveComponent<ADRNameAggregated>(edge);
                }
            }

            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate);
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
            TeardownIfEmpty(aggregate);
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
            TeardownIfEmpty(aggregate);
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

        private void TeardownIfEmpty(Entity aggregate)
        {
            if (!IsAdrNameAggregate(aggregate))
            {
                return;
            }
            var buffer = EntityManager.GetBuffer<ADRNameAggregateElement>(aggregate, true);
            if (buffer.Length == 0)
            {
                DestroyAggregate(aggregate);
            }
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
