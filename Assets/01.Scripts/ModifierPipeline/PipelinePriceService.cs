using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.ModifierPipeline
{
    public sealed class PipelinePriceService
    {
        private readonly SellPriceModifierResolver resolver = new SellPriceModifierResolver();
        private readonly PriceCalculator calculator = new PriceCalculator();

        public PipelinePriceResult Calculate(SellPriceInput input)
        {
            var context = new SellPriceCalculationContext(input);
            var modifiers = resolver.Resolve(context);
            return new PipelinePriceResult(calculator.Calculate(context.BasePrice, modifiers), modifiers);
        }
    }
}
