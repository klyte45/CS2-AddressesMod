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
using Game.Zones;
using Unity.Entities;
using static Game.UI.NameSystem;
using AreaType = Game.Zones.AreaType;

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
                if (!__instance.TryGetCustomName(entity, out __result) && GetAggregateName(out pattern, out __result, entity))
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
                if (!__instance.TryGetCustomName(entity, out __result) && GetDistrictName(out pattern, out __result, entity))
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

        private static bool GetName(ref Name __result, ref NameSystem __instance, ref Entity entity, ref bool omitBrand)
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

            return true;
        }

        private static bool GetAggregateName(out string format, out string name, Entity entity)
        {
            name = format = null;
            Entity refDistrict = default;// entityManager.TryGetComponent<BorderDistrict>(refRoad, out var refDistrictBorders) ? refDistrictBorders.m_Left == default ? refDistrictBorders.m_Right : refDistrictBorders.m_Left : default;
            if (!adrMainSystem.TryGetRoadNamesList(refDistrict, out var roadsNamesList)) return true;
            if (!entityManager.TryGetBuffer<AggregateElement>(entity, true, out var elements)) return true;
            var refRoad = elements[0].m_Edge;
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
                EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                entityCommandBuffer.AddComponent(entity, adrLoc);
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
