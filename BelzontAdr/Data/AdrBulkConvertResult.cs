namespace BelzontAdr
{
    /// <summary>Result of citywide bulk ADR convert (NET-06).</summary>
    public class AdrBulkConvertResult
    {
        /// <summary>Vanilla Aggregates fully forked via <c>TryForkFromVanillaSeed</c>.</summary>
        public int AggregatesConverted;
        /// <summary>Partial same-kind groups created with <c>CreateAggregate(edges)</c>.</summary>
        public int AggregatesCreatedPartial;
        /// <summary>Vanilla Aggregates with no non-ADR remainder (already fully on ADR).</summary>
        public int SkippedAlreadyAdr;
        /// <summary>Vanilla Aggregates whose non-ADR remainder had no edge in the kind mask.</summary>
        public int SkippedNoMatch;
        /// <summary>Fork/create attempts that failed.</summary>
        public int Failed;
    }
}
