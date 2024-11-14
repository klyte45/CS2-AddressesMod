using Belzont.Utils;
using Colossal.Serialization.Entities;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRVehicleData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public class CohtmlSafe
        {
            public VehiclePlateCategory plateCategory;
            public Entity cityOrigin;
            public ulong serialNumber;
            public string calculatedPlate;
            public int manufactureMonthsFromEpoch;

            private CohtmlSafe() { }

            public static CohtmlSafe From(ADRVehicleData data)
            {
                return new CohtmlSafe
                {
                    plateCategory = data.plateCategory,
                    cityOrigin = data.cityOrigin,
                    calculatedPlate = data.calculatedPlate.ToString(),
                    manufactureMonthsFromEpoch = data.manufactureMonthsFromEpoch,
                    serialNumber = data.serialNumber
                };
            }
        }

        public enum VehiclePlateCategory
        {
            Road,
            Air,
            Water,
            Rail
        }

        private const uint CURRENT_VERSION = 0;

        public VehiclePlateCategory plateCategory;
        public Entity cityOrigin;
        public Colossal.Hash128 checksumRule;
        public ulong serialNumber;
        public FixedString32Bytes calculatedPlate;
        public int manufactureMonthsFromEpoch;


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out int pc);
            plateCategory = (VehiclePlateCategory)pc;
            reader.Read(out cityOrigin);
            reader.Read(out checksumRule);
            reader.Read(out serialNumber);
            reader.Read(out calculatedPlate); ;
            reader.Read(out manufactureMonthsFromEpoch);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write((int)plateCategory);
            writer.Write(cityOrigin);
            writer.Write(checksumRule);
            writer.Write(serialNumber);
            writer.Write(calculatedPlate);
            writer.Write(manufactureMonthsFromEpoch);
        }
    }
}
