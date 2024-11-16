using Colossal;
using Game.Vehicles;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem
    {
        [BurstCompile]
        private unsafe struct ADRRegisterVehicles : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public NativeCounter.Concurrent m_serialNumber;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ComponentLookup<Aircraft> m_aircraftLkp;
            public ComponentLookup<Watercraft> m_watercraftLkp;
            public ComponentLookup<Train> m_trainLkp;
            public ComponentLookup<Controller> m_controllerLkp;
            public ComponentLookup<ADRVehicleData> m_adrVehicleDataLkp;
            public ComponentLookup<ADRVehiclePlateDataDirty> m_adrVehiclePlateDataLkp;
            public BufferLookup<LayoutElement> m_layoutElementLkp;
            public ulong refSerialNumber;
            public int m_refDateTime;
            public VehiclePlateSettings roadPlateSettings;
            public VehiclePlateSettings airPlatesSettings;
            public VehiclePlateSettings waterPlatesSettings;
            public VehiclePlateSettings railVehiclesPlatesSettings;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_entityHdl);
                var length = entities.Length;
                //   Debug.Log($"Chunk #{unfilteredChunkIndex} size: {length}");
                for (int i = 0; i < length; i++)
                {
                    Entity entity = entities[i];
                    var isTrain = m_trainLkp.HasComponent(entity);
                    var refEntity = !isTrain || !m_controllerLkp.TryGetComponent(entity, out var ctrl) || ctrl.m_Controller == entity ? entity : ctrl.m_Controller;
                    var serialNumber = refSerialNumber + (uint)m_serialNumber.Increment();
                    if (isTrain && refEntity != entity)
                    {
                        if (!m_adrVehicleDataLkp.TryGetComponent(refEntity, out var vehicleDataParent)
                            || m_adrVehiclePlateDataLkp.HasComponent(refEntity)
                            || !m_layoutElementLkp.TryGetBuffer(refEntity, out var layoutData)
                            )
                        {
                            continue;
                        }
                        int carNumber = CalculateTrainCarNumber(entity, refEntity, layoutData);
                        var newItem = new ADRVehicleData
                        {
                            plateCategory = ADRVehicleData.VehiclePlateCategory.Rail,
                            serialNumber = serialNumber,
                            manufactureMonthsFromEpoch = m_refDateTime,
                            calculatedPlate = railVehiclesPlatesSettings.GetPlateFor(0, vehicleDataParent.serialNumber, m_refDateTime, carNumber),
                            checksumRule = railVehiclesPlatesSettings.Checksum,
                        };
                        m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, newItem);
                    }
                    else
                    {
                        var settingEffective =
                            isTrain ? railVehiclesPlatesSettings
                            : m_aircraftLkp.HasComponent(entity) ? airPlatesSettings
                            : m_watercraftLkp.HasComponent(entity) ? waterPlatesSettings
                            : roadPlateSettings;
                        var plateCategory =
                            isTrain ? ADRVehicleData.VehiclePlateCategory.Rail
                            : m_aircraftLkp.HasComponent(entity) ? ADRVehicleData.VehiclePlateCategory.Air
                            : m_watercraftLkp.HasComponent(entity) ? ADRVehicleData.VehiclePlateCategory.Water
                            : ADRVehicleData.VehiclePlateCategory.Road;
                        var newItem = new ADRVehicleData
                        {
                            plateCategory = plateCategory,
                            serialNumber = serialNumber,
                            manufactureMonthsFromEpoch = m_refDateTime,
                            calculatedPlate = settingEffective.GetPlateFor(0, serialNumber, m_refDateTime),
                            checksumRule = settingEffective.Checksum,
                        };
                        m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, newItem);
#if DEBUG
                        if (newItem.serialNumber % 5 == 0) UnityEngine.Debug.Log($"Added serial nº: {newItem.serialNumber} => {newItem.calculatedPlate}");
#endif
                    }
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

