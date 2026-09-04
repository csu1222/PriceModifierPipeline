using PriceModifierPipeline.Debugging;
using UnityEngine;

namespace PriceModifierPipeline.ModifierPipeline
{
    [RequireComponent(typeof(PipelinePriceRuntime))]
    public sealed class PipelinePriceDebugCommand : MonoBehaviour, IPriceArchitectureDebugCommand
    {
        private PipelinePriceRuntime Runtime => GetComponent<PipelinePriceRuntime>();
        public void Preview() => Runtime.Preview();
        public void Commit() => Runtime.Commit();
        public void NextSeason() => Runtime.NextSeason();
        public void ToggleDistance() => Runtime.ToggleDistance();
        public void ToggleEvent() => Runtime.ToggleEvent();
        public void ToggleItemType() => Runtime.ToggleItemType();
        public void Reset() => Runtime.Reset();
    }
}
