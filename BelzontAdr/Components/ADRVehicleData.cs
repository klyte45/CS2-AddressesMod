using Belzont.Utils;
using Colossal.Serialization.Entities;
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
            public string calculatedConvoyPrefix;
            public int manufactureMonthsFromEpoch;
            public uint ownerSerialNumber;

            private CohtmlSafe() { }

            public static CohtmlSafe From(ADRVehicleData data)
            {
                return new CohtmlSafe
                {
                    plateCategory = data.plateCategory,
                    cityOrigin = data.cityOrigin,
                    calculatedPlate = data.calculatedPlate.ToString(),
                    calculatedConvoyPrefix = data.calculatedConvoyPrefix.ToString(),
                    manufactureMonthsFromEpoch = data.manufactureMonthsFromEpoch,
                    serialNumber = data.serialNumber,
                    ownerSerialNumber = data.ownerSerialNumber
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


        private const uint CURRENT_VERSION = 2;
        private const string LEGACY_CONVOY_PREFIX = "<INVALID LEGACY VALUE>";
        public VehiclePlateCategory plateCategory;
        public Entity cityOrigin;
        public Entity serialOwnerSource;
        public Colossal.Hash128 checksumRule;
        public ulong serialNumber;
        public uint ownerSerialNumber;
        public FixedString32Bytes calculatedPlate;
        public FixedString32Bytes calculatedConvoyPrefix;
        public int manufactureMonthsFromEpoch;


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            var version = reader.CheckVersionK45(CURRENT_VERSION, GetType());
            if (version < 2)
            {
                reader.Read(out int pc);
                plateCategory = (VehiclePlateCategory)pc;
            }
            else
            {
                reader.Read(out plateCategory);
            }
            reader.Read(out cityOrigin);
            reader.Read(out checksumRule);
            reader.Read(out serialNumber);
            reader.Read(out calculatedPlate);
            reader.Read(out manufactureMonthsFromEpoch);
            if (version >= 1)
            {
                reader.Read(out calculatedConvoyPrefix);
            }
            else
            {
                calculatedConvoyPrefix = plateCategory == VehiclePlateCategory.Rail ? (FixedString32Bytes)LEGACY_CONVOY_PREFIX : calculatedPlate;
            }
            if (version >= 2)
            {
                reader.Read(out serialOwnerSource);
                reader.Read(out ownerSerialNumber);
            }
            else
            {
                ownerSerialNumber = 0;
                serialOwnerSource = Entity.Null;
            }
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(plateCategory);
            writer.Write(cityOrigin);
            writer.Write(checksumRule);
            writer.Write(serialNumber);
            writer.Write(calculatedPlate);
            writer.Write(manufactureMonthsFromEpoch);
            writer.Write(calculatedConvoyPrefix);
            writer.Write(serialOwnerSource);
            writer.Write(ownerSerialNumber);
        }

#if DEBUG
        public readonly string ToDebugString()
        {
            return $"CURRENT_VERSION = {CURRENT_VERSION} | " +
            $"plateCategory = {plateCategory} | " +
            $"cityOrigin = {cityOrigin} | " +
            $"checksumRule = {checksumRule} | " +
            $"serialNumber = {serialNumber} | " +
            $"calculatedPlate = {calculatedPlate} | " +
            $"manufactureMonthsFromEpoch = {manufactureMonthsFromEpoch} | ";
        }
#endif
    }
}
