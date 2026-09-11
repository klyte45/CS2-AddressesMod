using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRNameAggregated : IComponentData, IQueryTypeParameter, ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        public Entity m_Aggregate;
        public bool preventPropagation;
        public bool numberOverrideActive;
        public int numberOverride;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_Aggregate);
            writer.Write(preventPropagation);
            writer.Write(numberOverrideActive);
            writer.Write(numberOverride);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out m_Aggregate);
            reader.Read(out preventPropagation);
            reader.Read(out numberOverrideActive);
            reader.Read(out numberOverride);
        }
    }
}
