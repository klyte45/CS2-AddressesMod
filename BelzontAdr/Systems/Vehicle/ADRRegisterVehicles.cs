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
            public VehiclePlateSettings.SafeStruct roadPlatesSettings;
            public VehiclePlateSettings.SafeStruct airPlatesSettings;
            public VehiclePlateSettings.SafeStruct waterPlatesSettings;
            public VehiclePlateSettings.SafeStruct railPlatesSettings;



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
                        var newItem = new ADRVehicleData
                        {
                            plateCategory = ADRVehicleData.VehiclePlateCategory.Rail,
                            serialNumber = serialNumber,
                            manufactureMonthsFromEpoch = m_refDateTime,
                            calculatedPlate = new Unity.Collections.FixedString32Bytes(),
                            calculatedConvoyPrefix = new Unity.Collections.FixedString32Bytes(),
                        };
                        m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, newItem);
                        m_cmdBuffer.AddComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, entity);
                    }
                    else
                    {
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
                            calculatedPlate = new Unity.Collections.FixedString32Bytes(),
                            calculatedConvoyPrefix = new Unity.Collections.FixedString32Bytes(),
                        };
                        m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, newItem);
                        m_cmdBuffer.AddComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, entity);
                        m_cmdBuffer.AddComponent<ADRVehicleSerialDataDirty>(unfilteredChunkIndex, entity);
#if DEBUG
                        //       if (newItem.serialNumber % 5 == 0) UnityEngine.Debug.Log($"Added serial nº: {newItem.serialNumber} => {newItem.calculatedPlate}");
#endif
                    }
                }

            }
        }

    }


}

