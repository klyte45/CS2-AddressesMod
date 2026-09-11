using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Number-range / mileage card payload for Road Grouping VUIO.
    /// <see cref="label"/> is the display text (house range or locale-formatted "km 0,5" / "mi 0.5").
    /// </summary>
    public struct AdrRoadGroupingSegmentCard
    {
        public Entity edge;
        public int sortKey;
        public int numberLow;
        public int numberHigh;
        public string label;
        public float midX;
        public float midY;
        public float midZ;
        public float viewportX;
        public float viewportY;
        public bool visible;
    }
}
