using System;
using System.Collections;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.DirectCalculation;
using PriceModifierPipeline.ModifierPipeline;
using PriceModifierPipeline.Debugging;
using UnityEngine;

namespace PriceModifierPipeline.Tests
{
    public class PipelinePriceTests
    {
        public static IEnumerable Contexts()
        {
            int[] expected = { 100,150,130,195,120,180,156,234,80,120,104,156,
                100,150,100,150,100,150,100,150,100,150,100,150 };
            int index = 0;
            foreach (TradeItemType item in Enum.GetValues(typeof(TradeItemType)))
            foreach (SeasonPriceState season in Enum.GetValues(typeof(SeasonPriceState)))
            foreach (DistancePriceState distance in Enum.GetValues(typeof(DistancePriceState)))
            foreach (TradeEventState tradeEvent in Enum.GetValues(typeof(TradeEventState)))
                yield return new TestCaseData(item, season, distance, tradeEvent, expected[index++]);
        }

        [TestCaseSource(nameof(Contexts))]
        public void IndependentControllersMatchExpectedPriceAndAppliedState(TradeItemType item,
            SeasonPriceState season, DistancePriceState distance, TradeEventState tradeEvent, int expected)
        {
            var input = new SellPriceInput(100, item, season, distance, tradeEvent);
            var preview = new PipelinePriceService().Calculate(input);
            var commit = new PipelinePriceService().Calculate(input);
            Assert.That(preview.FinalPrice, Is.EqualTo(expected));
            Assert.That(commit.FinalPrice, Is.EqualTo(expected));
            Assert.That(preview.SeasonApplied, Is.EqualTo(item == TradeItemType.Normal));
            Assert.That(preview.DistanceApplied, Is.EqualTo(item == TradeItemType.Normal));
            Assert.That(preview.EventApplied, Is.True);
            Assert.That(commit, Is.EqualTo(preview));
            var directPreview = new DirectPreviewController().CalculatePreview(input);
            var directCommit = new DirectCommitController().CalculateCommit(input);
            Assert.That(directPreview, Is.EqualTo(directCommit));
            Assert.That(preview.FinalPrice, Is.EqualTo(directPreview.FinalPrice));
            Assert.That(preview.SeasonMultiplier, Is.EqualTo(directPreview.SeasonMultiplier));
            Assert.That(preview.DistanceMultiplier, Is.EqualTo(directPreview.DistanceMultiplier));
            Assert.That(preview.EventMultiplier, Is.EqualTo(directPreview.EventMultiplier));
            var owner = new GameObject("Pipeline context test");
            try
            {
                var runtime = owner.AddComponent<PipelinePriceRuntime>();
                for (int i = 0; i < (int)season; i++) runtime.NextSeason();
                if (distance == DistancePriceState.Long) runtime.ToggleDistance();
                if (tradeEvent == TradeEventState.Lucky) runtime.ToggleEvent();
                if (item == TradeItemType.LocalSpecialty) runtime.ToggleItemType();
                runtime.Commit();
                Assert.That(runtime.HasPreview, Is.False);
                runtime.Preview();
                Assert.That(runtime.PreviewResult, Is.EqualTo(preview));
                Assert.That(runtime.CommitResult, Is.EqualTo(preview));
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [TestCase(1, 2)]
        [TestCase(5, 12)]
        [TestCase(0, 0)]
        public void OnlyFinalValueIsRounded(int price, int expected)
        {
            var input = new SellPriceInput(price, TradeItemType.Normal, SeasonPriceState.Favored,
                DistancePriceState.Long, TradeEventState.Lucky);
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(expected));
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(expected));
        }

        [Test]
        public void MidpointRoundsAwayFromZero()
        {
            var input = new SellPriceInput(1, TradeItemType.LocalSpecialty, SeasonPriceState.Normal,
                DistancePriceState.Short, TradeEventState.Lucky);
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(2));
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(2));
        }

        [Test]
        public void OverflowThrowsInBothPaths()
        {
            var input = new SellPriceInput(int.MaxValue, TradeItemType.Normal, SeasonPriceState.Favored,
                DistancePriceState.Long, TradeEventState.Lucky);
            Assert.Throws<OverflowException>(() => new PipelinePriceService().Calculate(input));
            Assert.Throws<OverflowException>(() => new PipelinePriceService().Calculate(input));
        }

        [Test]
        public void CommandsInvalidateBothResultsAndResetRestoresDefaults()
        {
            var owner = new GameObject("Direct test");
            try
            {
                var runtime = owner.AddComponent<PipelinePriceRuntime>();
                var source = owner.AddComponent<PipelinePriceDebugSource>();
                var command = owner.AddComponent<PipelinePriceDebugCommand>();
                Assert.That(source.CaptureSnapshot().ConsistencyState, Is.EqualTo(PriceConsistencyState.NotEvaluated));
                command.Commit();
                Assert.That(runtime.HasPreview, Is.False);
                Assert.That(runtime.CommitResult.FinalPrice, Is.EqualTo(100));
                command.Preview();
                Assert.That(source.CaptureSnapshot().ConsistencyState, Is.EqualTo(PriceConsistencyState.Pass));
                foreach (Action change in new Action[] {command.NextSeason, command.ToggleDistance, command.ToggleEvent, command.ToggleItemType})
                {
                    change();
                    Assert.That(runtime.HasPreview || runtime.HasCommit, Is.False);
                    Assert.That(source.CaptureSnapshot().ConsistencyState, Is.EqualTo(PriceConsistencyState.NotEvaluated));
                    command.Preview();
                    command.Commit();
                    Assert.That(source.CaptureSnapshot().ConsistencyState, Is.EqualTo(PriceConsistencyState.Pass));
                }
                Assert.That(runtime.CommitResult.FinalPrice, Is.EqualTo(150));
                Assert.That(source.CaptureSnapshot().SeasonApplied, Is.False);
                command.Reset();
                Assert.That(runtime.Input.BasePrice, Is.EqualTo(100));
                Assert.That(runtime.Input.ItemType, Is.EqualTo(TradeItemType.Normal));
                Assert.That(runtime.Input.Season, Is.EqualTo(SeasonPriceState.Normal));
                Assert.That(runtime.Input.Distance, Is.EqualTo(DistancePriceState.Short));
                Assert.That(runtime.Input.Event, Is.EqualTo(TradeEventState.None));
                Assert.That(runtime.HasPreview || runtime.HasCommit, Is.False);
                command.NextSeason(); command.NextSeason(); command.NextSeason();
                Assert.That(runtime.Input.Season, Is.EqualTo(SeasonPriceState.Normal));
                command.ToggleDistance(); command.ToggleDistance();
                command.ToggleEvent(); command.ToggleEvent();
                command.ToggleItemType(); command.ToggleItemType();
                Assert.That(runtime.Input.Distance, Is.EqualTo(DistancePriceState.Short));
                Assert.That(runtime.Input.Event, Is.EqualTo(TradeEventState.None));
                Assert.That(runtime.Input.ItemType, Is.EqualTo(TradeItemType.Normal));
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }
    }
}
