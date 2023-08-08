using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRDistrictData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public Guid m_roadsNamesId;

        const uint CURRENT_VERSION = 0;
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_roadsNamesId.ToString());
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADRDistrictData!");
            }
            reader.Read(out string roadsGuid);
            Guid.TryParse(roadsGuid, out m_roadsNamesId);
        }
    }
}
