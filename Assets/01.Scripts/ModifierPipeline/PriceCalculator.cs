using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.ModifierPipeline
{
    public sealed class PriceCalculator
    {
        public int Calculate(int basePrice, ResolvedSellPriceModifiers modifiers)
        {
            decimal price = basePrice;
            if (modifiers.SeasonApplied) price *= modifiers.SeasonMultiplier;
            if (modifiers.DistanceApplied) price *= modifiers.DistanceMultiplier;
            if (modifiers.EventApplied) price *= modifiers.EventMultiplier;
            return checked((int)decimal.Round(price, 0, SellPriceRules.RoundingMode));
        }
    }
}
