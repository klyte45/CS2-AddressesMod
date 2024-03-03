using Belzont.Interfaces;
using Belzont.Serialization;
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
using Unity.Jobs;
using Unity.Mathematics;

namespace BelzontAdr
{
    public partial class AdrMainSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrMainSystem>
    {
        private Action<string, object[]> m_eventCaller;
        private AdrCitywideSettings currentCitySettings = new();
        private AdrDistrictsSystem districtsSystem;
        private Queue<Action> actionsToGoOnUpdate;
        private AdrNamesetSystem namesetSystem;

        private static string DefaultRoadPrefixFilename = Path.Combine(AddressesCs2Mod.ModSettingsRootFolder, "DefaultRoadPrefixRules.xml");

        World IBelzontSerializableSingleton<AdrMainSystem>.World => World;

        public void SetupCallBinder(Action<string, Delegate> doBindLink)
        {
            doBindLink("main.getCurrentCitywideSettings", () => CurrentCitySettings);
            doBindLink("main.setSurnameAtFirst", (bool x) => { CurrentCitySettings.SurnameAtFirst = x; NotifyChanges(); });
            doBindLink("main.setCitizenMaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenMaleNameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenFemaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenFemaleNameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenSurnameOverridesStr", (string x) => { CurrentCitySettings.CitizenSurnameOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setCitizenDogOverridesStr", (string x) => { CurrentCitySettings.CitizenDogOverridesStr = x; NotifyChanges(); });
            doBindLink("main.setDefaultRoadNameOverridesStr", (string x) => { CurrentCitySettings.DefaultRoadNameOverridesStr = x; MarkRoadsDirty(); NotifyChanges(); });
            doBindLink("main.setAdrRoadPrefixSetting", (AdrRoadPrefixSetting x) => { CurrentCitySettings.RoadPrefixSetting = x; MarkRoadsDirty(); NotifyChanges(); });
            doBindLink("main.setDefaultDistrictNameOverridesStr", (string x) => { CurrentCitySettings.DefaultDistrictNameOverridesStr = x; NotifyChanges(); MarkDistrictsDirty(); districtsSystem.OnDistrictChanged(); });
            doBindLink("main.setRoadNameAsNameStation", (bool x) => { CurrentCitySettings.RoadNameAsNameStation = x; NotifyChanges(); });
            doBindLink("main.setRoadNameAsNameCargoStation", (bool x) => { CurrentCitySettings.RoadNameAsNameCargoStation = x; NotifyChanges(); });
            doBindLink("main.setDistrictNameAsNameStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameStation = x; NotifyChanges(); });
            doBindLink("main.setDistrictNameAsNameCargoStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameCargoStation = x; NotifyChanges(); });
            doBindLink("main.exploreToRoadPrefixRulesFileDefault", () => RemoteProcess.OpenFolder(DefaultRoadPrefixFilename));
            doBindLink("main.saveRoadPrefixRulesFileDefault", () => File.WriteAllText(DefaultRoadPrefixFilename, XmlUtils.DefaultXmlSerialize(CurrentCitySettings.RoadPrefixSetting)));
            doBindLink("main.loadRoadPrefixRulesFileDefault", () =>
            {
                if (File.Exists(DefaultRoadPrefixFilename))
                {
                    try
                    {
                        CurrentCitySettings.RoadPrefixSetting = XmlUtils.DefaultXmlDeserialize<AdrRoadPrefixSetting>(File.ReadAllText(DefaultRoadPrefixFilename));
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

        public AdrCitywideSettings CurrentCitySettings
        {
            get => currentCitySettings; private set
            {
                currentCitySettings = value;
                NotifyChanges();
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
        #endregion


        #region Citizen & Pet
        internal bool TryGetNameList(bool male, out AdrNameFile names) => namesetSystem.GetForGuid(male ? CurrentCitySettings.CitizenMaleNameOverrides : CurrentCitySettings.CitizenFemaleNameOverrides, out names);

        internal bool TryGetSurnameList(out AdrNameFile listForSurnames) => namesetSystem.GetForGuid(CurrentCitySettings.CitizenSurnameOverrides, out listForSurnames);

        internal string DoNameFormat(string name, string surname) => CurrentCitySettings.SurnameAtFirst ? $"{surname} {name}" : $"{name} {surname}";
        internal bool TryGetDogsList(out AdrNameFile listForDogs) => namesetSystem.GetForGuid(CurrentCitySettings.CitizenDogOverrides, out listForDogs);
        internal bool TryGetRoadNamesList(Entity district, out AdrNameFile roadsNamesList)
            => (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict) && adrDistrict.m_roadsNamesId != Guid.Empty && namesetSystem.GetForGuid(adrDistrict.m_roadsNamesId, out roadsNamesList))
                || namesetSystem.GetForGuid(CurrentCitySettings.DefaultRoadNameOverrides, out roadsNamesList);

        internal bool TryGetDistrictNamesList(out AdrNameFile districtNamesList) => namesetSystem.GetForGuid(CurrentCitySettings.DefaultDistrictNameOverrides, out districtNamesList);
        #endregion


        #region Serialization

        public const int CURRENT_VERSION = 0;


        private void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of XTMRouteAutoColorSystem!");
            }
            reader.Read(out string autoColorData);
            try
            {
                CurrentCitySettings = XmlUtils.DefaultXmlDeserialize<AdrCitywideSettings>(new string(autoColorData)) ?? new();
            }
            catch (Exception e)
            {
                LogUtils.DoWarnLog($"AdrMainSystem: Could not load settings from the City!!!\n{e}");
            }
        }

        private void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(XmlUtils.DefaultXmlSerialize(CurrentCitySettings));
        }

        void IBelzontSerializableSingleton<AdrMainSystem>.Serialize<TWriter>(TWriter writer) => Serialize(writer);
        void IBelzontSerializableSingleton<AdrMainSystem>.Deserialize<TReader>(TReader reader) => Deserialize(reader);
        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            CurrentCitySettings = new();
            return default;
        }
        #endregion
    }
}
