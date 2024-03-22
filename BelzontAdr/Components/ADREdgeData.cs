using Colossal.Serialization.Entities;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    public struct ADREdgeData : IComponentData, IQueryTypeParameter, ISerializable
    {
        const uint CURRENT_VERSION = 0;
        public float width;
        public float2 heightRange;
        public float maxSpeed;
        public bool isHighway;
        public Entity fixedAggregation;
        public bool acceptsZoning;

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADREdgeData!");
            }
            reader.Read(out width);
            reader.Read(out heightRange);
            reader.Read(out maxSpeed);
            reader.Read(out isHighway);
            reader.Read(out fixedAggregation);

        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(width);
            writer.Write(heightRange);
            writer.Write(maxSpeed);
            writer.Write(isHighway);
            writer.Write(fixedAggregation);
        }
    }
}
