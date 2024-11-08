
using Colossal.Randomization;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BelzontAdr
{
    [XmlRoot("AdrCitywideSettings")]
    [Obsolete("Legacy pre-0.2. Now using native serialize.")]
    public class AdrCitywideSettingsLegacy
    {
        internal int maximumGeneratedGivenNames = 1;
        internal int maximumGeneratedSurnames = 1;

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
        [XmlAttribute("MaximumGeneratedSurnames")]
        public int MaximumGeneratedSurnames { get => maximumGeneratedSurnames; set => maximumGeneratedSurnames = Math.Clamp(value, 1, 5); }

        [XmlAttribute("MaximumGeneratedGivenNames")]
        public int MaximumGeneratedGivenNames { get => maximumGeneratedGivenNames; set => maximumGeneratedGivenNames = Math.Clamp(value, 1, 5); }

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

        public long CityNameSeeds { get; set; } = new System.Random().NextLong();
    }
}
