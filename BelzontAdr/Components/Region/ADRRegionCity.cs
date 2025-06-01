using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BelzontAdr
{
    public struct ADRRegionCity : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 0;

        public FixedString64Bytes name;
        public ushort azimuthAngle;// 0x0000 to 0xFFFF, where 0x0000 is 0 degrees and 0xFFFF + 1 is 360 degrees
        public ushort azimuthWidthRight;
        public ushort azimuthWidthLeft;
        public Color mapColor;

        public readonly bool IsInside(ushort angleAzimuth)
        {
            unchecked
            {
                var min = azimuthAngle - azimuthWidthLeft;
                var max = azimuthAngle + azimuthWidthRight;
                return min > max ? min >= angleAzimuth || angleAzimuth >= max : min <= angleAzimuth && angleAzimuth <= max;
            }
        }

        public static float ToDegreeAngle(ushort azimuthValue) => 360f / ushort.MaxValue * azimuthValue;
        public static ushort ToAzimuthValue(float angle) => (ushort)(angle * 1f / 360f % 1 * ushort.MaxValue);

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(name.ToString());
            writer.Write(azimuthAngle);
            writer.Write(azimuthWidthRight);
            writer.Write(azimuthWidthLeft);
            writer.Write(mapColor);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out name);
            reader.Read(out azimuthAngle);
            reader.Read(out azimuthWidthRight);
            reader.Read(out azimuthWidthLeft);
            reader.Read(out mapColor);
        }
    }

    public struct ADRRegionWaterCity : IComponentData, IQueryTypeParameter, IEmptySerializable { }
    public struct ADRRegionLandCity : IComponentData, IQueryTypeParameter, IEmptySerializable { }
    public struct ADRRegionAirCity : IComponentData, IQueryTypeParameter, IEmptySerializable { }

    public struct ADRRegionCityReference : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 0;
        public Entity cityEntity;

        public readonly ADRRegionCity GetCityData(EntityManager entityManager) => entityManager.GetComponentData<ADRRegionCity>(cityEntity);
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(cityEntity);
        }
        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out cityEntity);
        }
    }
}
