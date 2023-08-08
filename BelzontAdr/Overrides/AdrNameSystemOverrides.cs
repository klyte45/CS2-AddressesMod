using Belzont.Utils;
using Colossal.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Companies;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.Localization;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using static Game.UI.NameSystem;
using AreaType = Game.Zones.AreaType;
using CargoTransportStation = Game.Buildings.CargoTransportStation;

namespace BelzontAdr
{
    public class AdrNameSystemOverrides : Redirector, IRedirectable
    {
        public void DoPatches(World world)
        {
            prefabSystem = world.GetExistingSystemManaged<PrefabSystem>();
            adrMainSystem = world.GetOrCreateSystemManaged<AdrMainSystem>();
            entityManager = world.EntityManager;
            m_EndFrameBarrier = world.GetOrCreateSystemManaged<EndFrameBarrier>();

            var GetCitizenName = typeof(NameSystem).GetMethod("GetCitizenName", RedirectorUtils.allFlags);
            AddRedirect(GetCitizenName, GetType().GetMethod("GetCitizenName", RedirectorUtils.allFlags));

            var GetName = typeof(NameSystem).GetMethod("GetName", RedirectorUtils.allFlags);
            AddRedirect(GetName, GetType().GetMethod("GetName", RedirectorUtils.allFlags));

            var GetRenderedLabelName = typeof(NameSystem).GetMethod("GetRenderedLabelName", RedirectorUtils.allFlags);
            AddRedirect(GetRenderedLabelName, GetType().GetMethod("GetRenderedLabelName", RedirectorUtils.allFlags));

            var GetSpawnableBuildingName = typeof(NameSystem).GetMethod("GetSpawnableBuildingName", RedirectorUtils.allFlags);
            AddRedirect(GetSpawnableBuildingName, GetType().GetMethod("GetSpawnableBuildingName", RedirectorUtils.allFlags));

            var GetMarkerTransportStopName = typeof(NameSystem).GetMethod("GetMarkerTransportStopName", RedirectorUtils.allFlags);
            AddRedirect(GetMarkerTransportStopName, GetType().GetMethod("GetMarkerTransportStopName", RedirectorUtils.allFlags));

            var GetStaticTransportStopName = typeof(NameSystem).GetMethod("GetStaticTransportStopName", RedirectorUtils.allFlags);
            AddRedirect(GetStaticTransportStopName, GetType().GetMethod("GetStaticTransportStopName", RedirectorUtils.allFlags));
        }
        private static PrefabSystem prefabSystem;
        private static EntityManager entityManager;
        private static EndFrameBarrier m_EndFrameBarrier;
        private static AdrMainSystem adrMainSystem;


        public static bool GetRenderedLabelName(ref string __result, ref NameSystem __instance, ref Entity entity)
        {
            string pattern = null;
            if (entityManager.HasComponent<Aggregate>(entity))
            {
                if (__instance.TryGetCustomName(entity, out __result)) return false;
                if (GetAggregateName(out pattern, out __result, entity))
                {
                    string id = GetId(entity, true);
                    __result = GameManager.instance.localizationManager.activeDictionary.TryGetValue(id, out string result2) ? result2 : id;
                    return false;
                }
                __result = pattern.Replace("{name}", __result);
                return false;
            }
            else if (entityManager.HasComponent<District>(entity))
            {
                if (__instance.TryGetCustomName(entity, out __result)) return false;
                if (GetDistrictName(out pattern, out __result, entity))
                {
                    string id = GetId(entity, true);
                    __result = GameManager.instance.localizationManager.activeDictionary.TryGetValue(id, out string result2) ? result2 : id;
                    return false;
                }
                __result = pattern.Replace("{name}", __result);
                return false;
            }
            return true;
        }
        private static bool GetMarkerTransportStopName(ref Name __result, ref NameSystem __instance, ref Entity stop)
        {
            Entity entity = stop;
            int num = 0;
            while (num < 8 && entityManager.TryGetComponent(entity, out Owner owner))
            {
                entity = owner.m_Owner;
                num++;
            }
            if (entity != stop)
            {
                __result = __instance.GetName(entity);
                return false;
            }
            return true;
        }
        private static bool GetStaticTransportStopName(ref Name __result, ref NameSystem __instance, ref Entity stop)
        {
            BuildingUtils.GetAddress(entityManager, stop, out Entity entity, out int num);
            if (GetAggregateName(out var pattern, out var genName, entity))
            {
                return true;
            }
            var roadName = pattern.Replace("{name}", genName);
            __result = NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", new string[]
               {
                        "ROAD",
                        roadName,
                        "NUMBER",
                        num.ToString()
               });
            return false;
        }

