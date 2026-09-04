using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.ModifierPipeline
{
    public readonly struct SellPriceCalculationContext
    {
        public int BasePrice { get; }
        public TradeItemType ItemType { get; }
        public SeasonPriceState Season { get; }
        public DistancePriceState Distance { get; }
        public TradeEventState Event { get; }
        public WeatherPriceState Weather { get; }

        public SellPriceCalculationContext(SellPriceInput input)
        {
            BasePrice = input.BasePrice;
            ItemType = input.ItemType;
            Season = input.Season;
            Distance = input.Distance;
            Event = input.Event;
            Weather = input.Weather;
        }
    }
}
