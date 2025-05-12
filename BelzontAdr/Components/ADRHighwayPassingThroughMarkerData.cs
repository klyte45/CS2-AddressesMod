using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRHighwayPassingThroughMarkerData : IComponentData, ISerializable
    {
        const uint CURRENT_VERSION = 0;

        public FixedString32Bytes prefix;
        public FixedString32Bytes suffix;

        public RouteDirection routeDirection;
        public DisplayInformation displayInformation;
        public int numericCustomParam1;
        public int numericCustomParam2;

        public bool overrideMileage;
        public float newMileage;
        public bool reverseMileageCounting;



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
            ROUTE_SHIELD,
            MILEAGE_KM,
            EXIT_NUMBER,
            CUSTOM_1,
            CUSTOM_2,
            CUSTOM_3,
            CUSTOM_4,
            CUSTOM_5
        }


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, typeof(ADRHighwayPassingThroughMarkerData));
            reader.Read(out prefix);
            reader.Read(out suffix);
            reader.Read(out routeDirection);
            reader.Read(out displayInformation);
            reader.Read(out numericCustomParam1);
            reader.Read(out numericCustomParam2);
            reader.Read(out overrideMileage);
            reader.Read(out newMileage);
            reader.Read(out reverseMileageCounting);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(prefix);
            writer.Write(suffix);
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
