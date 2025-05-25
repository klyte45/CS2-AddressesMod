using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{

    public struct ADRHighwayAggregationData : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 0;

        public Colossal.Hash128 highwayDataId;

        public readonly void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(highwayDataId);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of AdrAggregationType!");
            }

            reader.Read(out highwayDataId);
        }
    }
    public struct ADRHighwayAggregationDataDirtyHwId : IComponentData { }
}
