
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BelzontAdr
{
    [XmlRoot("AdrCitywideSettings")]
    public class AdrCitywideSettings
    {
        internal Guid CitizenMaleNameOverrides { get; private set; }
        internal Guid CitizenFemaleNameOverrides { get; private set; }
        internal Guid CitizenSurnameOverrides { get; private set; }
        internal Guid CitizenDogOverrides { get; private set; }
        internal Guid DefaultRoadNameOverrides { get; private set; }
        internal Guid DefaultDistrictNameOverrides { get; private set; }
        [XmlAttribute("RoadNameAsNameStation")]
        public bool RoadNameAsNameStation { get; set; }
        [XmlAttribute("RoadNameAsNameCargoStation")]
        public bool RoadNameAsNameCargoStation { get; set; }
        [XmlAttribute("SurnameAtFirst")]
        public bool SurnameAtFirst { get; set; }

        [XmlAttribute("CitizenMaleNameOverrides")]
        public string CitizenMaleNameOverridesStr { get => CitizenMaleNameOverrides.ToString(); set => CitizenMaleNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlAttribute("CitizenFemaleNameOverrides")]
        public string CitizenFemaleNameOverridesStr { get => CitizenFemaleNameOverrides.ToString(); set => CitizenFemaleNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlAttribute("CitizenSurnameOverrides")]
        public string CitizenSurnameOverridesStr { get => CitizenSurnameOverrides.ToString(); set => CitizenSurnameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlAttribute("CitizenDogOverrides")]
        public string CitizenDogOverridesStr { get => CitizenDogOverrides.ToString(); set => CitizenDogOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlAttribute("DefaultRoadNameOverrides")]
        public string DefaultRoadNameOverridesStr { get => DefaultRoadNameOverrides.ToString(); set => DefaultRoadNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlAttribute("DefaultDistrictNameOverrides")]
        public string DefaultDistrictNameOverridesStr { get => DefaultDistrictNameOverrides.ToString(); set => DefaultDistrictNameOverrides = Guid.TryParse(value ?? "", out var guid) ? guid : default; }
        [XmlElement("RoadPrefix")]
        public AdrRoadPrefixSetting RoadPrefixSetting { get; set; } = new AdrRoadPrefixSetting();
        [XmlAttribute("DistrictNameAsNameCargoStation")]
        public bool DistrictNameAsNameCargoStation { get; set; }
        [XmlAttribute("DistrictNameAsNameStation")]
        public bool DistrictNameAsNameStation { get; set; }
    }

    [XmlRoot("RoadPrefixSetting")]
    public class AdrRoadPrefixSetting
    {
        public AdrRoadPrefixRule FallbackRule { get; set; } = new() { FormatPattern = "{name}" };
        public List<AdrRoadPrefixRule> AdditionalRules { get; set; } = new();

        public AdrRoadPrefixRule GetFirstApplicable(RoadData roadData, bool fullBridge) => AdditionalRules.FirstOrDefault(x => x.IsApplicable(roadData, fullBridge)) ?? FallbackRule;
    }

    [XmlRoot("RoadPrefixRule")]
    public class AdrRoadPrefixRule
    {
        internal float MinSpeed { get; set; }
        internal float MaxSpeed { get; set; }
        internal RoadFlags RequiredFlags { get; set; }
        internal RoadFlags ForbiddenFlags { get; set; }
        internal bool? FullBridgeRequire { get; set; } = null;
        [XmlText]
        public string FormatPattern { get; set; }

        [XmlAttribute("MinSpeedKmh")]
        public float MinSpeedKmh { get => MinSpeed * 1.8f; set => MinSpeed = value / 1.8f; }

        [XmlAttribute("MaxSpeedKmh")]
        public float MaxSpeedKmh { get => MaxSpeed * 1.8f; set => MaxSpeed = value / 1.8f; }

        [XmlAttribute("RequiredFlags")]
        public int RequiredFlagsInt { get => (int)RequiredFlags; set => RequiredFlags = (RoadFlags)value; }

        [XmlAttribute("ForbiddenFlags")]
        public int ForbiddenFlagsInt { get => (int)ForbiddenFlags; set => ForbiddenFlags = (RoadFlags)value; }

        [XmlAttribute("FullBridge")]
        public int FullBridge
        {
            get => FullBridgeRequire.HasValue ? FullBridgeRequire.Value ? 1 : -1 : 0;
            set => FullBridgeRequire = value switch
            {
                >= 1 => true,
                <= -1 => false,
                _ => null
            };
        }

        public bool IsApplicable(RoadData roadData, bool fullBridge)
            => roadData.m_SpeedLimit >= MinSpeed
            && roadData.m_SpeedLimit <= MaxSpeed
            && (RequiredFlags & roadData.m_Flags) == RequiredFlags
            && (ForbiddenFlags & roadData.m_Flags) == 0
            && (!FullBridgeRequire.HasValue || FullBridgeRequire.Value == fullBridge);
    }
}
