using Colossal.Serialization.Entities;
using System;
using Unity.Entities;
using static BelzontAdr.ADRVehicleData;

namespace BelzontAdr
{
    public struct ADRVehicleSourceData :IComponentData, ISerializable
    {
        private const uint CURRENT_VERSION = 0;
        public enum VehicleSourceKind : ushort
        {
            Police,//Game.Buildings.PoliceStation
            Hospital, //Game.Buildings.Hospital
            Deathcare,//Game.Buildings.DeathcareFacility
            FireResponse, //Game.Buildings.FireStation
            Garbage, //Game.Buildings.GarbageFacility
            PublicTransport,//Game.Buildings.TransportDepot 
            CargoTransport,//Game.Buildings.CargoTransportStation && Game.Companies.TransportCompany 
            Maintenance, //Game.Buildings.MaintenanceDepot
            Post, //Game.Buildings.PostFacility
            CommercialCompany, //Game.Companies.CommercialCompany && Game.Companies.TransportCompany 
            IndustrialCompany, //Game.Companies.IndustrialCompany && Game.Companies.TransportCompany 

            TransportCompany = 0xFFFe, //Game.Companies.TransportCompany && !(Game.Companies.CommercialCompany || Game.Companies.IndustrialCompany || Game.Buildings.CargoTransportStation)
            Other = 0xffff                                                  
        }

        public ulong serialNumber;
        public VehicleSourceKind kind;
        public uint internalSerialCounter;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(serialNumber);
            writer.Write(internalSerialCounter);
            writer.Write((ushort) kind);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out serialNumber);
            reader.Read(out internalSerialCounter);
            reader.Read(out ushort kind);
            this.kind = (VehicleSourceKind) kind;
        }
    }
}
