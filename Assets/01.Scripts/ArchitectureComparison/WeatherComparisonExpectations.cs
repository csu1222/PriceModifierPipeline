using System;
using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.Comparison
{
    public static class WeatherComparisonExpectations
    {
        // BasePrice 100의 독립 고정 기대값. Weather, Item, Season, Distance, Event 순서.
        private static readonly int[] Expected = {
            100,150,130,195,120,180,156,234,80,120,104,156,
            100,150,100,150,100,150,100,150,100,150,100,150,
            110,165,143,215,132,198,172,257,88,132,114,172,
            110,165,110,165,110,165,110,165,110,165,110,165,
            120,180,156,234,144,216,187,281,96,144,125,187,
            120,180,120,180,120,180,120,180,120,180,120,180
        };

        public static int ExpectedPrice(SellPriceInput input)
        {
            if (input.BasePrice != 100)
                throw new ArgumentOutOfRangeException(nameof(input), "Weather expectations require base price 100.");
            return Expected[(int)input.Weather * 24 + (int)input.ItemType * 12
                + (int)input.Season * 4 + (int)input.Distance * 2 + (int)input.Event];
        }
    }
}
