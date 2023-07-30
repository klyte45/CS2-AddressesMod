using System;
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
    }
}
