using Game.Vehicles;
using System.Linq;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using static BelzontAdr.ADRVehicleSpawnerData;

namespace BelzontAdr
{

    public partial class AdrVehicleSystem
    {
        [BurstCompile]
        private struct ADRUpdateBuildingVehiclesConvoyId : IJobChunk
        {
            public VehicleSerialSettings.SafeStruct busSerialSettings;
            public VehicleSerialSettings.SafeStruct taxiSerialSettings;
            public VehicleSerialSettings.SafeStruct policeSerialSettings;
            public VehicleSerialSettings.SafeStruct firetruckSerialSettings;
            public VehicleSerialSettings.SafeStruct ambulanceSerialSettings;
            public VehicleSerialSettings.SafeStruct garbageSerialSettings;
            public VehicleSerialSettings.SafeStruct postalSerialSettings;
            public ComponentLookup<ADRVehicleData> m_vehicleDataLkp;
            public ComponentLookup<ADRVehicleSpawnerData> m_sourceDataLkp;
            public BufferLookup<LayoutElement> m_layoutElementLkp;
            public ComponentLookup<Controller> m_controllerLkp;
            public BufferLookup<OwnedVehicle> m_ownedVehiclesLkp;
            public EntityTypeHandle entityTypeHandle;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var buildingEntities = chunk.GetNativeArray(entityTypeHandle);
                var length = buildingEntities.Length;
                for (int i = 0; i < length; i++)
                {
                    Entity buildingEntity = buildingEntities[i];
                    var sourceData = m_sourceDataLkp[buildingEntity];
                    if (!m_ownedVehiclesLkp.TryGetBuffer(buildingEntity, out var ownedVehicles))
                    {
                        continue;
                    }
                    if (!sourceData.CategorySerialNumberSet)
                    {
                        m_cmdBuffer.AddComponent<ADRBuildingOwnSerialUnset>(unfilteredChunkIndex, buildingEntity);
                        continue;
                    }

                    var generator = sourceData.kind switch
                    {
                        VehicleSourceKind.PublicTransport_Bus => busSerialSettings,
                        VehicleSourceKind.Police => policeSerialSettings,
                        VehicleSourceKind.FireResponse => firetruckSerialSettings,
                        VehicleSourceKind.Hospital => ambulanceSerialSettings,
                        VehicleSourceKind.Garbage => garbageSerialSettings,
                        VehicleSourceKind.Post => postalSerialSettings,
                        VehicleSourceKind.PublicTransport_Taxi => taxiSerialSettings,
                        _ => default,
                    };

                    for (int v = 0; v < ownedVehicles.Length; v++)
                    {
                        var vehicleEntity = ownedVehicles[v].m_Vehicle;
                        if (!m_vehicleDataLkp.HasComponent(vehicleEntity) || !m_vehicleDataLkp.TryGetComponent(vehicleEntity, out var vehicleData))
                        {
                            continue;
                        }
                        if (vehicleData.ownerSerialNumber < 0 || vehicleData.serialOwnerSource != buildingEntity)
                        {
                            var refEntity = vehicleEntity;
                            var hasController = m_controllerLkp.TryGetComponent(vehicleEntity, out var controller);
                            if (hasController)
                            {
                                refEntity = controller.m_Controller;
                                hasController = refEntity != vehicleEntity;
                            }
                            var refEntityData = m_vehicleDataLkp[refEntity];
                            if (hasController && refEntityData.serialOwnerSource != buildingEntity)
                            {
                                continue;
                            }
                            vehicleData.ownerSerialNumber = hasController ? refEntityData.ownerSerialNumber : sourceData.GetNextInternalSerial();
                            vehicleData.serialOwnerSource = buildingEntity;
                            m_cmdBuffer.AddComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, vehicleEntity);
                            if (m_layoutElementLkp.TryGetBuffer(refEntity, out var layoutElements))
                            {
                                for (int k = 0; k < layoutElements.Length; k++)
                                {
                                    var element = layoutElements[k];
                                    if (element.m_Vehicle == refEntity || element.m_Vehicle == vehicleEntity) continue;
                                    if (m_vehicleDataLkp.TryGetComponent(element.m_Vehicle, out var linkedVehicleData))
                                    {
                                        linkedVehicleData.ownerSerialNumber = vehicleData.ownerSerialNumber;
                                        linkedVehicleData.serialOwnerSource = buildingEntity;
                                        m_cmdBuffer.SetComponent(unfilteredChunkIndex, element.m_Vehicle, linkedVehicleData);
                                        m_cmdBuffer.AddComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, element.m_Vehicle);
                                    }
                                }
                            }
                        }
                        if (vehicleData.plateCategory == ADRVehicleData.VehiclePlateCategory.Rail)
                        {
                            m_cmdBuffer.SetComponent(unfilteredChunkIndex, vehicleEntity, vehicleData);
                            m_cmdBuffer.RemoveComponent<ADRVehicleSerialDataDirty>(unfilteredChunkIndex, vehicleEntity);
                            continue;
                        }
                        if (generator.Checksum != default)
                        {
                            var buildingId = sourceData.customId;
                            if (buildingId.IsEmpty)
                            {
                                buildingId.Append(sourceData.CategorySerialNumber);
                            }
                            vehicleData.calculatedConvoyPrefix = generator.GetSerialFor(buildingId, (ulong)vehicleData.ownerSerialNumber);
                        }
                        else if (vehicleData.plateCategory == ADRVehicleData.VehiclePlateCategory.Road)
                        {
                            vehicleData.calculatedConvoyPrefix = new FixedString32Bytes();
                            vehicleData.calculatedConvoyPrefix.Append(vehicleData.ownerSerialNumber);
                            while (vehicleData.calculatedConvoyPrefix.Length < 3)
                            {
                                var temp = new FixedString32Bytes();
                                for (int c = 0; c < 3 - vehicleData.calculatedConvoyPrefix.Length; c++)
                                {
                                    temp.Append(vehicleData.calculatedConvoyPrefix[c]);
                                }
                                temp.Append(vehicleData.calculatedConvoyPrefix);
                                vehicleData.calculatedConvoyPrefix = temp;
                            }
                        }
                        m_cmdBuffer.SetComponent(unfilteredChunkIndex, vehicleEntity, vehicleData);
                        m_cmdBuffer.RemoveComponent<ADRVehicleSerialDataDirty>(unfilteredChunkIndex, vehicleEntity);
                    }
                    m_cmdBuffer.SetComponent(unfilteredChunkIndex, buildingEntity, sourceData);
                    m_cmdBuffer.RemoveComponent<ADRBuildingVehiclesSerialDirty>(unfilteredChunkIndex, buildingEntity);
                }
            }
        }

    }
}

