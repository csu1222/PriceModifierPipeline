using System;
using System.Collections;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.Comparison;
using UnityEngine;

namespace PriceModifierPipeline.Tests
{
    public class PriceArchitectureComparisonTests
    {
        // 기존 Cross-Option matrix를 직접 재사용한다. 기대값 목록을 테스트에 복제하지 않는다.
        public static IEnumerable Contexts() => PipelinePriceTests.Contexts();

        [TestCaseSource(nameof(Contexts))]
        public void FrozenMatrixMatchesValidatorAndSynchronizedRuntimes(TradeItemType item,
            SeasonPriceState season, DistancePriceState distance, TradeEventState tradeEvent, int expected)
        {
            var input = new SellPriceInput(100, item, season, distance, tradeEvent);
            Assert.That(PriceComparisonBaseline.ExpectedPrice(input), Is.EqualTo(expected));
            Assert.That(PriceArchitectureComparisonValidator.Validate(input, expected).AllEquivalent, Is.True);
            var owner = new GameObject("Comparison matrix");
            try
            {
                var runtime = owner.AddComponent<ArchitectureComparisonRuntime>();
                for (int i = 0; i < (int)season; i++) runtime.NextSeason();
                if (distance == DistancePriceState.Long) runtime.ToggleDistance();
                if (tradeEvent == TradeEventState.Lucky) runtime.ToggleEvent();
                if (item == TradeItemType.LocalSpecialty) runtime.ToggleItemType();
                Assert.That(runtime.Direct.Input, Is.EqualTo(input));
                Assert.That(runtime.Pipeline.Input, Is.EqualTo(input));
                runtime.RunBoth();
                var result = runtime.CaptureResult();
                Assert.That(result.AllEquivalent, Is.True);
                Assert.That(result.DirectCommitPrice, Is.EqualTo(expected));
                Assert.That(runtime.Direct.LatestResult.SeasonApplied, Is.EqualTo(item == TradeItemType.Normal));
                Assert.That(runtime.Direct.LatestResult.DistanceApplied, Is.EqualTo(item == TradeItemType.Normal));
                Assert.That(runtime.Direct.LatestResult.EventApplied, Is.True);
                Assert.That(runtime.Pipeline.LatestResult.SeasonApplied, Is.EqualTo(runtime.Direct.LatestResult.SeasonApplied));
                Assert.That(runtime.Pipeline.LatestResult.DistanceApplied, Is.EqualTo(runtime.Direct.LatestResult.DistanceApplied));
                Assert.That(runtime.Pipeline.LatestResult.EventApplied, Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [Test]
        public void CommandsInvalidateResultsAndResetRepairsDesynchronization()
        {
            var owner = new GameObject("Comparison commands");
            try
            {
                var runtime = owner.AddComponent<ArchitectureComparisonRuntime>();
                var initial = runtime.Direct.Input;
                Assert.Throws<InvalidOperationException>(() => runtime.CaptureResult());
                foreach (Action action in new Action[] { runtime.NextSeason, runtime.ToggleDistance, runtime.ToggleEvent, runtime.ToggleItemType, runtime.Reset })
                {
                    runtime.RunBoth();
                    action();
                    Assert.That(runtime.InputSynchronized, Is.True);
                    Assert.That(runtime.HasResults, Is.False);
                    Assert.That(runtime.Direct.HasPreview || runtime.Direct.HasCommit || runtime.Pipeline.HasPreview || runtime.Pipeline.HasCommit, Is.False);
                }
                Assert.That(runtime.Direct.Input, Is.EqualTo(initial));
                runtime.Direct.ToggleEvent();
                Assert.That(runtime.InputSynchronized, Is.False);
                Assert.Throws<InvalidOperationException>(() => runtime.RunBoth());
                runtime.Reset(); runtime.RunBoth();
                Assert.That(runtime.CaptureResult().AllEquivalent, Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [Test]
        public void ResultDistinguishesInternalCrossOptionAndReferenceFailures()
        {
            var input = new SellPriceInput(100, TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None);
            Assert.That(new PriceArchitectureComparisonResult(input, 100, 100, 99, 100, 100).IsDirectConsistent, Is.False);
            Assert.That(new PriceArchitectureComparisonResult(input, 100, 100, 100, 100, 99).IsPipelineConsistent, Is.False);
            Assert.That(new PriceArchitectureComparisonResult(input, 100, 100, 100, 99, 99).IsCrossOptionEquivalent, Is.False);
            var wrong = new PriceArchitectureComparisonResult(input, 100, 99, 99, 99, 99);
            Assert.That(wrong.IsCrossOptionEquivalent, Is.True);
            Assert.That(wrong.AllEquivalent, Is.False);
            Assert.That(PriceArchitectureComparisonValidator.Validate(input, 99).AllEquivalent, Is.False);
        }
    }
}
