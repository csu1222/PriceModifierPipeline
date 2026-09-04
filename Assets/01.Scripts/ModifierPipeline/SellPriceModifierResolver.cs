using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.ModifierPipeline
{
    public sealed class SellPriceModifierResolver
    {
        public ResolvedSellPriceModifiers Resolve(SellPriceCalculationContext context)
        {
            decimal season = SellPriceRules.NormalSeasonMultiplier;
            decimal distance = SellPriceRules.ShortDistanceMultiplier;
            decimal tradeEvent = SellPriceRules.NoEventMultiplier;
            bool regionalApplied = false;
            // 특산품은 계절과 거리 단계를 함께 제외하며 이벤트와 날씨는 그대로 적용한다.
            if (context.ItemType == TradeItemType.Normal)
            {
                regionalApplied = true;
                switch (context.Season)
                {
                    case SeasonPriceState.Favored:
                        season = SellPriceRules.FavoredSeasonMultiplier;
                        break;
                    case SeasonPriceState.Unfavored:
                        season = SellPriceRules.UnfavoredSeasonMultiplier;
                        break;
                }
                if (context.Distance == DistancePriceState.Long)
                    distance = SellPriceRules.LongDistanceMultiplier;
            }
            if (context.Event == TradeEventState.Lucky)
                tradeEvent = SellPriceRules.LuckyEventMultiplier;
            decimal weather = SellPriceRules.ClearWeatherMultiplier;
            switch (context.Weather)
            {
                case WeatherPriceState.Rain:
                    weather = SellPriceRules.RainWeatherMultiplier;
                    break;
                case WeatherPriceState.Storm:
                    weather = SellPriceRules.StormWeatherMultiplier;
                    break;
            }
            return new ResolvedSellPriceModifiers(season, distance, tradeEvent,
                regionalApplied, regionalApplied, true, weather, true);
        }
    }
}
