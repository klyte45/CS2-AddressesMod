using System;
using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    [InternalBufferCapacity(0)]
    public struct ADRNameAggregateElement : IBufferElementData, IEquatable<ADRNameAggregateElement>, ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        public Entity m_Edge;
        public bool preventPropagation;
        public bool numberOverrideActive;
        public int numberOverride;

        public ADRNameAggregateElement(Entity edge)
        {
            m_Edge = edge;
            preventPropagation = false;
            numberOverrideActive = false;
            numberOverride = 0;
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_Edge);
            writer.Write(preventPropagation);
            writer.Write(numberOverrideActive);
            writer.Write(numberOverride);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out m_Edge);
            reader.Read(out preventPropagation);
            reader.Read(out numberOverrideActive);
            reader.Read(out numberOverride);
        }

        public readonly bool Equals(ADRNameAggregateElement other) => m_Edge.Equals(other.m_Edge);

        public override readonly int GetHashCode() => m_Edge.GetHashCode();
    }
}
