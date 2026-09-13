using Belzont.Utils;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Authored highway route / mileage / direction on an <see cref="ADRNameAggregate"/> entity.
    /// Defaults mean inactive (not a highway). No derived cache — consumers read these fields directly.
    /// <para>
    /// When <see cref="mileageOverrideActive"/> is true, <see cref="mileageOverrideKm"/> is the
    /// chain-start mileage baseline (former Aggregate cache <c>startDistanceOverrideKm</c>), not a
    /// per-marker curve bake.
    /// </para>
    /// </summary>
    public struct ADRNameAggregateHighwayData : IComponentData, IQueryTypeParameter, ISerializable
    {
        private const uint CURRENT_VERSION = 0;

        public Colossal.Hash128 highwayDataId;
        public RouteDirection routeDirection;
        public bool mileageOverrideActive;
        /// <summary>
        /// Chain-start mileage baseline (km) when <see cref="mileageOverrideActive"/> is true.
        /// Former Aggregate cache field <c>startDistanceOverrideKm</c>.
        /// </summary>
        public float mileageOverrideKm;
        public bool reverseCounting;

        /// <summary>
        /// Cardinal / internal-external route direction for WE and display.
        /// Ordinals match former marker <c>ADRHighwayMarkerData.RouteDirection</c>
        /// (<c>NONE</c> replaces historical <c>UNDEFINED</c> at 0).
        /// </summary>
        public enum RouteDirection
        {
            NONE,
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

        public static ADRNameAggregateHighwayData CreateDefault() => new()
        {
            highwayDataId = default,
            routeDirection = RouteDirection.NONE,
            mileageOverrideActive = false,
            mileageOverrideKm = 0f,
            reverseCounting = false
        };

        public readonly bool HasHighwayRoute => highwayDataId != default;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(highwayDataId);
            writer.Write(routeDirection);
            writer.Write(mileageOverrideActive);
            writer.Write(mileageOverrideKm);
            writer.Write(reverseCounting);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out highwayDataId);
            reader.Read(out routeDirection);
            reader.Read(out mileageOverrideActive);
            reader.Read(out mileageOverrideKm);
            reader.Read(out reverseCounting);
        }
    }
}
