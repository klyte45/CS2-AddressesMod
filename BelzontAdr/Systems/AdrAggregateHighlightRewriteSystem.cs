using Colossal.Entities;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-05 H1: Rewrite CreationDefinition AggregateElement before GenerateAggregatesSystem.
    /// S1 Aggregate → remainder without ADRNameAggregated edges.
    /// S2 ADRNameAggregate → ensure AggregateElement filled from ADRNameAggregateElement
    /// (vanilla CreateDefinitionsJob only copies AggregateElement when present on the selected entity).
    /// Re-diff GUID d124bec16c0748da9ebf48366d9c1d68 — DefaultTool CreateDefinitionsJob ~367–372,
    /// GenerateAggregatesSystem CreateAggregates Highlighted loop.
    /// </summary>
    public partial class AdrAggregateHighlightRewriteSystem : SystemBase
    {
        private EntityQuery m_DefinitionQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_DefinitionQuery = GetEntityQuery(
                ComponentType.ReadOnly<CreationDefinition>(),
                ComponentType.ReadOnly<Updated>());
            RequireForUpdate(m_DefinitionQuery);
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = m_DefinitionQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity definition = entities[i];
                CreationDefinition creation = EntityManager.GetComponentData<CreationDefinition>(definition);
                Entity original = creation.m_Original;
                if (original == Entity.Null || !EntityManager.Exists(original))
                {
                    continue;
                }

                if (AdrNameAggregateMembershipHelper.IsS2Owner(EntityManager, original))
                {
                    DynamicBuffer<AggregateElement> dest = EntityManager.HasBuffer<AggregateElement>(definition)
                        ? EntityManager.GetBuffer<AggregateElement>(definition)
                        : EntityManager.AddBuffer<AggregateElement>(definition);
                    AdrNameAggregateMembershipHelper.FillAggregateElementFromAdr(EntityManager, original, dest);

                    if (creation.m_Prefab == Entity.Null
                        && EntityManager.TryGetComponent(original, out PrefabRef prefabRef))
                    {
                        creation.m_Prefab = prefabRef.m_Prefab;
                        EntityManager.SetComponentData(definition, creation);
                    }
                }
                else if (AdrNameAggregateMembershipHelper.IsS1Owner(EntityManager, original)
                         && EntityManager.HasBuffer<AggregateElement>(definition))
                {
                    DynamicBuffer<AggregateElement> buffer = EntityManager.GetBuffer<AggregateElement>(definition);
                    AdrNameAggregateMembershipHelper.FilterAggregateElementToS1Remainder(EntityManager, buffer);
                }
            }
        }
    }
}
