using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRVehicleBuildingOrigin : IComponentData, ISerializable
    {
        private const uint CURRENT_VERSION = 0;
        public enum VehicleSourceKind : uint
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
            PublicTransport_Taxi,
            PublicTransport_Bus,

            Unknown = ~0u - 2,
            TransportCompany = ~0u - 1, //Game.Companies.TransportCompany && !(Game.Companies.CommercialCompany || Game.Companies.IndustrialCompany || Game.Buildings.CargoTransportStation)
            Other = ~0u,
        }

        private ushort categorySerialNumber;
        public VehicleSourceKind kind;
        public FixedString32Bytes customId;
        private uint internalSerialCounter;


        public uint GetNextInternalSerial() => internalSerialCounter++;
        public bool CategorySerialNumberSet { get; private set; }

        public readonly ushort CategorySerialNumber => categorySerialNumber;
        public readonly uint InternalSerialCounter => internalSerialCounter;

        public void DoRegisterCategorySerialNumber()
        {
            if (!CategorySerialNumberSet)
            {
                categorySerialNumber = AdrVehicleSystem.GetNextSerial(kind);
                CategorySerialNumberSet = true;
            }
        }

        public ADRVehicleBuildingOrigin(VehicleSourceKind kind)
        {
            this.kind = kind;
            categorySerialNumber = 0;
            internalSerialCounter = 0;
            CategorySerialNumberSet = false;
            customId = default;
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(categorySerialNumber);
            writer.Write(internalSerialCounter);
            writer.Write(kind);
            writer.Write(CategorySerialNumberSet);
            writer.Write(customId);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out categorySerialNumber);
            reader.Read(out internalSerialCounter);
            reader.Read(out kind);
            reader.Read(out bool categorySerialNumberSet);
            CategorySerialNumberSet = categorySerialNumberSet;
            reader.Read(out customId);
        }

    }
}
