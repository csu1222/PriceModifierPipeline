using PriceModifierPipeline.Debugging;
using UnityEngine;

namespace PriceModifierPipeline.ModifierPipeline
{
    [RequireComponent(typeof(PipelinePriceRuntime))]
    public sealed class PipelinePriceDebugSource : MonoBehaviour, IPriceArchitectureDebugSource
    {
        public PriceDebugSnapshot CaptureSnapshot()
        {
            var runtime = GetComponent<PipelinePriceRuntime>();
            var result = runtime.LatestResult;
            return new PriceDebugSnapshot("Modifier Pipeline", runtime.Input,
                result.SeasonMultiplier, result.DistanceMultiplier, result.EventMultiplier,
                result.SeasonApplied, result.DistanceApplied, result.EventApplied,
                runtime.PreviewResult.FinalPrice, runtime.CommitResult.FinalPrice,
                runtime.HasPreview, runtime.HasCommit, runtime.LastAction,
                result.WeatherMultiplier, result.WeatherApplied);
        }
    }
}
