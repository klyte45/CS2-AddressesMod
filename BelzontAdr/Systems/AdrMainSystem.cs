using Belzont.Interfaces;
using Belzont.Utils;
using Colossal;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Areas;
using Game.Net;
using Game.Rendering;
using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    public partial class AdrMainSystem : GameSystemBase, IBelzontBindable, IDefaultSerializable
    {
        private Action<string, object[]> m_eventCaller;
        private AdrDistrictsSystem districtsSystem;
        private Queue<Action> actionsToGoOnUpdate;
        private AdrNamesetSystem namesetSystem;

        public AdrCitywideSettings CurrentCitySettings { get; private set; } = new();

        private static string DefaultRoadPrefixFilename = Path.Combine(AddressesCs2Mod.ModSettingsRootFolder, "DefaultRoadPrefixRules.xml");

        public void SetupCallBinder(Action<string, Delegate> doBindLink)
        {
            doBindLink("main.getCurrentCitywideSettings", () => CurrentCitySettings);
            doBindLink("main.setMaxSurnames", (int x) => { CurrentCitySettings.MaximumGeneratedSurnames = x; NotifyChanges(); return CurrentCitySettings.MaximumGeneratedSurnames; });
            doBindLink("main.setMaxGivenNames", (int x) => { CurrentCitySettings.MaximumGeneratedGivenNames = x; NotifyChanges(); return CurrentCitySettings.MaximumGeneratedGivenNames; });
            doBindLink("main.setSurnameAtFirst", (bool x) => { CurrentCitySettings.surnameAtFirst = x; NotifyChanges(); });
            doBindLink("main.setCitizenMaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenMaleNameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenFemaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenFemaleNameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenSurnameOverridesStr", (string x) => { CurrentCitySettings.CitizenSurnameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenDogOverridesStr", (string x) => { CurrentCitySettings.CitizenDogOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setDefaultRoadNameOverridesStr", (string x) => { CurrentCitySettings.DefaultRoadNameOverridesStr = x; MarkRoadsDirty(); NotifyChanges(); });
            doBindLink("main.setAdrRoadPrefixSetting", (AdrRoadPrefixSetting x) => { CurrentCitySettings.roadPrefixSetting = x; MarkRoadsDirty(); NotifyChanges(); });
            doBindLink("main.setDefaultDistrictNameOverridesStr", (string x) => { CurrentCitySettings.DefaultDistrictNameOverridesStr = x; NotifyChanges(); MarkDistrictsDirty(); districtsSystem.OnDistrictChanged(); });
            doBindLink("main.setRoadNameAsNameStation", (bool x) => { CurrentCitySettings.roadNameAsNameStation = x; NotifyChanges(); });
            doBindLink("main.setRoadNameAsNameCargoStation", (bool x) => { CurrentCitySettings.roadNameAsNameCargoStation = x; NotifyChanges(); });
            doBindLink("main.exploreToRoadPrefixRulesFileDefault", () => RemoteProcess.OpenFolder(DefaultRoadPrefixFilename));
            doBindLink("main.saveRoadPrefixRulesFileDefault", () => File.WriteAllText(DefaultRoadPrefixFilename, XmlUtils.DefaultXmlSerialize(CurrentCitySettings.roadPrefixSetting)));
            doBindLink("main.loadRoadPrefixRulesFileDefault", () =>
            {
                if (File.Exists(DefaultRoadPrefixFilename))
                {
                    try
                    {
                        CurrentCitySettings.roadPrefixSetting = XmlUtils.DefaultXmlDeserialize<AdrRoadPrefixSetting>(File.ReadAllText(DefaultRoadPrefixFilename));
                        NotifyChanges();
                        return 1;
                    }
                    catch (Exception e)
                    {
                        LogUtils.DoWarnLog($"Error loading defaults road rules file: {e}");
                        return -2;
                    }
                }
                return -1;
            });
            doBindLink("main.atob", (string x) => Encoding.UTF8.GetString(Convert.FromBase64String(x)));
            doBindLink("main.isCityOrEditorLoaded", () => GameManager.instance.gameMode.IsGameOrEditor());
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }

        protected override void OnUpdate()
        {
            if (!GameManager.instance.isLoading && !GameManager.instance.isGameLoading)
            {
                while (actionsToGoOnUpdate.TryDequeue(out var action))
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"Running action {action}");
                    action.Invoke();
                }
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            districtsSystem = World.GetOrCreateSystemManaged<AdrDistrictsSystem>();
            actionsToGoOnUpdate = new Queue<Action>();
            namesetSystem = World.GetOrCreateSystemManaged<AdrNamesetSystem>();

        }

        private void EnqueueToRunOnUpdate(Action a)
        {
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"Enqueue action {a}");
            actionsToGoOnUpdate.Enqueue(a);
            if (BasicIMod.VerboseMode) LogUtils.DoVerboseLog($"actions to go after add: {actionsToGoOnUpdate?.Count}!");
        }


        private void ResetRoadsCache()
        {
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"Run action typeof(AggregateMeshSystem).GetMethod(\"OnDictionaryChanged\",...)");
            typeof(AggregateMeshSystem).GetMethod("OnDictionaryChanged", ReflectionUtils.allFlags).Invoke(World.GetExistingSystemManaged<AggregateMeshSystem>(), new object[0]);
            isDirtyRoads = false;
        }

        private void ResetDistrictsCache()
        {
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"Run action typeof(AreaBufferSystem).GetMethod(\"OnDictionaryChanged\", ...)");
            typeof(AreaBufferSystem).GetMethod("OnDictionaryChanged", ReflectionUtils.allFlags).Invoke(World.GetExistingSystemManaged<AreaBufferSystem>(), new object[0]);
            isDirtyDistricts = false;
        }

        private void OnChangedRoadNameGenerationRules() => EnqueueToRunOnUpdate(ResetRoadsCache);

        private void OnChangedDistrictNameGenerationRules() => EnqueueToRunOnUpdate(ResetDistrictsCache);

        private bool isDirtyRoads;
        private bool isDirtyDistricts;

        internal void MarkRoadsDirty()
        {
            if (!isDirtyRoads)
            {
                isDirtyRoads = true;
                OnChangedRoadNameGenerationRules();
            }
        }
        internal void MarkDistrictsDirty()
        {
            if (!isDirtyDistricts)
            {
                isDirtyDistricts = true;
                OnChangedDistrictNameGenerationRules();
            }
        }


        private void NotifyChanges()
        {
            m_eventCaller?.Invoke("main.onCurrentCitywideSettingsLoaded", new object[0]);
        }

        #region Roads
        public bool FindReferenceRoad(Entity entity, out DynamicBuffer<AggregateElement> elements, out Entity refRoad)
        {
            refRoad = default;
            if (!EntityManager.TryGetBuffer(entity, true, out elements)) return false;
            if (!EntityManager.TryGetComponent<EdgeGeometry>(elements[0].m_Edge, out var geom0)) return false;
            if (!EntityManager.TryGetComponent<EdgeGeometry>(elements[^1].m_Edge, out var geomLast)) return false;
            refRoad = math.length(geom0.m_Bounds.min) < math.length(geomLast.m_Bounds.min) ? elements[0].m_Edge : elements[^1].m_Edge;
            return true;
        }
        public bool GetRoadNameList(Entity refRoad, out AdrNameFile roadsNamesList)
        {
            Entity refDistrict = EntityManager.TryGetComponent<BorderDistrict>(refRoad, out var refDistrictBorders) ? refDistrictBorders.m_Left == default ? refDistrictBorders.m_Right : refDistrictBorders.m_Left : default;
            return TryGetRoadNamesList(refDistrict, out roadsNamesList);
        }

        public float3 GetZeroMarkerPosition()
        {
            return default;
        }
        #endregion


        #region Citizen & Pet
        internal bool TryGetNameList(bool male, out AdrNameFile names) => namesetSystem.GetForGuid(male ? CurrentCitySettings.CitizenMaleNameOverrides : CurrentCitySettings.CitizenFemaleNameOverrides, out names);

        internal bool TryGetSurnameList(out AdrNameFile listForSurnames) => namesetSystem.GetForGuid(CurrentCitySettings.CitizenSurnameOverrides, out listForSurnames);

        internal string DoNameFormat(string name, string surname) => CurrentCitySettings.surnameAtFirst ? $"{surname} {name}" : $"{name} {surname}";
        internal bool TryGetDogsList(out AdrNameFile listForDogs) => namesetSystem.GetForGuid(CurrentCitySettings.CitizenDogOverrides, out listForDogs);
        internal bool TryGetRoadNamesList(Entity district, out AdrNameFile roadsNamesList)
            => (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict) && adrDistrict.m_roadsNamesId != Guid.Empty && namesetSystem.GetForGuid(adrDistrict.m_roadsNamesId, out roadsNamesList))
                || namesetSystem.GetForGuid(CurrentCitySettings.DefaultRoadNameOverrides, out roadsNamesList);

        internal bool TryGetDistrictNamesList(out AdrNameFile districtNamesList) => namesetSystem.GetForGuid(CurrentCitySettings.DefaultDistrictNameOverrides, out districtNamesList);
        #endregion


        #region Serialization

        public const int CURRENT_VERSION = 1;


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            if (version == 0)
            {

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
#pragma warning disable CS0612 // O tipo ou membro é obsoleto
                reader.Read(out string autoColorData);
                try
                {
                    var settings = XmlUtils.DefaultXmlDeserialize<AdrCitywideSettingsLegacy>(new string(autoColorData)) ?? new();
                    CurrentCitySettings = AdrCitywideSettings.FromLegacy(settings);
                }
                catch (Exception e)
                {
                    LogUtils.DoWarnLog($"AdrMainSystem: Could not load settings from the City!!!\n{e}");
                }
#pragma warning restore CS0612 // O tipo ou membro é obsoleto
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
            }
            else
            {
                CurrentCitySettings = new AdrCitywideSettings();
                reader.Read(CurrentCitySettings);
            }

        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(CurrentCitySettings);
        }

        public void SetDefaults(Context context)
        {
            CurrentCitySettings = new();
        }
        #endregion
    }
}
