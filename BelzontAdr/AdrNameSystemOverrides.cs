using Belzont.Utils;
using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.Localization;
using Unity.Entities;
using static Game.UI.NameSystem;

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
        }
        private static PrefabSystem prefabSystem;
        private static EntityManager entityManager;
        private static EndFrameBarrier m_EndFrameBarrier;
        private static AdrMainSystem adrMainSystem;

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
            //if (entityManager.HasComponent<Aggregate>(entity))
            //{
            //    entityManager.TryGetBuffer<AggregateElement>(entity, true, out var elements);
            //    LogUtils.DoLog("HAS AGGREGATE!");
            //}
            //if (entityManager.HasComponent<Aggregated>(entity))
            //{
            //    //entityManager.TryGetBuffer<AggregateElement>(entity, true, out var elements);
            //    LogUtils.DoLog("HAS AGGREGATED!");
            //}

            return true;
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
