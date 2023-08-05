using Colossal.Serialization.Entities;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public struct ADRLocalizationData : IComponentData, IQueryTypeParameter, ISerializable
    {
        public ushort m_seedReference;

        const uint CURRENT_VERSION = 0;
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(m_seedReference);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADRLocalizationData!");
            }
            reader.Read(out m_seedReference);
        }
    }

    public struct ADRDistrictRelativeData : IComponentData, IQueryTypeParameter, ISerializable
    {
        const uint CURRENT_VERSION = 0;
        public NativeHashMap<int, Entity> passengerTransportMainStations;
        public NativeHashMap<int, Entity> cargoTransportMainStations;

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of ADRDistrictRelativeData!");
            }
            passengerTransportMainStations.Dispose();
            cargoTransportMainStations.Dispose();

            reader.Read(out int countPassengers);
            passengerTransportMainStations.Capacity = countPassengers;
            for (int i = 0; i < countPassengers; i++)
            {
                reader.Read(out int key);
                reader.Read(out Entity value);
                passengerTransportMainStations[key] = value;
            }
            reader.Read(out int countCargo);
            cargoTransportMainStations.Capacity = countCargo;
            for (int i = 0; i < countCargo; i++)
            {
                reader.Read(out int key);
                reader.Read(out Entity value);
                cargoTransportMainStations[key] = value;
            }
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(passengerTransportMainStations.Count);
            foreach (var entry in passengerTransportMainStations.GetKeyArray(Allocator.Temp))
            {
                writer.Write(entry);
                writer.Write(passengerTransportMainStations[entry]);
            }
            writer.Write(cargoTransportMainStations.Count);
            foreach (var entry in cargoTransportMainStations.GetKeyArray(Allocator.Temp))
            {
                writer.Write(entry);
                writer.Write(cargoTransportMainStations[entry]);
            }
        }
    }
}
