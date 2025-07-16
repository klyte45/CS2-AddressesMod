using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRHighwayMarkerData : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 1;

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

        public enum PylonMaterial
        {
            Metal,
            Wood
        }


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, typeof(ADRHighwayMarkerData));
            reader.Read(out routeDataIndex);
            reader.Read(out routeDirection);
            reader.Read(out displayInformation);
            reader.Read(out numericCustomParam1);
            reader.Read(out numericCustomParam2);
            reader.Read(out overrideMileage);
            reader.Read(out newMileage);
            reader.Read(out reverseMileageCounting);
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
        }
    }
}
