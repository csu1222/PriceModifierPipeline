using System;
using PriceModifierPipeline.DirectCalculation;
using PriceModifierPipeline.ModifierPipeline;
using UnityEngine;

namespace PriceModifierPipeline.Comparison
{
    [RequireComponent(typeof(DirectPriceRuntime), typeof(PipelinePriceRuntime))]
    public sealed class ArchitectureComparisonRuntime : MonoBehaviour
    {
        public DirectPriceRuntime Direct => GetComponent<DirectPriceRuntime>();
        public PipelinePriceRuntime Pipeline => GetComponent<PipelinePriceRuntime>();
        public bool InputSynchronized => Direct.Input.Equals(Pipeline.Input);
        public bool HasResults => InputSynchronized && Direct.HasPreview && Direct.HasCommit && Pipeline.HasPreview && Pipeline.HasCommit;

        public PriceArchitectureComparisonResult CaptureResult()
        {
            if (!HasResults) throw new InvalidOperationException("Synchronized results are required.");
            return new PriceArchitectureComparisonResult(Direct.Input, WeatherComparisonExpectations.ExpectedPrice(Direct.Input),
                Direct.PreviewResult.FinalPrice, Direct.CommitResult.FinalPrice,
                Pipeline.PreviewResult.FinalPrice, Pipeline.CommitResult.FinalPrice);
        }

        public void RunBoth()
        {
            if (!InputSynchronized) throw new InvalidOperationException("Inputs differ; Reset both runtimes before comparison.");
            Direct.Preview(); Direct.Commit();
            Pipeline.Preview(); Pipeline.Commit();
        }

        public void NextSeason() { Direct.NextSeason(); Pipeline.NextSeason(); }
        public void ToggleDistance() { Direct.ToggleDistance(); Pipeline.ToggleDistance(); }
        public void NextWeather() { Direct.NextWeather(); Pipeline.NextWeather(); }
        public void ToggleEvent() { Direct.ToggleEvent(); Pipeline.ToggleEvent(); }
        public void ToggleItemType() { Direct.ToggleItemType(); Pipeline.ToggleItemType(); }
        public void Reset() { Direct.Reset(); Pipeline.Reset(); }
    }
}
