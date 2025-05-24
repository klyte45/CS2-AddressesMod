using Belzont.Utils;
using Colossal;
using Colossal.OdinSerializer.Utilities;
using Colossal.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BelzontAdr
{
    public class AdrNameFile : ISerializable
    {
        private const int CURRENT_VERSION = 0;

        internal Hash128 Id { get; private set; }
        public string Name;
        public ImmutableList<string> Values { get; set; }
        public ImmutableList<string> ValuesAlternative { get; set; }
        internal Hash128 Checksum { get; private set; }
        public string IdString => Id.ToString();
        public AdrNameFile() { }

        public AdrNameFile(string name, IEnumerable<string> values)
        {
            Name = name;
            var rawList = values?.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Split(";").Where(x => !x.IsNullOrWhitespace()).ToArray());
            Values = new ImmutableList<string>(rawList?.Select(x => x[0].Trim()).ToList() ?? new());
            ValuesAlternative = new ImmutableList<string>(rawList?.Select(x => (x.Length > 1 ? x[1] : x[0]).Trim()).ToList() ?? new());
            RecalculateChecksum();
            Id = GuidUtils.Create(Checksum.ToGuid(), name);
        }
        public AdrNameFile(string name, IEnumerable<string> values, IEnumerable<string> valuesAlternative)
        {
            Name = name;
            Values = new ImmutableList<string>(values.ToList());
            ValuesAlternative = new ImmutableList<string>(valuesAlternative.ToList());
            RecalculateChecksum();
            Id = GuidUtils.Create(Checksum.ToGuid(), name);
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

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(Values?.Count ?? 0);
            for (int i = 0; i < Values.Count; i++)
            {
                writer.Write(Values[i]);
            }
            writer.Write(ValuesAlternative?.Count ?? 0);
            for (int i = 0; i < Values.Count; i++)
            {
                writer.Write(ValuesAlternative[i]);
            }
            writer.Write(Checksum);
        }
        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out Hash128 id);
            Id = id;
            reader.Read(out Name);
            Values = ReadValuesList(reader);
            ValuesAlternative = ReadValuesList(reader);
            reader.Read(out Hash128 checksum);
            Checksum = checksum;
        }

        private ImmutableList<string> ReadValuesList<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out int valuesCount);
            var newValues = new List<string>();
            for (int i = 0; i < valuesCount; i++)
            {
                reader.Read(out string value);
                newValues.Add(value);
            }
            return new ImmutableList<string>(newValues);
        }
        #region Legacy
        [XmlRoot("AdrNameFileXML")]
        public class AdrNameFileXML
        {
            public Guid Id { get; set; }
            public List<string> Values { get; set; }
            public string Name { get; set; }
        }
        #endregion
    }
}
