using System;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.ModifierPipeline;

namespace PriceModifierPipeline.Tests
{
    public class PipelinePolicyTests
    {
        [TestCase(TradeItemType.Normal, true, 1.2, 1.3)]
        [TestCase(TradeItemType.LocalSpecialty, false, 1.0, 1.0)]
        public void ResolverOwnsApplicability(TradeItemType item, bool applied, double season, double distance)
        {
            var input = new SellPriceInput(100, item, SeasonPriceState.Favored,
                DistancePriceState.Long, TradeEventState.Lucky);
            var context = new SellPriceCalculationContext(input);
            Assert.That(context.BasePrice, Is.EqualTo(input.BasePrice));
            Assert.That(context.ItemType, Is.EqualTo(input.ItemType));
            Assert.That(context.Season, Is.EqualTo(input.Season));
            Assert.That(context.Distance, Is.EqualTo(input.Distance));
            Assert.That(context.Event, Is.EqualTo(input.Event));
            var result = new SellPriceModifierResolver().Resolve(context);
            Assert.That(result.SeasonApplied, Is.EqualTo(applied));
            Assert.That(result.DistanceApplied, Is.EqualTo(applied));
            Assert.That(result.EventApplied, Is.True);
            Assert.That(result.SeasonMultiplier, Is.EqualTo((decimal)season));
            Assert.That(result.DistanceMultiplier, Is.EqualTo((decimal)distance));
            Assert.That(result.EventMultiplier, Is.EqualTo(1.5m));
        }

        [TestCase(0, 0)]
        [TestCase(1, 2)]
        [TestCase(5, 12)]
        [TestCase(100, 234)]
        public void CalculatorRoundsOnlyAtEnd(int input, int expected)
        {
            var modifiers = new ResolvedSellPriceModifiers(1.2m, 1.3m, 1.5m, true, true, true);
            Assert.That(new PriceCalculator().Calculate(input, modifiers), Is.EqualTo(expected));
        }

        [Test]
        public void CalculatorHonorsResolvedFlagsWithoutDomainConditions()
        {
            var modifiers = new ResolvedSellPriceModifiers(9m, 8m, 1.5m, false, false, true);
            Assert.That(new PriceCalculator().Calculate(1, modifiers), Is.EqualTo(2));
            modifiers = new ResolvedSellPriceModifiers(9m, 8m, 7m, false, false, false);
            Assert.That(new PriceCalculator().Calculate(100, modifiers), Is.EqualTo(100));
        }

        [Test]
        public void CalculatorUsesDecimalPrecision()
        {
            var modifiers = new ResolvedSellPriceModifiers(1.4999999999999999999999999999m,
                1m, 1m, true, true, true);
            Assert.That(new PriceCalculator().Calculate(1, modifiers), Is.EqualTo(1));
        }

        [Test]
        public void CalculatorMultipliesSeasonBeforeDistanceBeforeEvent()
        {
            var calculator = new PriceCalculator();
            // Overflow-sensitive operands distinguish sequence despite multiplication commutativity.
            Assert.Throws<OverflowException>(() => calculator.Calculate(2,
                new ResolvedSellPriceModifiers(decimal.MaxValue, 0m, 1m, true, true, true)));
            Assert.That(calculator.Calculate(2,
                new ResolvedSellPriceModifiers(0m, decimal.MaxValue, decimal.MaxValue, true, true, true)), Is.Zero);
            Assert.Throws<OverflowException>(() => calculator.Calculate(2,
                new ResolvedSellPriceModifiers(1m, decimal.MaxValue, 0m, true, true, true)));
        }

        [Test]
        public void CalculatorRejectsIntOverflow()
        {
            Assert.Throws<OverflowException>(() => new PriceCalculator().Calculate(int.MaxValue,
                new ResolvedSellPriceModifiers(1m, 1m, 1.5m, true, true, true)));
        }
    }
}
