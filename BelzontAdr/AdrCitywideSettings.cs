using System;
using System.Xml.Serialization;

namespace BelzontAdr
{
    [XmlRoot("AdrCitywideSettings")]
    public class AdrCitywideSettings
    {
        internal Guid CitizenMaleNameOverrides { get; set; }
        internal Guid CitizenFemaleNameOverrides { get; set; }
        internal Guid CitizenSurnameOverrides { get; set; }
        [XmlAttribute("SurnameAtFirst")]
        public bool SurnameAtFirst { get; set; }

        [XmlAttribute("CitizenMaleNameOverrides")]
        public string CitizenMaleNameOverridesStr { get => CitizenMaleNameOverrides.ToString(); set => CitizenMaleNameOverrides = new Guid(value); }
        [XmlAttribute("CitizenFemaleNameOverrides")]
        public string CitizenFemaleNameOverridesStr { get => CitizenFemaleNameOverrides.ToString(); set => CitizenFemaleNameOverrides = new Guid(value); }
        [XmlAttribute("CitizenSurnameOverrides")]
        public string CitizenSurnameOverridesStr { get => CitizenSurnameOverrides.ToString(); set => CitizenSurnameOverrides = new Guid(value); }
    }
}
