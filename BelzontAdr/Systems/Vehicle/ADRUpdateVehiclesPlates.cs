using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem
    {
        [BurstCompile]
        private unsafe struct ADRUpdateVehiclesPlates : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public ComponentTypeHandle<ADRVehicleData> m_vehicleHdl;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ComponentLookup<Aircraft> m_aircraftLkp;
            public ComponentLookup<Watercraft> m_watercraftLkp;
            public VehiclePlateSettings roadPlateSettings;
            public VehiclePlateSettings airPlatesSettings;
            public VehiclePlateSettings waterPlatesSettings;
            public VehiclePlateSettings railVehiclesPlatesSettings;
            public ComponentLookup<Train> m_trainLkp;
            public ComponentLookup<Controller> m_controllerLkp;
            public ComponentLookup<ADRVehicleData> m_adrVehicleDataLkp;
            public ComponentLookup<ADRVehiclePlateDataDirty> m_adrVehiclePlateDataLkp;
            public BufferLookup<LayoutElement> m_layoutElementLkp;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_entityHdl);
                var vehicles = chunk.GetNativeArray(ref m_vehicleHdl);
                var length = entities.Length;

                for (int i = 0; i < length; i++)
                {
                    Entity entity = entities[i];
                    var vehicleData = vehicles[i];
                    var isTrain = m_trainLkp.HasComponent(entity);
                    var refEntity = !isTrain || !m_controllerLkp.TryGetComponent(entity, out var ctrl) || ctrl.m_Controller == entity ? entity : ctrl.m_Controller;
                    if (isTrain && refEntity != entity)
                    {
                        if (!m_adrVehicleDataLkp.TryGetComponent(refEntity, out var vehicleDataParent)
                            || m_adrVehiclePlateDataLkp.HasComponent(refEntity)
                            || !m_layoutElementLkp.TryGetBuffer(refEntity, out var layoutData)
                            )
                        {
                            continue;
                        }
                        var carNumber = 1;
                        for (; carNumber < layoutData.Length; carNumber++)
                        {
                            if (layoutData[carNumber].m_Vehicle == entity)
                            {
                                break;
                            }
                        }
                        vehicleData.calculatedPlate = railVehiclesPlatesSettings.GetPlateFor(0, vehicleDataParent.serialNumber, vehicleData.manufactureMonthsFromEpoch, carNumber);
                        vehicleData.checksumRule = railVehiclesPlatesSettings.Checksum;
                    }
                    else
                    {
                        var settingEffective =
                            isTrain ? railVehiclesPlatesSettings
                            : m_aircraftLkp.HasComponent(entity) ? airPlatesSettings
                            : m_watercraftLkp.HasComponent(entity) ? waterPlatesSettings
                            : roadPlateSettings;

                        var serialNumber = vehicleData.serialNumber;

                        vehicleData.calculatedPlate = settingEffective.GetPlateFor(0, serialNumber, vehicleData.manufactureMonthsFromEpoch);
                        vehicleData.checksumRule = settingEffective.Checksum;

                    }
                    m_cmdBuffer.SetComponent(unfilteredChunkIndex, entity, vehicleData);
                    m_cmdBuffer.RemoveComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, entity);
                }

            }
        }
    }


}

