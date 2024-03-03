using Colossal;
using Colossal.OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BelzontAdr
{
    public class AdrNameFile
    {

        internal readonly Guid Id;
        public string Name;
        public ImmutableList<string> Values { get; set; }
        internal Guid Checksum { get; private set; }
        public string IdString => Id.ToString();

        public AdrNameFile(string name, IEnumerable<string> values)
        {
            Name = name;
            Values = new ImmutableList<string>(values?.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new());
            RecalculateChecksum();
            Id = GuidUtils.Create(Checksum, name);
        }
        private void RecalculateChecksum()
        {
            Checksum = GuidUtils.Create(default, Values.SelectMany(x => Encoding.UTF8.GetBytes(x)).ToArray());
        }
        private AdrNameFile(Guid id, string name, IEnumerable<string> values)
        {
            Id = id;
            Name = name;
            Values = new ImmutableList<string>(values?.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new());
            RecalculateChecksum();
        }
        public static AdrNameFile FromXML(AdrNameFileXML xml)
        {
            return new AdrNameFile(xml.Id, xml.Name, xml.Values.Where(x => !x.IsNullOrWhitespace()));
        }
        public AdrNameFileXML ToXML()
        {
            return new AdrNameFileXML()
            {
                Id = Id,
                Name = Name,
                Values = Values.ToList()
            };
        }

        [XmlRoot("AdrNameFileXML")]
        public class AdrNameFileXML
        {
            public Guid Id { get; set; }
            public List<string> Values { get; set; }
            public string Name { get; set; }


        }
    }
}
