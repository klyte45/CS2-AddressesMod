using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.Serialization.Entities;
using Game.Rendering;
using System;
using System.Linq;
using Unity.Entities;
using Unity.Jobs;

namespace BelzontAdr
{
    public class AdrMainSystem : SystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrMainSystem>
    {
        private Action<string, object[]> m_eventCaller;
        private AdrCitywideSettings currentCitySettings = new();

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
            eventCaller("main.setDefaultDistrictNameOverridesStr", (string x) => { CurrentCitySettings.DefaultDistrictNameOverridesStr = x; NotifyChanges(); OnChangedDistrictNameGenerationRules(); });
            eventCaller("main.setRoadNameAsNameStation", (bool x) => { CurrentCitySettings.RoadNameAsNameStation = x; NotifyChanges(); });
            eventCaller("main.setRoadNameAsNameCargoStation", (bool x) => { CurrentCitySettings.RoadNameAsNameCargoStation = x; NotifyChanges(); });
            eventCaller("main.setDistrictNameAsNameStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameStation = x; NotifyChanges(); });
            eventCaller("main.setDistrictNameAsNameCargoStation", (bool x) => { CurrentCitySettings.DistrictNameAsNameCargoStation = x; NotifyChanges(); });
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
        }

        private void OnChangedRoadNameGenerationRules() => typeof(AggregateMeshSystem).GetMethod("OnDictionaryChanged", ReflectionUtils.allFlags)?.Invoke(World.GetExistingSystemManaged<AggregateMeshSystem>(), new object[0]);
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
        internal bool TryGetRoadNamesList(Entity district, out AdrNameFile roadsNamesList) => AdrNameFilesManager.Instance.SimpleNamesDict.TryGetValue(CurrentCitySettings.DefaultRoadNameOverrides, out roadsNamesList);
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
