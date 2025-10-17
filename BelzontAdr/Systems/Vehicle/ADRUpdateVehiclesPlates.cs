using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
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
            public VehiclePlateSettings.SafeStruct roadPlatesSettings;
            public VehiclePlateSettings.SafeStruct airPlatesSettings;
            public VehiclePlateSettings.SafeStruct waterPlatesSettings;
            public VehiclePlateSettings.SafeStruct railPlatesSettings;
            public ComponentLookup<Train> m_trainLkp;
            public ComponentLookup<Controller> m_controllerLkp;
            public ComponentLookup<ADRVehicleData> m_adrVehicleDataLkp;
            public ComponentLookup<ADRVehiclePlateDataDirty> m_adrVehiclePlateDataLkp;
            public ComponentLookup<ADRVehicleBuildingOrigin> m_spawnerDataLkp;
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
                    ushort regionalCode = ushort.MaxValue;
                    var regionalAcronym = default(FixedString32Bytes);
                    var refSerial = vehicleData.serialNumber;
                    var refEpoch = vehicleData.manufactureMonthsFromEpoch;
                    if (m_adrVehicleDataLkp.TryGetComponent(refEntity, out var dataController) && dataController.serialOwnerSource != Entity.Null && m_spawnerDataLkp.TryGetComponent(dataController.serialOwnerSource, out var spawnerData))
                    {
                        regionalCode = spawnerData.CategorySerialNumber;
                        regionalAcronym = spawnerData.customId;
                        refSerial = dataController.ownerSerialNumber;
                        refEpoch = -1;
                    }
                    if (isTrain)
                    {
                        if (!m_layoutElementLkp.TryGetBuffer(refEntity, out var layoutData))
                        {
                            continue;
                        }
                        var carNumber = CalculateTrainCarNumber(entity, refEntity, layoutData);


                        vehicleData.calculatedPlate = railPlatesSettings.GetPlateFor(regionalCode, regionalAcronym, refSerial, refEpoch, carNumber);
                        vehicleData.calculatedConvoyPrefix = railPlatesSettings.GetPlateFor(regionalCode, regionalAcronym, refSerial, refEpoch, carNumber, true);
                        vehicleData.checksumRule = railPlatesSettings.Checksum;
                    }
                    else
                    {
                        var settingEffective =
                            m_aircraftLkp.HasComponent(entity) ? airPlatesSettings
                            : m_watercraftLkp.HasComponent(entity) ? waterPlatesSettings
                            : roadPlatesSettings;

                        var serialNumber = vehicleData.serialNumber;

                        vehicleData.calculatedPlate = settingEffective.GetPlateFor(regionalCode, regionalAcronym, serialNumber, vehicleData.manufactureMonthsFromEpoch);
                        vehicleData.checksumRule = settingEffective.Checksum;

                    }
                    m_cmdBuffer.SetComponent(unfilteredChunkIndex, entity, vehicleData);
                    m_cmdBuffer.RemoveComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, entity);
                }
            }

            public static int CalculateTrainCarNumber(Entity entity, Entity refEntity, DynamicBuffer<LayoutElement> layoutData)
            {
                var carNumber = 0;
                for (; carNumber < layoutData.Length; carNumber++)
                {
                    if (layoutData[carNumber].m_Vehicle == entity)
                    {
                        break;
                    }
                }
                carNumber = layoutData[0].m_Vehicle == refEntity ? carNumber + 1 : layoutData.Length - carNumber;
                return carNumber;
            }
        }
    }


}

