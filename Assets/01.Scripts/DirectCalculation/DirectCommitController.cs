using System;
using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.DirectCalculation
{
    public sealed class DirectCommitController
    {
        public DirectPriceResult CalculateCommit(SellPriceInput input)
        {
            decimal price = input.BasePrice;
            decimal seasonMultiplier = SellPriceRules.NormalSeasonMultiplier;
            decimal distanceMultiplier = SellPriceRules.ShortDistanceMultiplier;
            decimal eventMultiplier = SellPriceRules.NoEventMultiplier;
            bool seasonApplied = false;
            bool distanceApplied = false;

            // Option 1의 독립 정책 경로: 다른 Controller의 판단이나 결과를 공유하지 않는다.
            if (input.ItemType == TradeItemType.Normal)
            {
                seasonApplied = true;
                switch (input.Season)
                {
                    case SeasonPriceState.Favored:
                        seasonMultiplier = SellPriceRules.FavoredSeasonMultiplier;
                        break;
                    case SeasonPriceState.Unfavored:
                        seasonMultiplier = SellPriceRules.UnfavoredSeasonMultiplier;
                        break;
                }
                price *= seasonMultiplier;
                distanceApplied = true;
                if (input.Distance == DistancePriceState.Long)
                    distanceMultiplier = SellPriceRules.LongDistanceMultiplier;
                price *= distanceMultiplier;
            }

            if (input.Event == TradeEventState.Lucky)
                eventMultiplier = SellPriceRules.LuckyEventMultiplier;
            price *= eventMultiplier;
            decimal weatherMultiplier = SellPriceRules.ClearWeatherMultiplier;
            switch (input.Weather)
            {
                case WeatherPriceState.Rain:
                    weatherMultiplier = SellPriceRules.RainWeatherMultiplier;
                    break;
                case WeatherPriceState.Storm:
                    weatherMultiplier = SellPriceRules.StormWeatherMultiplier;
                    break;
            }
            price *= weatherMultiplier;
            int finalPrice = checked((int)decimal.Round(price, 0, SellPriceRules.RoundingMode));
            return new DirectPriceResult(finalPrice, seasonMultiplier, distanceMultiplier,
                eventMultiplier, seasonApplied, distanceApplied, true, weatherMultiplier, true);
        }
    }
}
