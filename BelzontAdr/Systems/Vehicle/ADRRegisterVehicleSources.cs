using Colossal;
using Game.Buildings;
using Game.Companies;
using Game.Objects;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using static BelzontAdr.ADRVehicleSourceData;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem
    {
#if DEBUG
        [BurstCompile]
        private unsafe struct ADRRegisterVehicleSources : IJobChunk
        {
            internal EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            internal EntityTypeHandle m_entityHdl;
            internal ComponentLookup<PoliceStation> m_policeStation;
            internal ComponentLookup<Hospital> m_hospital;
            internal ComponentLookup<DeathcareFacility> m_deathcareFacility;
            internal ComponentLookup<FireStation> m_fireStation;
            internal ComponentLookup<GarbageFacility> m_garbageFacility;
            internal ComponentLookup<TransportDepot> m_transportDepot;
            internal ComponentLookup<CargoTransportStation> m_cargoTransportStation;
            internal ComponentLookup<MaintenanceDepot> m_maintenanceDepot;
            internal ComponentLookup<PostFacility> m_postFacility;
            internal ComponentLookup<TransportCompany> m_transportCompany;
            internal ComponentLookup<IndustrialCompany> m_industrialCompany;
            internal ComponentLookup<CommercialCompany> m_commercialCompany;
            internal uint refSerialNumber;
            internal NativeCounter.Concurrent m_serialNumberCounter;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_entityHdl);
                var length = entities.Length;
                //   Debug.Log($"Chunk #{unfilteredChunkIndex} size: {length}");
                for (int i = 0; i < length; i++)
                {
                    Entity entity = entities[i];
                    var newCmp = new ADRVehicleSourceData
                    {
                        serialNumber = refSerialNumber + (uint)m_serialNumberCounter.Increment(),
                        internalSerialCounter = 0,
                        kind = m_policeStation.HasComponent(entity) ? VehicleSourceKind.Police
                            : m_hospital.HasComponent(entity) ? VehicleSourceKind.Hospital
                            : m_deathcareFacility.HasComponent(entity) ? VehicleSourceKind.Deathcare
                            : m_fireStation.HasComponent(entity) ? VehicleSourceKind.FireResponse
                            : m_garbageFacility.HasComponent(entity) ? VehicleSourceKind.Garbage
                            : m_transportDepot.HasComponent(entity) ? VehicleSourceKind.PublicTransport
                            : m_cargoTransportStation.HasComponent(entity) ? VehicleSourceKind.CargoTransport
                            : m_maintenanceDepot.HasComponent(entity) ? VehicleSourceKind.Maintenance
                            : m_postFacility.HasComponent(entity) ? VehicleSourceKind.Post
                            : m_industrialCompany.HasComponent(entity) ? VehicleSourceKind.IndustrialCompany
                            : m_commercialCompany.HasComponent(entity) ? VehicleSourceKind.CommercialCompany
                            : m_transportCompany.HasComponent(entity) ? VehicleSourceKind.TransportCompany
                            : VehicleSourceKind.Other
                    };
                    m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, newCmp);
                }
            }
        }
        
#endif
    }
}

