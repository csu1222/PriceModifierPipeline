using PriceModifierPipeline.Common;
using PriceModifierPipeline.DirectCalculation;
using PriceModifierPipeline.ModifierPipeline;

namespace PriceModifierPipeline.Comparison
{
    public static class PriceArchitectureComparisonValidator
    {
        // 기대값은 호출자의 검증 데이터이며 Validator는 정책을 재구현하지 않는다.
        public static PriceArchitectureComparisonResult Validate(SellPriceInput input, int expectedPrice)
        {
            return new PriceArchitectureComparisonResult(input, expectedPrice,
                new DirectPreviewController().CalculatePreview(input).FinalPrice,
                new DirectCommitController().CalculateCommit(input).FinalPrice,
                new PipelinePriceService().Calculate(input).FinalPrice,
                new PipelinePriceService().Calculate(input).FinalPrice);
        }
    }
}
