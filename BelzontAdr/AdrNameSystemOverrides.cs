using Belzont.Utils;
using Colossal.Entities;
using Game;
using Game.Citizens;
using Game.Common;
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
        }
        private static PrefabSystem prefabSystem;
        private static EntityManager entityManager;
        private static EndFrameBarrier m_EndFrameBarrier;
        private static AdrMainSystem adrMainSystem;


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
                if (!entityManager.TryGetComponent(entity, out ADRLocalizationData adrLoc))
                {
                    adrLoc.m_seedReference = (ushort)entity.Index;
                    EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                    entityCommandBuffer.AddComponent(entity, adrLoc);
                }

                name = listForNames.Values[adrLoc.m_seedReference % listForNames.Values.Length];
            }
            else
            {
                GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetId(entity, true), out name);
            }
            HouseholdMember householdMemberData = entityManager.GetComponentData<HouseholdMember>(entity);
            if (hasListForSurnames)
            {

                if (!entityManager.TryGetComponent(householdMemberData.m_Household, out ADRLocalizationData adrLoc))
                {
                    adrLoc.m_seedReference = (ushort)householdMemberData.m_Household.Index;
                    EntityCommandBuffer entityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                    entityCommandBuffer.AddComponent(householdMemberData.m_Household, adrLoc);
                }

                surname = listForSurnames.Values[adrLoc.m_seedReference % listForSurnames.Values.Length];
            }
            else
            {
                surname = GameManager.instance.localizationManager.activeDictionary.TryGetValue(GetGenderedLastNameId(householdMemberData.m_Household, male), out surname) ? surname : "???";
            }
            __result = Name.CustomName(adrMainSystem.DoNameFormat(name, surname));
            return false;
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
