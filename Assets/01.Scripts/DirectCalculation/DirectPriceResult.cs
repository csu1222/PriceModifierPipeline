namespace PriceModifierPipeline.DirectCalculation
{
    public readonly struct DirectPriceResult
    {
        public int FinalPrice { get; }
        public decimal SeasonMultiplier { get; }
        public decimal DistanceMultiplier { get; }
        public decimal EventMultiplier { get; }
        public decimal WeatherMultiplier { get; }
        public bool WeatherApplied { get; }
        public bool SeasonApplied { get; }
        public bool DistanceApplied { get; }
        public bool EventApplied { get; }

        public DirectPriceResult(int finalPrice, decimal seasonMultiplier, decimal distanceMultiplier,
            decimal eventMultiplier, bool seasonApplied, bool distanceApplied, bool eventApplied,
            decimal weatherMultiplier = 1m, bool weatherApplied = true)
        {
            FinalPrice = finalPrice;
            SeasonMultiplier = seasonMultiplier;
            DistanceMultiplier = distanceMultiplier;
            EventMultiplier = eventMultiplier;
            SeasonApplied = seasonApplied;
            DistanceApplied = distanceApplied;
            EventApplied = eventApplied;
            WeatherMultiplier = weatherMultiplier;
            WeatherApplied = weatherApplied;
        }
    }
}
