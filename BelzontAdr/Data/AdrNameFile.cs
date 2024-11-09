using Belzont.Utils;
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

        internal readonly Hash128 Id;
        public string Name;
        public ImmutableList<string> Values { get; set; }
        public ImmutableList<string> ValuesAlternative { get; set; }
        internal Guid Checksum { get; private set; }
        public string IdString => Id.ToString();

        public AdrNameFile(string name, IEnumerable<string> values)
        {
            Name = name;
            var rawList = values?.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Split(";"));
            Values = new ImmutableList<string>(rawList?.Select(x => x[0].Trim()).ToList() ?? new());
            ValuesAlternative = new ImmutableList<string>(rawList?.Select(x => (x.Length > 1 ? x[1] : x[0]).Trim()).ToList() ?? new());
            RecalculateChecksum();
            Id = GuidUtils.Create(Checksum, name);
        }
        private void RecalculateChecksum()
        {
            Checksum = GuidUtils.Create(default, Values.SelectMany(x => Encoding.UTF8.GetBytes(x)).ToArray());
        }
        private AdrNameFile(Guid id, string name, IEnumerable<string> values) : this(name, values)
        {
            Id = id;
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
                Id = Id.ToGuid(),
                Name = Name,
                Values = Values.Select((x, i) => ValuesAlternative[i] == x ? x : $"{x};{ValuesAlternative[i]}").ToList()
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
