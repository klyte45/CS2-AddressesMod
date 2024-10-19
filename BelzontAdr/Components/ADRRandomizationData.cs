using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRRandomizationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        private ushort m_seedIdentifier = 0;

        const uint CURRENT_VERSION = 0;

        public readonly ushort SeedIdentifier => m_seedIdentifier;

        public ADRRandomizationData() { }

        public void Redraw()
        {
            m_seedIdentifier = (ushort)AdrNamesetSystem.SeedGenerator.NextUInt(ushort.MinValue, ushort.MaxValue);
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
                throw new Exception("Invalid version of ADRDistrictData!");
            }
            reader.Read(out m_seedIdentifier);
        }

        internal void AddDelta(int delta)
        {
            m_seedIdentifier = (ushort)((delta + m_seedIdentifier) & 0xFFFF);
        }
    }
}
