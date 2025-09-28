using Game.Common;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem
    {
        [BurstCompile]
        private unsafe struct ADRUpdateVehiclesConvoyId : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ComponentLookup<Owner> m_ownerLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_entityHdl);
                var length = entities.Length;
                for (int i = 0; i < length; i++)
                {
                    Entity entity = entities[i];
                    if (m_ownerLookup.TryGetComponent(entity, out var owner))
                    {
                        m_cmdBuffer.AddComponent<ADRBuildingVehiclesSerialDirty>(unfilteredChunkIndex, owner.m_Owner);
                    }
                    m_cmdBuffer.RemoveComponent<ADRVehicleSerialDataDirty>(unfilteredChunkIndex, entity);
                }

            }
        }
    }


}

