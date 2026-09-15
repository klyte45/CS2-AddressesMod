namespace BelzontAdr
{
    /// <summary>Result of citywide bulk ADR revert / destroy (inverse of NET-06 convert).</summary>
    public class AdrBulkRevertResult
    {
        /// <summary>ADR name-aggregates destroyed via <c>DestroyAggregate</c>.</summary>
        public int AggregatesRemoved;
        /// <summary>ADR aggregates whose kind was not in the mask (or unresolved).</summary>
        public int SkippedNoMatch;
        /// <summary>Road (or other) aggregates with highway route kept because Highways was unchecked.</summary>
        public int SkippedHighway;
        /// <summary>Destroy attempts that failed.</summary>
        public int Failed;
    }
}
