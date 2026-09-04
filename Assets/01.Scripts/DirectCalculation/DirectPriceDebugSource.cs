using PriceModifierPipeline.Debugging;
using UnityEngine;

namespace PriceModifierPipeline.DirectCalculation
{
    [RequireComponent(typeof(DirectPriceRuntime))]
    public sealed class DirectPriceDebugSource : MonoBehaviour, IPriceArchitectureDebugSource
    {
        public PriceDebugSnapshot CaptureSnapshot()
        {
            var runtime = GetComponent<DirectPriceRuntime>();
            var result = runtime.LatestResult;
            return new PriceDebugSnapshot("Direct Calculation", runtime.Input,
                result.SeasonMultiplier, result.DistanceMultiplier, result.EventMultiplier,
                result.SeasonApplied, result.DistanceApplied, result.EventApplied,
                runtime.PreviewResult.FinalPrice, runtime.CommitResult.FinalPrice,
                runtime.HasPreview, runtime.HasCommit, runtime.LastAction);
        }
    }
}
