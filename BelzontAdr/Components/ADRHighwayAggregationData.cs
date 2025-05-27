using Belzont.Utils;
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
            reader.CheckVersionK45(CURRENT_VERSION, GetType());

            reader.Read(out highwayDataId);
        }
    }
    public struct ADRHighwayAggregationDataDirtyHwId : IComponentData { }
    public struct ADRHighwayAggregationCacheData : IComponentData, ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        public float startDistanceOverrideKm;
        public bool reverseCounting;
        public readonly void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(startDistanceOverrideKm);
            writer.Write(reverseCounting);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());

            reader.Read(out startDistanceOverrideKm);
            reader.Read(out reverseCounting);
        }
    }
}
