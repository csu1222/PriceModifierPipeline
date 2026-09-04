using System;

namespace PriceModifierPipeline.Common
{
    public static class SellPriceRules
    {
        public const int DefaultBasePrice = 100;
        public const decimal NormalSeasonMultiplier = 1.0m;
        public const decimal FavoredSeasonMultiplier = 1.2m;
        public const decimal UnfavoredSeasonMultiplier = 0.8m;
        public const decimal ShortDistanceMultiplier = 1.0m;
        public const decimal LongDistanceMultiplier = 1.3m;
        public const decimal ClearWeatherMultiplier = 1.0m;
        public const decimal RainWeatherMultiplier = 1.1m;
        public const decimal StormWeatherMultiplier = 1.2m;

        public const decimal NoEventMultiplier = 1.0m;
        public const decimal LuckyEventMultiplier = 1.5m;

        // 실행 순서 계약이며, 적용 대상 선택과 실제 실행은 각 Architecture가 담당한다.
        public const string ModifierOrder = "BasePrice -> Season -> Distance -> Event -> Weather -> FinalRound";
        public const MidpointRounding RoundingMode = MidpointRounding.AwayFromZero;
    }
}
