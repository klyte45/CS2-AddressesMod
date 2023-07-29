using Belzont.Interfaces;
using System.Xml.Serialization;

namespace BelzontAdr
{
    public class AdrModData : IBasicModData
    {
        [XmlAttribute]
        public bool DebugMode { get; set; }

    }
}
