using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRRandomizationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public ushort m_seedIdentifier;

        const uint CURRENT_VERSION = 0;

        public ADRRandomizationData()
        {
            m_seedIdentifier = (ushort)new Random().Next(ushort.MinValue, ushort.MaxValue);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_seedIdentifier);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADRDistrictData!");
            }
            reader.Read(out m_seedIdentifier);
        }
    }
}
