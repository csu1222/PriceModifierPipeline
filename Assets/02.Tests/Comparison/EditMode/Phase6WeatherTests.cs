using System;
using System.Collections.Generic;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.Comparison;
using PriceModifierPipeline.DirectCalculation;
using PriceModifierPipeline.ModifierPipeline;
using UnityEngine;

namespace PriceModifierPipeline.Tests
{
    public class Phase6WeatherTests
    {
        public static IEnumerable<TestCaseData> Contexts()
        {
            for (int item = 0; item < 2; item++)
            for (int season = 0; season < 3; season++)
            for (int distance = 0; distance < 2; distance++)
            for (int tradeEvent = 0; tradeEvent < 2; tradeEvent++)
            for (int weather = 0; weather < 3; weather++)
                yield return new TestCaseData(item, season, distance, tradeEvent, weather);
        }

        [TestCaseSource(nameof(Contexts))]
        public void All72ContextsMatchIndependentExpectationAndRealRuntimePaths(
            int item, int season, int distance, int tradeEvent, int weather)
        {
            // Production constants/lookup/calculator를 참조하지 않는 요구사항 기반 decimal oracle.
            decimal raw = 100m;
            if (item == 0) raw *= new[] { 1m, 1.2m, 0.8m }[season] * new[] { 1m, 1.3m }[distance];
            raw *= new[] { 1m, 1.5m }[tradeEvent] * new[] { 1m, 1.1m, 1.2m }[weather];
            int expected = (int)decimal.Round(raw, 0, MidpointRounding.AwayFromZero);
            var owner = new GameObject("Phase6 matrix");
            try
            {
                var runtime = owner.AddComponent<ArchitectureComparisonRuntime>();
                if (item == 1) runtime.ToggleItemType();
                for (int i = 0; i < season; i++) runtime.NextSeason();
                if (distance == 1) runtime.ToggleDistance();
                if (tradeEvent == 1) runtime.ToggleEvent();
                for (int i = 0; i < weather; i++) runtime.NextWeather();
                runtime.RunBoth();
                var result = runtime.CaptureResult();
                Assert.That(runtime.InputSynchronized, Is.True);
                Assert.That(result.ExpectedPrice, Is.EqualTo(expected));
                Assert.That(result.DirectPreviewPrice, Is.EqualTo(expected));
                Assert.That(result.DirectCommitPrice, Is.EqualTo(expected));
                Assert.That(result.PipelinePreviewPrice, Is.EqualTo(expected));
                Assert.That(result.PipelineCommitPrice, Is.EqualTo(expected));
                foreach (var direct in new[] { runtime.Direct.PreviewResult, runtime.Direct.CommitResult })
                {
                    Assert.That(direct.WeatherApplied, Is.True);
                    Assert.That(direct.WeatherMultiplier, Is.EqualTo(new[] { 1m, 1.1m, 1.2m }[weather]));
                    Assert.That(direct.SeasonApplied, Is.EqualTo(item == 0));
                    Assert.That(direct.DistanceApplied, Is.EqualTo(item == 0));
                    Assert.That(direct.EventApplied, Is.True);
                }
                foreach (var pipeline in new[] { runtime.Pipeline.PreviewResult, runtime.Pipeline.CommitResult })
                {
                    Assert.That(pipeline.WeatherApplied, Is.True);
                    Assert.That(pipeline.WeatherMultiplier, Is.EqualTo(new[] { 1m, 1.1m, 1.2m }[weather]));
                    Assert.That(pipeline.SeasonApplied, Is.EqualTo(item == 0));
                    Assert.That(pipeline.DistanceApplied, Is.EqualTo(item == 0));
                    Assert.That(pipeline.EventApplied, Is.True);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        [Test]
        public void WeatherInputContractAndConstants()
        {
            Assert.That(Enum.GetValues(typeof(WeatherPriceState)), Is.EqualTo(new[] {
                WeatherPriceState.Clear, WeatherPriceState.Rain, WeatherPriceState.Storm }));
            Assert.That(default(SellPriceInput).Weather, Is.EqualTo(WeatherPriceState.Clear));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SellPriceInput(100,
                TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short,
                TradeEventState.None, (WeatherPriceState)99));
            Assert.That(SellPriceRules.ClearWeatherMultiplier, Is.EqualTo(1m));
            Assert.That(SellPriceRules.RainWeatherMultiplier, Is.EqualTo(1.1m));
            Assert.That(SellPriceRules.StormWeatherMultiplier, Is.EqualTo(1.2m));
            Assert.That(SellPriceRules.ModifierOrder, Is.EqualTo("BasePrice -> Season -> Distance -> Event -> Weather -> FinalRound"));
        }

        [TestCase(100, TradeItemType.Normal, WeatherPriceState.Rain, 257)]
        [TestCase(100, TradeItemType.Normal, WeatherPriceState.Storm, 281)]
        [TestCase(100, TradeItemType.LocalSpecialty, WeatherPriceState.Rain, 165)]
        [TestCase(1, TradeItemType.Normal, WeatherPriceState.Rain, 3)]
        [TestCase(5, TradeItemType.Normal, WeatherPriceState.Rain, 13)]
        [TestCase(0, TradeItemType.LocalSpecialty, WeatherPriceState.Storm, 0)]
        public void RequiredCasesAndRoundOnlyAfterWeather(int price, TradeItemType item, WeatherPriceState weather, int expected)
        {
            var input = new SellPriceInput(price, item, SeasonPriceState.Favored,
                DistancePriceState.Long, TradeEventState.Lucky, weather);
            Assert.That(new DirectPreviewController().CalculatePreview(input).FinalPrice, Is.EqualTo(expected));
            Assert.That(new DirectCommitController().CalculateCommit(input).FinalPrice, Is.EqualTo(expected));
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(expected));
        }

        [Test]
        public void MidpointAndWeatherOverflowArePreserved()
        {
            var input = new SellPriceInput(5, TradeItemType.Normal, SeasonPriceState.Normal,
                DistancePriceState.Short, TradeEventState.None, WeatherPriceState.Rain);
            Assert.That(new DirectPreviewController().CalculatePreview(input).FinalPrice, Is.EqualTo(6));
            Assert.That(new DirectCommitController().CalculateCommit(input).FinalPrice, Is.EqualTo(6));
            Assert.That(new PipelinePriceService().Calculate(input).FinalPrice, Is.EqualTo(6));
            var overflow = new SellPriceInput(int.MaxValue, TradeItemType.LocalSpecialty,
                SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None, WeatherPriceState.Storm);
            Assert.Throws<OverflowException>(() => new DirectPreviewController().CalculatePreview(overflow));
            Assert.Throws<OverflowException>(() => new DirectCommitController().CalculateCommit(overflow));
            Assert.Throws<OverflowException>(() => new PipelinePriceService().Calculate(overflow));
        }

        [Test]
        public void CalculatorExecutesResolvedWeatherWithoutDomainPolicy()
        {
            var calculator = new PriceCalculator();
            Assert.That(calculator.Calculate(100, new ResolvedSellPriceModifiers(1m, 1m, 1m,
                false, false, true, 1.37m, true)), Is.EqualTo(137));
            Assert.That(calculator.Calculate(100, new ResolvedSellPriceModifiers(1m, 1m, 1m,
                false, false, true, 1.37m, false)), Is.EqualTo(100));
        }

        [Test]
        public void WeatherCommandsRetainInputInvalidateResultsDetectDesyncAndReset()
        {
            var owner = new GameObject("Weather commands");
            try
            {
                var runtime = owner.AddComponent<ArchitectureComparisonRuntime>();
                runtime.NextWeather();
                foreach (Action command in new Action[] { runtime.NextSeason, runtime.ToggleDistance,
                    runtime.ToggleEvent, runtime.ToggleItemType })
                {
                    runtime.RunBoth(); command();
                    Assert.That(runtime.HasResults, Is.False);
                    Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Rain));
                    Assert.That(runtime.Pipeline.Input.Weather, Is.EqualTo(WeatherPriceState.Rain));
                    Assert.That(runtime.Direct.LatestResult.WeatherApplied, Is.False);
                    Assert.That(runtime.Pipeline.LatestResult.WeatherApplied, Is.False);
                }
                runtime.RunBoth(); runtime.NextWeather();
                Assert.That(runtime.HasResults, Is.False);
                Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Storm));
                runtime.NextWeather();
                Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Clear));
                runtime.Direct.NextWeather();
                Assert.That(runtime.InputSynchronized, Is.False);
                Assert.Throws<InvalidOperationException>(() => runtime.RunBoth());
                runtime.Reset();
                Assert.That(runtime.InputSynchronized, Is.True);
                Assert.That(runtime.HasResults, Is.False);
                Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Clear));
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }
    }
}
