using Belzont.Utils;
using Colossal.Serialization.Entities;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    public struct ADRHighwayMarkerData : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 3;

        public DisplayInformation displayInformation;
        public int numericCustomParam1;
        public int numericCustomParam2;

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
            if (version < 3)
            {
                // Discard pre-v3 route/mileage identity; keep visual display fields.
                reader.Read(out Colossal.Hash128 _); // was routeDataIndex
                reader.Read(out string _); // was RouteDirection (enum as string)
                reader.Read(out displayInformation);
                reader.Read(out numericCustomParam1);
                reader.Read(out numericCustomParam2);
                reader.Read(out bool _); // was overrideMileage
                reader.Read(out float _); // was newMileage
                reader.Read(out bool _); // was reverseMileageCounting
            }
            else
            {
                reader.Read(out displayInformation);
                reader.Read(out numericCustomParam1);
                reader.Read(out numericCustomParam2);
            }
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
            writer.Write(displayInformation);
            writer.Write(numericCustomParam1);
            writer.Write(numericCustomParam2);
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
