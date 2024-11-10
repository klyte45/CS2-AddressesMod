using Belzont.Utils;
using Colossal.Serialization.Entities;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRVehicleData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public Entity cityOrigin;
        public Colossal.Hash128 checksumRule;
        public ulong serialNumber;
        public FixedString32Bytes calculatedPlate;
        public long manifactureTicks;


        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out cityOrigin);
            reader.Read(out checksumRule);
            reader.Read(out serialNumber);
            reader.Read(out calculatedPlate); ;
            reader.Read(out manifactureTicks);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(cityOrigin);
            writer.Write(checksumRule);
            writer.Write(serialNumber);
            writer.Write(calculatedPlate);
            writer.Write(manifactureTicks);
        }
    }
}
