using System;
using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.Comparison
{
    public static class PriceComparisonBaseline
    {
        // Phase 4 PipelinePriceTests.Contexts의 확정값. 테스트에서 원본과 대조한다.
        private static readonly int[] Expected = {
            100,150,130,195,120,180,156,234,80,120,104,156,
            100,150,100,150,100,150,100,150,100,150,100,150
        };

        public static int ExpectedPrice(SellPriceInput input)
        {
            if (input.BasePrice != 100)
                throw new ArgumentOutOfRangeException(nameof(input), "Frozen baseline requires base price 100.");
            return Expected[(int)input.ItemType * 12 + (int)input.Season * 4 + (int)input.Distance * 2 + (int)input.Event];
        }
    }
}
