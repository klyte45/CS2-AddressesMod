using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRLocalizationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public ushort m_seedReference;

        const uint CURRENT_VERSION = 0;
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_seedReference);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of XTMPaletteSettedUpInformation!");
            }
            reader.Read(out m_seedReference);
        }
    }
}
