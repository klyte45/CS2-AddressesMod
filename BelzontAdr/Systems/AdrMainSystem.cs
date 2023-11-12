using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Entities;
using Unity.Jobs;

namespace BelzontAdr
{

    public partial class AdrMainSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrMainSystem>
    {
        private Action<string, object[]> m_eventCaller;
        private AdrCitywideSettings currentCitySettings = new();
        private AdrDistrictsSystem districtsSystem;
        private Queue<Action> actionsToGo;

        private static string DefaultRoadPrefixFilename = Path.Combine(AddressesCs2Mod.ModSettingsRootFolder, "DefaultRoadPrefixRules.xml");

        World IBelzontSerializableSingleton<AdrMainSystem>.World => World;

        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("main.listSimpleNames", () => AdrNameFilesManager.Instance.SimpleNamesDict.Values.ToArray());
            eventCaller("main.reloadSimpleNames", () =>
            {
                AdrNameFilesManager.Instance.ReloadNameFiles();
                OnChangedDistrictNameGenerationRules();
                OnChangedRoadNameGenerationRules();
                return AdrNameFilesManager.Instance.SimpleNamesDict.Values.ToArray();
            });
            eventCaller("main.goToSimpleNamesFolder", () => { RemoteProcess.OpenFolder(AdrNameFilesManager.SimpleNameFolder); });
            eventCaller("main.getCurrentCitywideSettings", () => CurrentCitySettings);
            eventCaller("main.setSurnameAtFirst", (bool x) => { CurrentCitySettings.SurnameAtFirst = x; NotifyChanges(); });
            eventCaller("main.setCitizenMaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenMaleNameOverridesStr = x; NotifyChanges(); });
            eventCaller("main.setCitizenFemaleNameOverridesStr", (string x) => { CurrentCitySettings.CitizenFemaleNameOverridesStr = x; NotifyChanges(); });
            eventCaller("main.setCitizenSurnameOverridesStr", (string x) => { CurrentCitySettings.CitizenSurnameOverridesStr = x; NotifyChanges(); });
            eventCaller("main.setCitizenDogOverridesStr", (string x) => { CurrentCitySettings.CitizenDogOverridesStr = x; NotifyChanges(); });
            eventCaller("main.setDefaultRoadNameOverridesStr", (string x) => { CurrentCitySettings.DefaultRoadNameOverridesStr = x; OnChangedRoadNameGenerationRules(); NotifyChanges(); });
            eventCaller("main.setAdrRoadPrefixSetting", (AdrRoadPrefixSetting x) => { CurrentCitySettings.RoadPrefixSetting = x; OnChangedRoadNameGenerationRules(); NotifyChanges(); });
            eventCaller("main.setDefaultDistrictNameOverridesStr", (string x) => { CurrentCitySettings.DefaultDistrictNameOverridesStr = x; NotifyChanges(); OnChangedDistrictNameGenerationRules(); districtsSystem.OnDistrictChanged(); });
            eventCaller("main.setRoadNameAsNameStation", (bool x) => { CurrentCitySettings.RoadNameAsNameStation = x; NotifyChanges(); });
            eventCaller("main.setRoadNameAsNameCargoStation", (bool x) => { CurrentCitySettings.RoadNameAsNameCargoStation = x; NotifyChanges(); });
            eventCaller("main.setDistrictNameAsNameStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameStation = x; NotifyChanges(); });
            eventCaller("main.setDistrictNameAsNameCargoStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameCargoStation = x; NotifyChanges(); });
            eventCaller("main.exploreToRoadPrefixRulesFileDefault", () => RemoteProcess.OpenFolder(DefaultRoadPrefixFilename));
            eventCaller("main.saveRoadPrefixRulesFileDefault", () => File.WriteAllText(DefaultRoadPrefixFilename, XmlUtils.DefaultXmlSerialize(CurrentCitySettings.RoadPrefixSetting)));
            eventCaller("main.loadRoadPrefixRulesFileDefault", () =>
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
            while (actionsToGo.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            districtsSystem = World.GetOrCreateSystemManaged<AdrDistrictsSystem>();
            actionsToGo = new Queue<Action>();
        }

        internal void EnqueueToRunOnUpdate(Action a)
        {
            actionsToGo.Enqueue(a);
        }


        internal void OnChangedRoadNameGenerationRules() => typeof(AggregateMeshSystem).GetMethod("OnDictionaryChanged", ReflectionUtils.allFlags)?.Invoke(World.GetExistingSystemManaged<AggregateMeshSystem>(), new object[0]);
        private void OnChangedDistrictNameGenerationRules() => typeof(AreaBufferSystem).GetMethod("OnDictionaryChanged", ReflectionUtils.allFlags)?.Invoke(World.GetExistingSystemManaged<AreaBufferSystem>(), new object[0]);
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

        #region Citizen & Pet
        internal bool TryGetNameList(bool male, out AdrNameFile names) => AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(male ? CurrentCitySettings.CitizenMaleNameOverrides : CurrentCitySettings.CitizenFemaleNameOverrides, out names);

        internal bool TryGetSurnameList(out AdrNameFile listForSurnames) => AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(CurrentCitySettings.CitizenSurnameOverrides, out listForSurnames);

        internal string DoNameFormat(string name, string surname) => CurrentCitySettings.SurnameAtFirst ? $"{surname} {name}" : $"{name} {surname}";
        internal bool TryGetDogsList(out AdrNameFile listForDogs) => AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(CurrentCitySettings.CitizenDogOverrides, out listForDogs);
        internal bool TryGetRoadNamesList(Entity district, out AdrNameFile roadsNamesList)
            => (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict) && adrDistrict.m_roadsNamesId != Guid.Empty && AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(adrDistrict.m_roadsNamesId, out roadsNamesList))
                || AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(CurrentCitySettings.DefaultRoadNameOverrides, out roadsNamesList);

        internal bool TryGetDistrictNamesList(out AdrNameFile districtNamesList) => AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(CurrentCitySettings.DefaultDistrictNameOverrides, out districtNamesList);
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
