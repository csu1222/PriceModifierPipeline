namespace PriceModifierPipeline.ModifierPipeline
{
    public readonly struct PipelinePriceResult
    {
        public int FinalPrice { get; }
        public ResolvedSellPriceModifiers Modifiers { get; }
        public decimal SeasonMultiplier => Modifiers.SeasonMultiplier;
        public decimal DistanceMultiplier => Modifiers.DistanceMultiplier;
        public decimal EventMultiplier => Modifiers.EventMultiplier;
        public bool SeasonApplied => Modifiers.SeasonApplied;
        public bool DistanceApplied => Modifiers.DistanceApplied;
        public bool EventApplied => Modifiers.EventApplied;

        public PipelinePriceResult(int finalPrice, ResolvedSellPriceModifiers modifiers)
        {
            FinalPrice = finalPrice;
            Modifiers = modifiers;
        }
    }
}
