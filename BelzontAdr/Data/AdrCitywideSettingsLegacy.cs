
using Colossal.Randomization;
using Colossal.Serialization.Entities;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BelzontAdr
{
    public class AdrCitywideSettings : ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        private int maximumGeneratedGivenNames = 1;
        private int maximumGeneratedSurnames = 1;
        private Colossal.Hash128 citizenMaleNameOverrides;
        private Colossal.Hash128 citizenFemaleNameOverrides;
        private Colossal.Hash128 citizenSurnameOverrides;
        private Colossal.Hash128 citizenDogOverrides;
        private Colossal.Hash128 defaultRoadNameOverrides;
        private Colossal.Hash128 defaultDistrictNameOverrides;
        public bool roadNameAsNameStation;
        public bool roadNameAsNameCargoStation;
        public bool surnameAtFirst;
        public AdrRoadPrefixSetting roadPrefixSetting = new();
        public bool districtNameAsNameCargoStation;
        public bool districtNameAsNameStation;

        #region Indirect setters & public getters

        internal Colossal.Hash128 CitizenMaleNameOverrides => citizenMaleNameOverrides;
        internal Colossal.Hash128 CitizenFemaleNameOverrides => citizenFemaleNameOverrides;
        internal Colossal.Hash128 CitizenSurnameOverrides => citizenSurnameOverrides;
        internal Colossal.Hash128 CitizenDogOverrides => citizenDogOverrides;
        internal Colossal.Hash128 DefaultRoadNameOverrides => defaultRoadNameOverrides;
        internal Colossal.Hash128 DefaultDistrictNameOverrides => defaultDistrictNameOverrides;
        public int MaximumGeneratedSurnames { get => maximumGeneratedSurnames; set => maximumGeneratedSurnames = Math.Clamp(value, 1, 5); }
        public int MaximumGeneratedGivenNames { get => maximumGeneratedGivenNames; set => maximumGeneratedGivenNames = Math.Clamp(value, 1, 5); }
        public string CitizenMaleNameOverridesStr { get => CitizenMaleNameOverrides.ToString(); set => citizenMaleNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        public string CitizenFemaleNameOverridesStr { get => CitizenFemaleNameOverrides.ToString(); set => citizenFemaleNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        public string CitizenSurnameOverridesStr { get => CitizenSurnameOverrides.ToString(); set => citizenSurnameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        public string CitizenDogOverridesStr { get => CitizenDogOverrides.ToString(); set => citizenDogOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        public string DefaultRoadNameOverridesStr { get => DefaultRoadNameOverrides.ToString(); set => defaultRoadNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        public string DefaultDistrictNameOverridesStr { get => DefaultDistrictNameOverrides.ToString(); set => defaultDistrictNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }

        #endregion

        public long CityNameSeeds { get; set; } = new System.Random().NextLong();

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(maximumGeneratedGivenNames);
            writer.Write(maximumGeneratedSurnames);
            writer.Write(citizenMaleNameOverrides);
            writer.Write(citizenFemaleNameOverrides);
            writer.Write(citizenSurnameOverrides);
            writer.Write(citizenDogOverrides);
            writer.Write(defaultRoadNameOverrides);
            writer.Write(defaultDistrictNameOverrides);
            writer.Write(roadNameAsNameStation);
            writer.Write(roadNameAsNameCargoStation);
            writer.Write(surnameAtFirst);
            writer.Write(roadPrefixSetting);
            writer.Write(districtNameAsNameCargoStation);
            writer.Write(districtNameAsNameStation);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out maximumGeneratedGivenNames);
            reader.Read(out maximumGeneratedSurnames);
            reader.Read(out citizenMaleNameOverrides);
            reader.Read(out citizenFemaleNameOverrides);
            reader.Read(out citizenSurnameOverrides);
            reader.Read(out citizenDogOverrides);
            reader.Read(out defaultRoadNameOverrides);
            reader.Read(out defaultDistrictNameOverrides);
            reader.Read(out roadNameAsNameStation);
            reader.Read(out roadNameAsNameCargoStation);
            reader.Read(out surnameAtFirst);
            roadPrefixSetting = new AdrRoadPrefixSetting();
            reader.Read(roadPrefixSetting);
            reader.Read(out districtNameAsNameCargoStation);
            reader.Read(out districtNameAsNameStation);
        }
        [Obsolete]
        internal static AdrCitywideSettings FromLegacy(AdrCitywideSettingsLegacy settings)
        {
            return new AdrCitywideSettings
            {
                maximumGeneratedGivenNames = settings.MaximumGeneratedGivenNames,
                maximumGeneratedSurnames = settings.MaximumGeneratedSurnames,
                citizenMaleNameOverrides = settings.CitizenMaleNameOverrides,
                citizenFemaleNameOverrides = settings.CitizenFemaleNameOverrides,
                citizenSurnameOverrides = settings.CitizenSurnameOverrides,
                citizenDogOverrides = settings.CitizenDogOverrides,
                defaultRoadNameOverrides = settings.DefaultRoadNameOverrides,
                defaultDistrictNameOverrides = settings.DefaultDistrictNameOverrides,
                roadNameAsNameStation = settings.RoadNameAsNameStation,
                roadNameAsNameCargoStation = settings.RoadNameAsNameCargoStation,
                surnameAtFirst = settings.SurnameAtFirst,
                roadPrefixSetting = AdrRoadPrefixSetting.FromLegacy(settings.RoadPrefixSetting),
                districtNameAsNameCargoStation = settings.DistrictNameAsNameCargoStation,
                districtNameAsNameStation = settings.DistrictNameAsNameStation
            };
        }
    }

    public class AdrRoadPrefixSetting : ISerializable
    {
        private const uint CURRENT_VERSION = 0;
        public AdrRoadPrefixRule FallbackRule { get; set; } = new() { formatPattern = "{name}" };
        public List<AdrRoadPrefixRule> AdditionalRules { get; set; } = new();
        public AdrRoadPrefixRule GetFirstApplicable(RoadData roadData, bool fullBridge) => AdditionalRules.FirstOrDefault(x => x.IsApplicable(roadData, fullBridge)) ?? FallbackRule;

        [Obsolete]
        internal static AdrRoadPrefixSetting FromLegacy(AdrRoadPrefixSettingLegacy roadPrefixSetting)
        {
            return new AdrRoadPrefixSetting
            {
                FallbackRule = AdrRoadPrefixRule.FromLegacy(roadPrefixSetting.FallbackRule),
                AdditionalRules = roadPrefixSetting.AdditionalRules.Select(x => AdrRoadPrefixRule.FromLegacy(x)).ToList()
            };
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(FallbackRule);
            var count = AdditionalRules.Count;
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                writer.Write(AdditionalRules[i]);
            }

        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            FallbackRule = new AdrRoadPrefixRule();
            reader.Read(FallbackRule);
            reader.Read(out int count);
            AdditionalRules ??= new List<AdrRoadPrefixRule>();
            AdditionalRules.Clear();
            for (int i = 0; i < count; i++)
            {
                var nextRule = new AdrRoadPrefixRule();
                reader.Read(nextRule);
                AdditionalRules.Add(nextRule);
            }
        }
    }

    public class AdrRoadPrefixRule : ISerializable
    {
        private const uint CURRENT_VERSION = 0;
        public enum FullBridgeRequirement
        {
            False = -1,
            Unset = 0,
            True = 1
        }

        internal float minSpeed;
        internal float maxSpeed;
        internal RoadFlags requiredFlags;
        internal RoadFlags forbiddenFlags;
        internal FullBridgeRequirement fullBridgeRequire;
        internal string formatPattern;

        public float MinSpeedKmh { get => minSpeed * 1.8f; set => minSpeed = value / 1.8f; }

        public float MaxSpeedKmh { get => maxSpeed * 1.8f; set => maxSpeed = value / 1.8f; }

        public int RequiredFlagsInt { get => (int)requiredFlags; set => requiredFlags = (RoadFlags)value; }

        public int ForbiddenFlagsInt { get => (int)forbiddenFlags; set => forbiddenFlags = (RoadFlags)value; }
        public string FormatPattern { get => formatPattern; set => formatPattern = value.Contains("{name}") ? value : formatPattern; }

        public int FullBridge
        {
            get => (int)fullBridgeRequire;
            set => fullBridgeRequire = value switch
            {
                >= 1 => FullBridgeRequirement.True,
                <= -1 => FullBridgeRequirement.False,
                _ => FullBridgeRequirement.Unset
            };
        }

        public bool IsApplicable(RoadData roadData, bool fullBridge)
            => roadData.m_SpeedLimit >= minSpeed
            && roadData.m_SpeedLimit <= maxSpeed
            && (requiredFlags & roadData.m_Flags) == requiredFlags
            && (forbiddenFlags & roadData.m_Flags) == 0
            && (fullBridgeRequire == FullBridgeRequirement.Unset || fullBridgeRequire == (fullBridge ? FullBridgeRequirement.True : FullBridgeRequirement.False));

        [Obsolete]
        internal static AdrRoadPrefixRule FromLegacy(AdrRoadPrefixRuleLegacy fallbackRule)
        {
            return new AdrRoadPrefixRule
            {
                minSpeed = fallbackRule.MinSpeed,
                maxSpeed = fallbackRule.MaxSpeed,
                requiredFlags = fallbackRule.RequiredFlags,
                forbiddenFlags = fallbackRule.ForbiddenFlags,
                fullBridgeRequire = fallbackRule.FullBridgeRequire switch
                {
                    true => FullBridgeRequirement.True,
                    false => FullBridgeRequirement.False,
                    _ => FullBridgeRequirement.Unset
                },
                formatPattern = fallbackRule.FormatPattern,
            };
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(minSpeed);
            writer.Write(maxSpeed);
            writer.Write((int)requiredFlags);
            writer.Write((int)forbiddenFlags);
            writer.Write((int)fullBridgeRequire);
            writer.Write(formatPattern);


        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out minSpeed);
            reader.Read(out maxSpeed);
            reader.Read(out int requiredFlags);
            reader.Read(out int forbiddenFlags);
            reader.Read(out int fullBridgeRequire);
            reader.Read(out formatPattern);
            this.requiredFlags = (RoadFlags)requiredFlags;
            this.forbiddenFlags = (RoadFlags)forbiddenFlags;
            this.fullBridgeRequire = (FullBridgeRequirement)fullBridgeRequire;
        }
    }
}
