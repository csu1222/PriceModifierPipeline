using PriceModifierPipeline.Debugging;
using UnityEngine;

namespace PriceModifierPipeline.DirectCalculation
{
    [RequireComponent(typeof(DirectPriceRuntime))]
    public sealed class DirectPriceDebugCommand : MonoBehaviour, IPriceArchitectureDebugCommand
    {
        private DirectPriceRuntime Runtime => GetComponent<DirectPriceRuntime>();
        public void Preview() => Runtime.Preview();
        public void Commit() => Runtime.Commit();
        public void NextSeason() => Runtime.NextSeason();
        public void ToggleDistance() => Runtime.ToggleDistance();
        public void NextWeather() => Runtime.NextWeather();
        public void ToggleEvent() => Runtime.ToggleEvent();
        public void ToggleItemType() => Runtime.ToggleItemType();
        public void Reset() => Runtime.Reset();
    }
}
