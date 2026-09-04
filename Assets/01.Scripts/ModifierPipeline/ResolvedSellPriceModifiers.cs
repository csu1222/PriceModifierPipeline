namespace PriceModifierPipeline.ModifierPipeline
{
    public readonly struct ResolvedSellPriceModifiers
    {
        public decimal SeasonMultiplier { get; }
        public decimal DistanceMultiplier { get; }
        public decimal EventMultiplier { get; }
        public bool SeasonApplied { get; }
        public bool DistanceApplied { get; }
        public bool EventApplied { get; }

        public ResolvedSellPriceModifiers(decimal seasonMultiplier, decimal distanceMultiplier,
            decimal eventMultiplier, bool seasonApplied, bool distanceApplied, bool eventApplied)
        {
            SeasonMultiplier = seasonMultiplier;
            DistanceMultiplier = distanceMultiplier;
            EventMultiplier = eventMultiplier;
            SeasonApplied = seasonApplied;
            DistanceApplied = distanceApplied;
            EventApplied = eventApplied;
        }
    }
}
