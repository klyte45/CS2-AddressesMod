using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Per-segment house-number range card payload for Road Grouping VUIO.
    /// Stable sort key is AggregateElement buffer index.
    /// </summary>
    public struct AdrRoadGroupingSegmentCard
    {
        public Entity edge;
        public int sortKey;
        public int numberLow;
        public int numberHigh;
        public float midX;
        public float midY;
        public float midZ;
        public float viewportX;
        public float viewportY;
        public bool visible;
    }
}
