using UnityEngine;
using UnityEngine.UI;

namespace PriceModifierPipeline.Comparison
{
    public sealed class PriceArchitectureComparisonPanel : MonoBehaviour
    {
        [SerializeField] private ArchitectureComparisonRuntime runtime;
        [SerializeField] private Text input;
        [SerializeField] private Text direct;
        [SerializeField] private Text pipeline;
        [SerializeField] private Text comparison;

        private void OnEnable() => Refresh();
        private void Update() => Refresh();
        public void NextSeason() { runtime.NextSeason(); Refresh(); }
        public void ToggleDistance() { runtime.ToggleDistance(); Refresh(); }
        public void NextWeather() { runtime.NextWeather(); Refresh(); }
        public void ToggleEvent() { runtime.ToggleEvent(); Refresh(); }
        public void ToggleItemType() { runtime.ToggleItemType(); Refresh(); }
        public void RunBoth() { if (runtime.InputSynchronized) runtime.RunBoth(); Refresh(); }
        // Unity도 컴포넌트 추가 시 Reset을 호출하므로 직렬화 참조 설정 전에는 실행하지 않는다.
        public void Reset() { if (runtime) runtime.Reset(); Refresh(); }

        private static string State(bool value) => value ? "PASS" : "FAIL";
        private static string Applied(bool value) => value ? "ON" : "OFF";

        public void Refresh()
        {
            if (!runtime || !input || !direct || !pipeline || !comparison) return;
            var value = runtime.Direct.Input;
            input.text = $"INPUT   /   Base Price {value.BasePrice}\nItem  {value.ItemType}     Season  {value.Season}     Distance  {value.Distance}     Event  {value.Event}     Weather  {value.Weather}";
            if (!runtime.HasResults)
            {
                direct.text = "DIRECT CALCULATION\n\nPreview   —\nCommit    —\n\nSeason   —    Distance   —    Event   —\nWeather   —\nConsistency   N/A";
                pipeline.text = "MODIFIER PIPELINE\n\nPreview   —\nCommit    —\n\nSeason   —    Distance   —    Event   —\nWeather   —\nConsistency   N/A";
                comparison.text = $"Input Synchronized   {State(runtime.InputSynchronized)}     Cross Option   N/A\nExpected   {WeatherComparisonExpectations.ExpectedPrice(value)}     All vs Expected   N/A";
                return;
            }
            var result = runtime.CaptureResult();
            var d = runtime.Direct.LatestResult;
            var p = runtime.Pipeline.LatestResult;
            direct.text = $"DIRECT CALCULATION\n\nPreview   {result.DirectPreviewPrice}\nCommit    {result.DirectCommitPrice}\n\nSeason   {Applied(d.SeasonApplied)}    Distance   {Applied(d.DistanceApplied)}    Event   {Applied(d.EventApplied)}\nWeather   {Applied(d.WeatherApplied)}   ×{d.WeatherMultiplier:0.0}\nConsistency   {State(result.IsDirectConsistent)}";
            pipeline.text = $"MODIFIER PIPELINE\n\nPreview   {result.PipelinePreviewPrice}\nCommit    {result.PipelineCommitPrice}\n\nSeason   {Applied(p.SeasonApplied)}    Distance   {Applied(p.DistanceApplied)}    Event   {Applied(p.EventApplied)}\nWeather   {Applied(p.WeatherApplied)}   ×{p.WeatherMultiplier:0.0}\nConsistency   {State(result.IsPipelineConsistent)}";
            comparison.text = $"Input Synchronized   PASS     Cross Option   {State(result.IsCrossOptionEquivalent)}\nExpected   {result.ExpectedPrice}     All vs Expected   {State(result.AllEquivalent)}";
        }
    }
}
