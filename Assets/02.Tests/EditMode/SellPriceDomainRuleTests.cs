using System;
using System.Collections.Generic;
using NUnit.Framework;
using PriceModifierPipeline.Common;

namespace PriceModifierPipeline.Tests
{
    public class SellPriceDomainRuleTests
    {
        [TestCase(TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None, 100)]
        [TestCase(TradeItemType.Normal, SeasonPriceState.Favored, DistancePriceState.Short, TradeEventState.None, 120)]
        [TestCase(TradeItemType.Normal, SeasonPriceState.Unfavored, DistancePriceState.Short, TradeEventState.None, 80)]
        [TestCase(TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Long, TradeEventState.None, 130)]
        [TestCase(TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.Lucky, 150)]
        [TestCase(TradeItemType.Normal, SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky, 234)]
        [TestCase(TradeItemType.LocalSpecialty, SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.None, 100)]
        [TestCase(TradeItemType.LocalSpecialty, SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky, 150)]
        public void RequiredCombinationsMatchExpectedPrice(TradeItemType itemType,
            SeasonPriceState season, DistancePriceState distance, TradeEventState tradeEvent, int expected)
        {
            var input = new SellPriceInput(SellPriceRules.DefaultBasePrice, itemType, season, distance, tradeEvent);
            Assert.That(CalculateReference(input), Is.EqualTo(expected));
        }

        [TestCase(SeasonPriceState.Normal, TradeEventState.None, 100)]
        [TestCase(SeasonPriceState.Favored, TradeEventState.Lucky, 150)]
        [TestCase(SeasonPriceState.Unfavored, TradeEventState.Lucky, 150)]
        public void LocalSpecialtyExcludesSeasonAndDistance(SeasonPriceState season,
            TradeEventState tradeEvent, int expected)
        {
            var applied = new List<string>();
            var input = new SellPriceInput(100, TradeItemType.LocalSpecialty, season, DistancePriceState.Long, tradeEvent);
            Assert.That(CalculateReference(input, applied), Is.EqualTo(expected));
            Assert.That(applied, Is.EqualTo(new[] { "Event" }));
        }

        [Test]
        public void NormalAppliesModifiersInContractOrder()
        {
            var applied = new List<string>();
            CalculateReference(new SellPriceInput(100, TradeItemType.Normal,
                SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky), applied);
            Assert.That("BasePrice -> " + string.Join(" -> ", applied) + " -> FinalRound",
                Is.EqualTo(SellPriceRules.ModifierOrder));
        }

        [Test]
        public void RoundsOnlyOnceAfterAllModifiers()
        {
            var input = new SellPriceInput(1, TradeItemType.Normal,
                SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky);
            // 2.34를 최종 반올림하면 2, 각 단계에서 반올림하면 3이다.
            Assert.That(CalculateReference(input), Is.EqualTo(2));
        }

        [Test]
        public void MidpointRoundsAwayFromZero()
        {
            var input = new SellPriceInput(3, TradeItemType.Normal,
                SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.Lucky);
            Assert.That(CalculateReference(input), Is.EqualTo(5));
        }

        [Test]
        public void DefaultInputIsValidZeroPrice()
        {
            Assert.That(CalculateReference(default), Is.Zero);
        }

        [Test]
        public void ZeroRemainsZeroWithAllModifiers()
        {
            Assert.That(CalculateReference(new SellPriceInput(0, TradeItemType.Normal,
                SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky)), Is.Zero);
        }

        [Test]
        public void NegativeBasePriceIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(-1,
                TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None));
        }

        [Test]
        public void UndefinedEnumValuesAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(100,
                (TradeItemType)99, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(100,
                TradeItemType.Normal, (SeasonPriceState)99, DistancePriceState.Short, TradeEventState.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(100,
                TradeItemType.Normal, SeasonPriceState.Normal, (DistancePriceState)99, TradeEventState.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(100,
                TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, (TradeEventState)99));
        }

        [Test]
        public void MaximumBasePriceWithoutBoostIsPreserved()
        {
            Assert.That(CalculateReference(new SellPriceInput(int.MaxValue, TradeItemType.Normal,
                SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None)), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void FinalPriceOutsideIntRangeThrows()
        {
            Assert.Throws<OverflowException>(() => CalculateReference(new SellPriceInput(int.MaxValue,
                TradeItemType.Normal, SeasonPriceState.Favored, DistancePriceState.Long, TradeEventState.Lucky)));
        }

        // 규칙 기대값 검증 전용이다. Runtime의 Direct/Pipeline 계산 경로에서 재사용하지 않는다.
        private static int CalculateReference(SellPriceInput input, List<string> applied = null)
        {
            decimal rawPrice = input.BasePrice;
            if (input.ItemType == TradeItemType.Normal)
            {
                applied?.Add("Season");
                switch (input.Season)
                {
                    case SeasonPriceState.Normal: rawPrice *= SellPriceRules.NormalSeasonMultiplier; break;
                    case SeasonPriceState.Favored: rawPrice *= SellPriceRules.FavoredSeasonMultiplier; break;
                    case SeasonPriceState.Unfavored: rawPrice *= SellPriceRules.UnfavoredSeasonMultiplier; break;
                    default: throw new ArgumentOutOfRangeException(nameof(input));
                }

                applied?.Add("Distance");
                switch (input.Distance)
                {
                    case DistancePriceState.Short: rawPrice *= SellPriceRules.ShortDistanceMultiplier; break;
                    case DistancePriceState.Long: rawPrice *= SellPriceRules.LongDistanceMultiplier; break;
                    default: throw new ArgumentOutOfRangeException(nameof(input));
                }
            }

            applied?.Add("Event");
            switch (input.Event)
            {
                case TradeEventState.None: rawPrice *= SellPriceRules.NoEventMultiplier; break;
                case TradeEventState.Lucky: rawPrice *= SellPriceRules.LuckyEventMultiplier; break;
                default: throw new ArgumentOutOfRangeException(nameof(input));
            }
            return checked((int)Math.Round(rawPrice, 0, SellPriceRules.RoundingMode));
        }
    }
}
