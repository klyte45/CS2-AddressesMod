using Game.Common;
using Game.Net;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Jobs;
using Game;
using Game.Tools;








#if BURST
using Unity.Burst;
#endif
namespace BelzontAdr
{
    public partial class AdrHighwayRoutes2BSystem : GameSystemBase
    {
        private EntityQuery m_ModifiedQuery;
        private ModificationBarrier2B m_ModifiedBarrier2B;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_ModifiedQuery = base.GetEntityQuery(new EntityQueryDesc[]
           {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregated>(),
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Created>(),
                        ComponentType.ReadOnly<Updated>(),
                        ComponentType.ReadOnly<Deleted>()
                    },
                    None =  new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>()
                    },
                }
           });
            RequireForUpdate(m_ModifiedQuery);
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            m_ModifiedBarrier2B = World.GetExistingSystemManaged<ModificationBarrier2B>();
        }

        protected override void OnUpdate()
        {
            var updater = new AggregationCacheEraserFromDeletedAggregated
            {
                m_CommandBuffer = m_ModifiedBarrier2B.CreateCommandBuffer().AsParallelWriter(),
                m_aggregatedType = GetComponentTypeHandle<Aggregated>()
            };
            updater.ScheduleParallel(m_ModifiedQuery, Dependency).Complete();
        }

#if BURST
        [BurstCompile]
#endif
        private struct AggregationCacheEraserFromDeletedAggregated : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public ComponentTypeHandle<Aggregated> m_aggregatedType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var aggs = chunk.GetNativeArray(ref m_aggregatedType);
                for (int i = 0; i < aggs.Length; i++)
                {
                    var agg = aggs[i];
                    m_CommandBuffer.RemoveComponent<ADRHighwayAggregationCacheData>(unfilteredChunkIndex, agg.m_Aggregate);
                    m_CommandBuffer.AddComponent<ADRHighwayAggregationDataDirtyHwId>(unfilteredChunkIndex, agg.m_Aggregate);
                }
            }


        }
    }
}
