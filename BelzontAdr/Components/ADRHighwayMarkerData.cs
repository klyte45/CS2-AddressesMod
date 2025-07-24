using Belzont.Utils;
using Colossal.Serialization.Entities;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    public struct ADRHighwayMarkerData : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 2;

        public Colossal.Hash128 routeDataIndex;

        public RouteDirection routeDirection;
        public DisplayInformation displayInformation;
        public int numericCustomParam1;
        public int numericCustomParam2;

        public bool overrideMileage;
        public float newMileage;
        public bool reverseMileageCounting;

        public byte pylonCount;
        public float pylonSpacing;
        public PylonMaterial pylonMaterial;
        public PylonFormat pylonFormat;
        public float pylonHeight;

        private bool initialized;

        public bool Initialized
        {
            readonly get => initialized;
            set
            {
                if (value)
                {
                    initialized = value;
                }
            }
        }

        public enum RouteDirection
        {
            UNDEFINED,
            NORTH,
            NORTHEAST,
            EAST,
            SOUTHEAST,
            SOUTH,
            SOUTHWEST,
            WEST,
            NORTHWEST,
            INTERNAL,
            EXTERNAL
        }

        public enum DisplayInformation
        {
            ORIGINAL,
            CUSTOM_1,
            CUSTOM_2,
            CUSTOM_3,
            CUSTOM_4,
            CUSTOM_5,
            CUSTOM_6,
            CUSTOM_7
        }

        public enum PylonMaterial : byte
        {
            Metal,
            Wood
        }
        public enum PylonFormat : byte
        {
            Cylinder,
            Cubic
        }


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            var version = reader.CheckVersionK45(CURRENT_VERSION, typeof(ADRHighwayMarkerData));
            reader.Read(out routeDataIndex);
            reader.Read(out routeDirection);
            reader.Read(out displayInformation);
            reader.Read(out numericCustomParam1);
            reader.Read(out numericCustomParam2);
            reader.Read(out overrideMileage);
            reader.Read(out newMileage);
            reader.Read(out reverseMileageCounting);
            if (version >= 2)
            {
                reader.Read(out pylonCount);
                reader.Read(out pylonSpacing);
                reader.Read(out pylonMaterial);
                reader.Read(out pylonFormat);
                reader.Read(out pylonHeight);
            }
            else
            {
                pylonCount = 1;
                pylonSpacing = .5f;
                pylonMaterial = PylonMaterial.Metal;
                pylonFormat = PylonFormat.Cylinder;
                pylonHeight = 2f;
            }
            Initialized = true;
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(routeDataIndex);
            writer.Write(routeDirection);
            writer.Write(displayInformation);
            writer.Write(numericCustomParam1);
            writer.Write(numericCustomParam2);
            writer.Write(overrideMileage);
            writer.Write(newMileage);
            writer.Write(reverseMileageCounting);
            writer.Write(pylonCount);
            writer.Write(pylonSpacing);
            writer.Write(pylonMaterial);
            writer.Write(pylonFormat);
            writer.Write(pylonHeight);
        }

        public readonly float3 GetPylonScale() => new(1, pylonHeight, 1);

        public readonly float3 GetSignOffset() => new(-0.004f, pylonHeight - .14f, 0);

        private static readonly string DLL_PREFIX = typeof(ADRHighwayMarkerData).Assembly.GetName().Name + ":";
        public readonly string GetPylonMeshName() => DLL_PREFIX + (pylonFormat switch
        {
            PylonFormat.Cubic => "__BoxHolder",
            _ => "__CylinderHolder"
        });

        public readonly float3 GetNthPylonOffset(Dictionary<string, string> vars)
        {
            if (pylonCount <= 1) return default;
            if (vars.TryGetValue("pylonIndex", out string pylonIndexStr) && ushort.TryParse(pylonIndexStr, out var pylonIndex) && pylonIndex < pylonCount)
            {
                var totalSpacing = pylonSpacing * (pylonCount - 1);
                float offset = (pylonIndex / (pylonCount - 1f) * totalSpacing) - (totalSpacing / 2f);
                return new float3(0, 0, offset);
            }
            return default;
        }

    }
}
