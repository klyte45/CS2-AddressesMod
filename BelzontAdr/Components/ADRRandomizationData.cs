using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRRandomizationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        private uint m_seedIdentifier = 0;

        const uint CURRENT_VERSION = 2;

        public readonly uint SeedIdentifier => m_seedIdentifier;

        public ADRRandomizationData() { }

        public void Redraw()
        {
            m_seedIdentifier = AdrNamesetSystem.Instance.GetNextSeed();
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(SeedIdentifier);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            if (version == 0)
            {
                reader.Read(out ushort identifier);
                m_seedIdentifier = identifier;
            }
            else
            {
                reader.Read(out m_seedIdentifier);
                if (version == 1 && m_seedIdentifier == ~0u)
                {
                    Redraw();
                }
            }
        }

        internal void AddDelta(int delta)
        {
            unchecked
            {
                m_seedIdentifier = (uint)(delta + m_seedIdentifier);
            }
        }
    }
}
