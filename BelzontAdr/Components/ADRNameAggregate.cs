using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Host tag for an ADR name-aggregate. Carries persisted <see cref="AdrNetKind"/>
    /// classified from prefab <c>*Data</c> at create/fork (backfilled from first member when None).
    /// </summary>
    public struct ADRNameAggregate : IComponentData, IQueryTypeParameter, ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        public AdrNetKind m_Kind;

        public static ADRNameAggregate CreateDefault() => new()
        {
            m_Kind = AdrNetKind.None
        };

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_Kind);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out m_Kind);
        }
    }
}
