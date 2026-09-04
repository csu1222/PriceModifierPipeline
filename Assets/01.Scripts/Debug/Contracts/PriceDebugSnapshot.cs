using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.Debugging
{
    public sealed class PriceDebugSnapshot
    {
        public string ArchitectureName { get; }
        public int BasePrice { get; }
        public TradeItemType ItemType { get; }
        public SeasonPriceState Season { get; }
        public DistancePriceState Distance { get; }
        public TradeEventState Event { get; }
        public WeatherPriceState Weather { get; }
        public decimal WeatherMultiplier { get; }
        public bool WeatherApplied { get; }
        public decimal SeasonMultiplier { get; }
        public decimal DistanceMultiplier { get; }
        public decimal EventMultiplier { get; }
        public bool SeasonApplied { get; }
        public bool DistanceApplied { get; }
        public bool EventApplied { get; }
        public int PreviewPrice { get; }
        public int CommitPrice { get; }
        public bool HasPreview { get; }
        public bool HasCommit { get; }
        public string LastAction { get; }
        public PriceConsistencyState ConsistencyState { get; }

        public PriceDebugSnapshot(string architectureName, SellPriceInput input,
            decimal seasonMultiplier, decimal distanceMultiplier, decimal eventMultiplier,
            bool seasonApplied, bool distanceApplied, bool eventApplied,
            int previewPrice, int commitPrice, bool hasPreview, bool hasCommit, string lastAction,
            decimal weatherMultiplier = 1m, bool weatherApplied = false)
        {
            ArchitectureName = architectureName;
            BasePrice = input.BasePrice;
            ItemType = input.ItemType;
            Season = input.Season;
            Distance = input.Distance;
            Event = input.Event;
            Weather = input.Weather;
            WeatherMultiplier = weatherMultiplier;
            WeatherApplied = weatherApplied;
            SeasonMultiplier = seasonMultiplier;
            DistanceMultiplier = distanceMultiplier;
            EventMultiplier = eventMultiplier;
            SeasonApplied = seasonApplied;
            DistanceApplied = distanceApplied;
            EventApplied = eventApplied;
            PreviewPrice = previewPrice;
            CommitPrice = commitPrice;
            HasPreview = hasPreview;
            HasCommit = hasCommit;
            LastAction = lastAction;
            ConsistencyState = !hasPreview || !hasCommit ? PriceConsistencyState.NotEvaluated
                : previewPrice == commitPrice ? PriceConsistencyState.Pass : PriceConsistencyState.Fail;
        }
    }
}
