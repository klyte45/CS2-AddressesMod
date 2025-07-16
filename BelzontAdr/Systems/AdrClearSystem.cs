using Game;
using Game.Prefabs;
using Unity.Entities;

namespace BelzontAdr
{
    internal partial class AdrClearSystem : GameSystemBase
    {
        private EntityQuery m_ClearQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_ClearQuery = base.GetEntityQuery(new EntityQueryDesc[]
        {
                new EntityQueryDesc
                {
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRRegionCity>()
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<NetCompositionData>(),
                        ComponentType.ReadOnly<PrefabData>()
                    }
                }
        });
        }
        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(m_ClearQuery);
        }
    }
}
