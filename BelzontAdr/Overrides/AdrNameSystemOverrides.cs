using Belzont.Interfaces;
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

            var getCitizenName = typeof(NameSystem).GetMethod("GetCitizenName", RedirectorUtils.allFlags);
            AddRedirect(getCitizenName, GetType().GetMethod(nameof(GetCitizenName), RedirectorUtils.allFlags));

            var getName = typeof(NameSystem).GetMethod("GetName", RedirectorUtils.allFlags);
            AddRedirect(getName, GetType().GetMethod(nameof(GetName), RedirectorUtils.allFlags));

            var getRenderedLabelName = typeof(NameSystem).GetMethod("GetRenderedLabelName", RedirectorUtils.allFlags);
            AddRedirect(getRenderedLabelName, GetType().GetMethod(nameof(GetRenderedLabelName), RedirectorUtils.allFlags));

            var getSpawnableBuildingName = typeof(NameSystem).GetMethod("GetSpawnableBuildingName", RedirectorUtils.allFlags);
            AddRedirect(getSpawnableBuildingName, GetType().GetMethod(nameof(GetSpawnableBuildingName), RedirectorUtils.allFlags));

            var getMarkerTransportStopName = typeof(NameSystem).GetMethod("GetMarkerTransportStopName", RedirectorUtils.allFlags);
            AddRedirect(getMarkerTransportStopName, GetType().GetMethod(nameof(GetMarkerTransportStopName), RedirectorUtils.allFlags));

            var getStaticTransportStopName = typeof(NameSystem).GetMethod("GetStaticTransportStopName", RedirectorUtils.allFlags);
            AddRedirect(getStaticTransportStopName, GetType().GetMethod(nameof(GetStaticTransportStopName), RedirectorUtils.allFlags));

            var getFamilyName = typeof(NameSystem).GetMethod("GetFamilyName", RedirectorUtils.allFlags);
            AddRedirect(getFamilyName, GetType().GetMethod(nameof(GetFamilyName), RedirectorUtils.allFlags));

            var getResidentName = typeof(NameSystem).GetMethod("GetResidentName", RedirectorUtils.allFlags);
            AddRedirect(getResidentName, GetType().GetMethod(nameof(GetResidentName), RedirectorUtils.allFlags));
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
            return GetName_Internal(ref __result, ref __instance, entity, new(), entity);
        }

        private static bool GetName_Internal(ref Name __result, ref NameSystem __instance, Entity entity, HashSet<Entity> pastEntities, Entity original)
        {
            if (__instance.TryGetCustomName(entity, out string name))
            {
                __result = NameSystem.Name.CustomName(name);
                return false;
            }
            if (entityManager.HasComponent<Household>(entity))
            {
                if (!adrMainSystem.TryGetSurnameList(out var surnames)) return true;
                DynamicBuffer<HouseholdCitizen> buffer = entityManager.GetBuffer<HouseholdCitizen>(entity, false);
                var hasMale = false;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (entityManager.TryGetComponent((Entity)buffer[i], out Citizen citizen) && (citizen.m_State & CitizenFlags.Male) != CitizenFlags.None)
                    {
                        hasMale = true;
                        break;
                    }
                }
                __result = NameSystem.Name.FormattedName("K45::ADR.main[localesFmt.household]", "surname", GenerateSurname(hasMale, surnames, entity));
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
                    __result = Name.CustomName(entityManager.HasComponent<Building>(original) ? genName : pattern.Replace("{name}", genName));
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
            var wasOriginal = entity == original;
            while (entityManager.TryGetComponent<Owner>(entity, out var owner))
            {
                entity = owner.m_Owner;
            }
            if (entityManager.TryGetComponent<ADREntityManualBuildingRef>(entity, out var manualRef) && manualRef.m_refNamedEntity != Entity.Null)
            {
                if (pastEntities.Contains(manualRef.m_refNamedEntity))
                {
                    entityManager.RemoveComponent<ADREntityManualBuildingRef>(manualRef.m_refNamedEntity);
                }
                pastEntities.Add(entity);
                return GetName_Internal(ref __result, ref __instance, manualRef.m_refNamedEntity, pastEntities, wasOriginal ? entity : original);
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
                    var refStation = entityStationRef.m_refStationBuilding;
                    if (refStation == entity)
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
                    Entity refAggregate = GetMainReferenceAggregate(entity, buildingData);
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

        internal static Entity GetMainReferenceAggregate(Entity entity, Building buildingData)
        {
            Queue<Entity> roadsToMap = new Queue<Entity>();
            HashSet<Entity> roadsMapped = new();
            roadsToMap.Enqueue(buildingData.m_RoadEdge);
            Entity refAggregate = Entity.Null;
            int maxIterations = 30;
            while (roadsToMap.TryDequeue(out Entity nextItem))
            {
                if (maxIterations-- == 0) break;
                if (!entityManager.TryGetComponent<Aggregated>(nextItem, out var aggNextItem)) continue;
                var hasAggRef = entityManager.TryGetComponent<ADREntityStationRef>(aggNextItem.m_Aggregate, out var aggregationStationRef);
                if (!entityManager.HasComponent<Road>(nextItem) || (hasAggRef && aggregationStationRef.m_refStationBuilding != Entity.Null && aggregationStationRef.m_refStationBuilding != entity))
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

            return refAggregate;
        }


        private static bool GetAggregateName(out string format, out string name, Entity entity)
        {
            name = format = null;
            if (!adrMainSystem.FindReferenceRoad(entity, out DynamicBuffer<AggregateElement> elements, out Entity refRoad)) return true;
            if (!adrMainSystem.GetRoadNameList(refRoad, out var roadsNamesList)) return true;
            if (!entityManager.TryGetComponent<PrefabRef>(refRoad, out var roadPrefab)) return true;
            if (!entityManager.TryGetComponent<RoadData>(roadPrefab, out var roadData)) return true;
            var fullBridge = true;
            for (int i = 0; i < elements.Length; i++)
            {
                if (!entityManager.TryGetComponent<Elevation>(elements[i].m_Edge, out var elevData) || elevData.m_Elevation[0] < 6)
                {
                    fullBridge = false;
                    break;
                }
            }

            format = adrMainSystem.CurrentCitySettings.RoadPrefixSetting.GetFirstApplicable(roadData, fullBridge).FormatPattern;

            name = GetFromList(roadsNamesList, entity, allowNull: true);
            return false;
        }


        private static bool GetDistrictName(out string format, out string name, Entity entity)
        {
            name = format = null;
            if (!adrMainSystem.TryGetDistrictNamesList(out var districtNamesList)) return true;

            format = "{name}";// adrMainSystem.CurrentCitySettings.RoadPrefixSetting.GetFirstApplicable(roadData).FormatPattern;

            name = GetFromList(districtNamesList, entity, allowNull: true);
            return false;
        }

        private static bool GetCitizenName(ref Name __result, ref Entity entity)
        {
            var isCitizen = entityManager.TryGetComponent(entity, out Citizen citizen);
            if (!isCitizen) return true;
            return CommonGetCitizenName(ref __result, entity, citizen);
        }

        private static bool GetResidentName(ref Name __result, Entity entity)
        {
            return !entityManager.TryGetComponent(entity, out Game.Creatures.Resident resident)
                || !entityManager.TryGetComponent(resident.m_Citizen, out Citizen citizen)
                || CommonGetCitizenName(ref __result, entity, citizen);
        }

        private static bool CommonGetCitizenName(ref Name __result, Entity entity, Citizen citizen)
        {
            bool male = (citizen.m_State & CitizenFlags.Male) != CitizenFlags.None;
            var hasListForNames = adrMainSystem.TryGetNameList(male, out var listForNames);
            var hasListForSurnames = adrMainSystem.TryGetSurnameList(out var listForSurnames);
            if (!hasListForNames && !hasListForSurnames)
            {
                return true;
            }
            string name, surname;
            if (hasListForNames)
            {
                name = GetFromList(listForNames, entity, namesToExtract: adrMainSystem.CurrentCitySettings.MaximumGeneratedGivenNames);
            }
            else
            {
                GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetId(entity, true), out name);
            }
            HouseholdMember householdMemberData = entityManager.GetComponentData<HouseholdMember>(entity);
            surname = GenerateSurname(male, listForSurnames, householdMemberData.m_Household);
            __result = Name.CustomName(adrMainSystem.DoNameFormat(name, surname));
            return false;
        }

        private static string GenerateSurname(bool male, AdrNameFile listForSurnames, Entity household)
            => listForSurnames != null ? GetFromList(listForSurnames, household, namesToExtract: adrMainSystem.CurrentCitySettings.MaximumGeneratedSurnames, isAlternative: male)
                            : GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetGenderedLastNameId(household, male), out var surname) ? surname
                            : "???";

        private static ADRRandomizationData? GetAdrLocData(Entity entity, bool allowNull)
        {
            if (!entityManager.TryGetComponent(entity, out ADRRandomizationData data))
            {
                if (allowNull)
                {
                    return null;
                }
            }

            if (data.SeedIdentifier == 0)
            {
                data.Redraw();
                entityManager.SetComponentData(entity, data);
            }

            return data;
        }

        private static string GetFromList(AdrNameFile namesFile, Entity entityRef, int namesToExtract = 1, bool isAlternative = false, bool allowNull = false)
        {
            var adrLoc = GetAdrLocData(entityRef, allowNull);
            if (adrLoc == null) return null;
            var adrLocEnsured = adrLoc ?? throw new System.Exception("IMPUSSIBRU");
            var name = new HashSet<string>();
            var currentValue = adrLocEnsured.SeedIdentifier;
            var refList = isAlternative ? namesFile.ValuesAlternative : namesFile.Values;

            var countNames = (uint)refList.Count;
            for (int i = 0; i < namesToExtract; i++)
            {
                var idx = currentValue % (countNames + 1);
                if (idx < countNames || currentValue == 0)
                {
                    name.Add(refList[(int)(currentValue % countNames)]);
                }
                else if (i == 0)
                {
                    i--;
                }
                currentValue /= countNames + 1;
            }
            var result = string.Join(" ", name);
            if (BasicIMod.VerboseMode) LogUtils.DoVerboseLog($"Generated name for Entity {entityRef}: '{result}' ({adrLocEnsured.SeedIdentifier} x{namesToExtract} - ALT = {isAlternative})");
            return result;
        }

        private static string GetGenderedLastNameId(Entity household, bool male)
        {
            if (household == Entity.Null)
            {
                return null;
            }
            if (!entityManager.TryGetComponent(household, out PrefabRef refData) || !prefabSystem.GetPrefab<PrefabBase>(refData).TryGet(out RandomGenderedLocalization randomGenderedLocalization))
            {
                return GetId(household, true);
            }
            string text = male ? randomGenderedLocalization.m_MaleID : randomGenderedLocalization.m_FemaleID;
            return entityManager.TryGetBuffer(household, true, out DynamicBuffer<RandomLocalizationIndex> dynamicBuffer) && dynamicBuffer.Length > 0
                ? LocalizationUtils.AppendIndex(text, dynamicBuffer[0])
                : text;
        }

        private static string GetId(Entity entity, bool useRandomLocalization = true)
        {
            if (entity == Entity.Null)
            {
                return null;
            }
            Entity entity2 = Entity.Null;
            if (entityManager.TryGetComponent(entity, out PrefabRef prefabRef))
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
        private static bool GetFamilyName(ref NameSystem __instance, ref Entity household, ref Name __result)
        {
            if (!adrMainSystem.TryGetSurnameList(out var listForSurnames))
            {
                return true;
            }
            if (__instance.TryGetCustomName(household, out var customName))
            {
                __result = Name.CustomName(customName);
                return false;
            }
            DynamicBuffer<HouseholdCitizen> buffer = entityManager.GetBuffer<HouseholdCitizen>(household, false);
            var hasMale = false;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (entityManager.TryGetComponent((Entity)buffer[i], out Citizen citizen) && (citizen.m_State & CitizenFlags.Male) != CitizenFlags.None)
                {
                    hasMale = true;
                    break;
                }
            }
            __result = Name.CustomName(GenerateSurname(hasMale, listForSurnames, household));
            return false;
        }
    }

}