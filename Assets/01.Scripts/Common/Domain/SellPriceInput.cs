using System;

namespace PriceModifierPipeline.Common
{
    public readonly struct SellPriceInput
    {
        public int BasePrice { get; }
        public TradeItemType ItemType { get; }
        public SeasonPriceState Season { get; }
        public DistancePriceState Distance { get; }
        public TradeEventState Event { get; }
        public WeatherPriceState Weather { get; }

        public SellPriceInput(int basePrice, TradeItemType itemType,
            SeasonPriceState season, DistancePriceState distance, TradeEventState tradeEvent,
            WeatherPriceState weather = WeatherPriceState.Clear)
        {
            if (basePrice < 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "기본 가격은 음수일 수 없습니다.");
            if (!Enum.IsDefined(typeof(TradeItemType), itemType))
                throw new ArgumentOutOfRangeException(nameof(itemType));
            if (!Enum.IsDefined(typeof(SeasonPriceState), season))
                throw new ArgumentOutOfRangeException(nameof(season));
            if (!Enum.IsDefined(typeof(DistancePriceState), distance))
                throw new ArgumentOutOfRangeException(nameof(distance));
            if (!Enum.IsDefined(typeof(TradeEventState), tradeEvent))
                throw new ArgumentOutOfRangeException(nameof(tradeEvent));

            if (!Enum.IsDefined(typeof(WeatherPriceState), weather))
                throw new ArgumentOutOfRangeException(nameof(weather));

            Weather = weather;
            BasePrice = basePrice;
            ItemType = itemType;
            Season = season;
            Distance = distance;
            Event = tradeEvent;
        }
    }
}