        private static bool GetSpawnableBuildingName(ref Name __result, ref Entity building, ref Entity zone, ref bool omitBrand)
        {
            BuildingUtils.GetAddress(entityManager, building, out Entity entity, out int num);
            if (GetAggregateName(out var pattern, out var genName, entity))
            {
                return true;
            }
            var roadName = pattern.Replace("{name}", genName);
            ZonePrefab prefab = prefabSystem.GetPrefab<ZonePrefab>(zone);
            if (!omitBrand && prefab.m_AreaType != AreaType.Residential)
            {
                string brandId = GetBrandId(building);
                if (brandId != null)
                {
                    __result = NameSystem.Name.FormattedName("Assets.NAMED_ADDRESS_NAME_FORMAT", new string[]
                    {
                        "NAME",
                        brandId,
                        "ROAD",
                        roadName,
                        "NUMBER",
                        num.ToString()
                    });
                    return false;
                }
            }
            __result = NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", new string[]
            {
                "ROAD",
                roadName,
                "NUMBER",
                num.ToString()
            });
            return false;
        }

        private static string GetBrandId(Entity building)
        {
            DynamicBuffer<Renter> buffer = entityManager.GetBuffer<Renter>(building, true);
            for (int i = 0; i < buffer.Length; i++)
            {
                CompanyData companyData;
                if (entityManager.TryGetComponent(buffer[i].m_Renter, out companyData))
                {
                    return GetId(companyData.m_Brand, true);
                }
            }
            return null;
        }

        private static bool GetName(ref Name __result, ref NameSystem __instance, Entity entity)
        {
            if (__instance.TryGetCustomName(entity, out string name))
            {
                __result = NameSystem.Name.CustomName(name);
                return false;
            }
            if (entityManager.HasComponent<Household>(entity))
            {
                if (!adrMainSystem.TryGetSurnameList(out var surnames)) return true;

                __result = NameSystem.Name.FormattedName("K45::ADR.main[localesFmt.household]", "surname", GetFromList(surnames, entity));
                return false;
            }
            if (entityManager.HasComponent<HouseholdPet>(entity))
            {
                entityManager.TryGetComponent<PrefabRef>(entity, out var petPrefabRef);
                entityManager.TryGetComponent<HouseholdPetData>(petPrefabRef, out var petData);
                switch (petData.m_Type)
                {
                    case PetType.Dog:
                        if (!adrMainSystem.TryGetDogsList(out var dogs)) return true;
                        __result = NameSystem.Name.CustomName(GetFromList(dogs, entity));
                        return false;
                    default:
                        return true;
                }
            }
            if (entityManager.HasComponent<Aggregate>(entity))
            {
                var shallRunOrignal = GetAggregateName(out var pattern, out var genName, entity);
                if (!shallRunOrignal)
                {
                    __result = Name.CustomName(pattern.Replace("{name}", genName));
                }
                return shallRunOrignal;
            }
            if (entityManager.HasComponent<District>(entity))
            {
                var shallRunOrignal = GetDistrictName(out var pattern, out var genName, entity);
                if (!shallRunOrignal)
                {
                    __result = Name.CustomName(pattern.Replace("{name}", genName));
                }
                return shallRunOrignal;
            }
            while (entityManager.TryGetComponent<Owner>(entity, out var owner))
            {
                entity = owner.m_Owner;
            }
            if (__instance.TryGetCustomName(entity, out name))
            {
                __result = NameSystem.Name.CustomName(name);
                return false;
            }
            if (entityManager.TryGetComponent<Building>(entity, out var buildingData))
            {
                if (((adrMainSystem.CurrentCitySettings.DistrictNameAsNameStation && entityManager.HasComponent<PublicTransportStation>(entity))
                    || (adrMainSystem.CurrentCitySettings.DistrictNameAsNameCargoStation && entityManager.HasComponent<CargoTransportStation>(entity)))
                    && entityManager.TryGetComponent<CurrentDistrict>(entity, out var currDistrict) && currDistrict.m_District != Entity.Null)
                {
                    if (!entityManager.TryGetComponent<ADREntityStationRef>(currDistrict.m_District, out var entityStationRef))
                    {
                        var cmd = m_EndFrameBarrier.CreateCommandBuffer();
                        entityStationRef = new ADREntityStationRef
                        {
                            m_refStationBuilding = entity
                        };
                        cmd.AddComponent(currDistrict.m_District, entityStationRef);
                    }
                    if (entityStationRef.m_refStationBuilding == Entity.Null)
                    {
                        var cmd = m_EndFrameBarrier.CreateCommandBuffer();
                        entityStationRef.m_refStationBuilding = entity;
                        cmd.SetComponent(currDistrict.m_District, entityStationRef);
                    }
                    if (entityStationRef.m_refStationBuilding == entity)
                    {
                        if (GetDistrictName(out var pattern, out var mainName, currDistrict.m_District))
                        {
                            string id = GetId(entity, true);
                            __result = Name.LocalizedName(GameManager.instance.localizationManager.activeDictionary.TryGetValue(id, out string result2) ? result2 : id);
                            return false;
                        }
                        __result = Name.CustomName(pattern.Replace("{name}", mainName));
                        return false;
                    }
                }
                if (
                    ((adrMainSystem.CurrentCitySettings.RoadNameAsNameStation && entityManager.HasComponent<PublicTransportStation>(entity))
                    || (adrMainSystem.CurrentCitySettings.RoadNameAsNameCargoStation && entityManager.HasComponent<CargoTransportStation>(entity)))
                    && entityManager.HasComponent<Aggregated>(buildingData.m_RoadEdge))
                {

                    Queue<Entity> roadsToMap = new Queue<Entity>();
                    HashSet<Entity> roadsMapped = new();
                    roadsToMap.Enqueue(buildingData.m_RoadEdge);
                    Entity refAggregate = Entity.Null;
                    int maxIterations = 20;
                    while (roadsToMap.TryDequeue(out Entity nextItem))
                    {
                        if (maxIterations-- == 0) break;
                        if (!entityManager.TryGetComponent<Aggregated>(nextItem, out var aggNextItem)) continue;
                        var hasAggRef = entityManager.TryGetComponent<ADREntityStationRef>(aggNextItem.m_Aggregate, out var aggregationStationRef);
                        if (hasAggRef && aggregationStationRef.m_refStationBuilding != Entity.Null && aggregationStationRef.m_refStationBuilding != entity)
                        {
                            if (roadsMapped.Add(nextItem) && entityManager.TryGetComponent(nextItem, out Edge edge))
                            {
                                if (entityManager.TryGetBuffer(edge.m_Start, true, out DynamicBuffer<ConnectedEdge> connectedStart))
                                {
                                    for (int k = 0; k < connectedStart.Length; k++)
                                    {
                                        if (!roadsMapped.Contains(connectedStart[k].m_Edge))
                                        {
                                            roadsToMap.Enqueue(connectedStart[k].m_Edge);
                                        }
                                    }
                                }
                                if (entityManager.TryGetBuffer(edge.m_End, true, out DynamicBuffer<ConnectedEdge> connectedEnd))
                                {
                                    for (int k = 0; k < connectedEnd.Length; k++)
                                    {
                                        if (!roadsMapped.Contains(connectedEnd[k].m_Edge))
                                        {
                                            roadsToMap.Enqueue(connectedEnd[k].m_Edge);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (hasAggRef)
                            {
                                if (aggregationStationRef.m_refStationBuilding != entity)
                                {
                                    EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                                    aggregationStationRef.m_refStationBuilding = entity;
                                    entityCommandBuffer.SetComponent(aggNextItem.m_Aggregate, aggregationStationRef);
                                }
                            }
                            else
                            {
                                EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                                aggregationStationRef = new()
                                {
                                    m_refStationBuilding = entity
                                };
                                entityCommandBuffer.AddComponent(aggNextItem.m_Aggregate, aggregationStationRef);
                            }
                            refAggregate = aggNextItem.m_Aggregate;
                            break;
                        }
                    }
                    if (refAggregate == Entity.Null)
                    {
                        return true;
                    }

                    __result = __instance.TryGetCustomName(refAggregate, out name) ? Name.CustomName(name)
                        : GetAggregateName(out _, out string roadName, refAggregate) ? __instance.GetName(refAggregate)
                        : Name.CustomName(roadName);
                    return false;
                }
            }

            return true;
        }

        private static bool GetAggregateName(out string format, out string name, Entity entity)
        {
            name = format = null;
            if (!entityManager.TryGetBuffer<AggregateElement>(entity, true, out var elements)) return true;
            if (!entityManager.TryGetComponent<EdgeGeometry>(elements[0].m_Edge, out var geom0)) return true;
            if (!entityManager.TryGetComponent<EdgeGeometry>(elements[^1].m_Edge, out var geomLast)) return true;
            var refRoad = math.length(geom0.m_Bounds.min) < math.length(geomLast.m_Bounds.min) ? elements[0].m_Edge : elements[^1].m_Edge;
            Entity refDistrict = entityManager.TryGetComponent<BorderDistrict>(refRoad, out var refDistrictBorders) ? refDistrictBorders.m_Left == default ? refDistrictBorders.m_Right : refDistrictBorders.m_Left : default;
            if (!adrMainSystem.TryGetRoadNamesList(refDistrict, out var roadsNamesList)) return true;
            if (!entityManager.TryGetComponent<PrefabRef>(refRoad, out var roadPrefab)) return true;
            if (!entityManager.TryGetComponent<RoadData>(roadPrefab, out var roadData)) return true;
            format = adrMainSystem.CurrentCitySettings.RoadPrefixSetting.GetFirstApplicable(roadData).FormatPattern;

            name = GetFromList(roadsNamesList, entity);
            return false;
        }

        private static bool GetDistrictName(out string format, out string name, Entity entity)
        {
            name = format = null;
            if (!adrMainSystem.TryGetDistrictNamesList(out var districtNamesList)) return true;

            format = "{name}";// adrMainSystem.CurrentCitySettings.RoadPrefixSetting.GetFirstApplicable(roadData).FormatPattern;

            name = GetFromList(districtNamesList, entity);
            return false;
        }

        private static bool GetCitizenName(ref Name __result, ref Entity entity, ref Entity prefab)
        {
            bool male = entityManager.TryGetComponent(entity, out Citizen citizen) && (citizen.m_State & CitizenFlags.Male) != CitizenFlags.None;
            var hasListForNames = adrMainSystem.TryGetNameList(male, out var listForNames);
            var hasListForSurnames = adrMainSystem.TryGetSurnameList(out var listForSurnames);
            if (!hasListForNames && !hasListForSurnames)
            {
                return true;
            }
            string name, surname;
            if (hasListForNames)
            {
                name = GetFromList(listForNames, entity);
            }
            else
            {
                GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetId(entity, true), out name);
            }
            HouseholdMember householdMemberData = entityManager.GetComponentData<HouseholdMember>(entity);
            surname = hasListForSurnames
                ? GetFromList(listForSurnames, householdMemberData.m_Household)
                : GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetGenderedLastNameId(householdMemberData.m_Household, male), out surname) ? surname : "???";
            __result = Name.CustomName(adrMainSystem.DoNameFormat(name, surname));
            return false;
        }

        private static ADRLocalizationData GetAdrLocData(Entity entity)
        {
            if (!entityManager.TryGetComponent(entity, out ADRLocalizationData adrLoc))
            {
                adrLoc.m_seedReference = (ushort)entity.Index;
                adrMainSystem.EnqueueToRunOnUpdate(() =>
                {
                    if (!entityManager.HasComponent<ADRLocalizationData>(entity))
                    {
                        EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                        entityCommandBuffer.AddComponent(entity, adrLoc);
                    }
                });
            }

            return adrLoc;
        }

        private static string GetFromList(AdrNameFile namesFile, Entity entityRef)
        {
            var adrLoc = GetAdrLocData(entityRef);
            string surname = namesFile.Values[adrLoc.m_seedReference % namesFile.Values.Length];
            return surname;
        }

        private static string GetGenderedLastNameId(Entity household, bool male)
        {
            if (household == Entity.Null)
            {
                return null;
            }
            PrefabRef refData;
            RandomGenderedLocalization randomGenderedLocalization;
            if (!entityManager.TryGetComponent(household, out refData) || !prefabSystem.GetPrefab<PrefabBase>(refData).TryGet<RandomGenderedLocalization>(out randomGenderedLocalization))
            {
                return GetId(household, true);
            }
            string text = male ? randomGenderedLocalization.m_MaleID : randomGenderedLocalization.m_FemaleID;
            DynamicBuffer<RandomLocalizationIndex> dynamicBuffer;
            if (entityManager.TryGetBuffer(household, true, out dynamicBuffer) && dynamicBuffer.Length > 0)
            {
                return LocalizationUtils.AppendIndex(text, dynamicBuffer[0]);
            }
            return text;
        }

        private static string GetId(Entity entity, bool useRandomLocalization = true)
        {
            if (entity == Entity.Null)
            {
                return null;
            }
            Entity entity2 = Entity.Null;
            PrefabRef prefabRef;
            if (entityManager.TryGetComponent(entity, out prefabRef))
            {
                entity2 = prefabRef.m_Prefab;
            }
            SpawnableBuildingData spawnableBuildingData;
            if (!entityManager.HasComponent<SignatureBuildingData>(entity2) && entityManager.TryGetComponent(entity2, out spawnableBuildingData))
            {
                entity2 = spawnableBuildingData.m_ZonePrefab;
            }
            if (entityManager.HasComponent<ChirperAccountData>(entity) || entityManager.HasComponent<BrandData>(entity))
            {
                entity2 = entity;
            }
            if (!(entity2 != Entity.Null))
            {
                return string.Empty;
            }
            if (!prefabSystem.TryGetPrefab(entity2, out PrefabBase prefabBase))
            {
                return "Assets.NAME[" + prefabSystem.GetObsoleteID(entity2).GetName() + "]";
            }
            if (!prefabBase.TryGet(out Localization localization))
            {
                return "Assets.NAME[" + prefabBase.name + "]";
            }
            DynamicBuffer<RandomLocalizationIndex> dynamicBuffer;
            if (useRandomLocalization && localization is RandomLocalization && entityManager.TryGetBuffer(entity, true, out dynamicBuffer) && dynamicBuffer.Length > 0)
            {
                return LocalizationUtils.AppendIndex(localization.m_LocalizationID, dynamicBuffer[0]);
            }
            return localization.m_LocalizationID;
        }
    }
}
