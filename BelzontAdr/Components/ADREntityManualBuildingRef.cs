using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADREntityManualBuildingRef : IComponentData, IQueryTypeParameter, ISerializable
    {
        const uint CURRENT_VERSION = 0;
        public Entity m_refNamedEntity;

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADREntityManualBuildingRef!");
            }
            reader.Read(out m_refNamedEntity);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_refNamedEntity);
        }

    }
}
