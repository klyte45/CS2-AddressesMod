using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRDistrictData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public Colossal.Hash128 m_roadsNamesId;

        const uint CURRENT_VERSION = 1;
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_roadsNamesId);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADRDistrictData!");
            }
            if (version == 0)
            {
                reader.Read(out string _);
                return;
            }
            reader.Read(out m_roadsNamesId);
        }
    }
}
