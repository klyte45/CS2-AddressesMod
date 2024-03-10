using Colossal.Serialization.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRAggregationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public enum HighwayClass : byte
        {
            None = 0,
            AccessRamp,
            AccessRoad,
            Priority_VeryLow,
            Priority_Low,
            Priority_Medium,
            Priority_High,
            Priority_VeryHigh,
            Custom_00 = 0xC0,
            Custom_01 = 0xC1,
            Custom_02 = 0xC2,
            Custom_03 = 0xC3,
            Custom_04 = 0xC4,
            Custom_05 = 0xC5,
            Custom_06 = 0xC6,
            Custom_07 = 0xC7,
            Custom_08 = 0xC8,
            Custom_09 = 0xC9,
            Custom_10 = 0xCa,
            Custom_11 = 0xCb,
            Custom_12 = 0xCc,
            Custom_13 = 0xCd,
            Custom_14 = 0xCe,
            Custom_15 = 0xCf,
        }
        public enum HighwayDirection : byte
        {
            Inverse = 255,
            None = 0,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
        }

        public enum ElevationType : sbyte
        {
            Underground = -1,
            GroundOrHighway,
            BridgeOrDam,
        }

        const uint CURRENT_VERSION = 0;

        public HighwayClass highwayClass;
        public HighwayDirection highwayDirection;
        public ushort roadNumberReference;
        public Entity parentAggregation;
        public ushort priority;
        public uint lenghtMeters;
        public ElevationType aggregationType;

        public readonly void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write((byte)highwayClass);
            writer.Write((byte)highwayDirection);
            writer.Write(roadNumberReference);
            writer.Write(parentAggregation);
            writer.Write(priority);
            writer.Write(lenghtMeters);
            writer.Write((sbyte)aggregationType);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of AdrAggregationType!");
            }

            reader.Read(out byte hClass);
            highwayClass = (HighwayClass)hClass;
            reader.Read(out byte hDir);
            highwayDirection = (HighwayDirection)hDir;
            reader.Read(out roadNumberReference);
            reader.Read(out parentAggregation);
            reader.Read(out priority);
            reader.Read(out lenghtMeters);
            reader.Read(out sbyte aggType);
            aggregationType = (ElevationType)aggType;
        }
    }
}
