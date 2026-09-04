using PriceModifierPipeline.Common;
using UnityEngine;

namespace PriceModifierPipeline.ModifierPipeline
{
    public sealed class PipelinePriceRuntime : MonoBehaviour
    {
        private readonly PipelinePriceService service = new PipelinePriceService();
        public SellPriceInput Input { get; private set; } = new SellPriceInput(SellPriceRules.DefaultBasePrice,
            TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None);
        public PipelinePriceResult PreviewResult { get; private set; }
        public PipelinePriceResult CommitResult { get; private set; }
        public PipelinePriceResult LatestResult { get; private set; }
        public bool HasPreview { get; private set; }
        public bool HasCommit { get; private set; }
        public string LastAction { get; private set; } = "Ready (no calculation)";

        public void Preview()
        {
            PreviewResult = service.Calculate(Input);
            LatestResult = PreviewResult;
            HasPreview = true;
            LastAction = "Preview";
        }

        public void Commit()
        {
            CommitResult = service.Calculate(Input);
            LatestResult = CommitResult;
            HasCommit = true;
            LastAction = "Commit";
        }

        // 입력 변경은 이전 Context의 결과와 적용 상태를 함께 무효화한다.
        private void ChangeInput(SellPriceInput input, string action)
        {
            Input = input;
            HasPreview = HasCommit = false;
            PreviewResult = CommitResult = LatestResult = default;
            LastAction = action + " (results cleared)";
        }

        public void NextSeason() => ChangeInput(new SellPriceInput(Input.BasePrice, Input.ItemType,
            (SeasonPriceState)(((int)Input.Season + 1) % 3), Input.Distance, Input.Event), "Next Season");
        public void ToggleDistance() => ChangeInput(new SellPriceInput(Input.BasePrice, Input.ItemType,
            Input.Season, Input.Distance == DistancePriceState.Short ? DistancePriceState.Long : DistancePriceState.Short,
            Input.Event), "Toggle Distance");
        public void ToggleEvent() => ChangeInput(new SellPriceInput(Input.BasePrice, Input.ItemType,
            Input.Season, Input.Distance, Input.Event == TradeEventState.None ? TradeEventState.Lucky : TradeEventState.None), "Toggle Event");
        public void ToggleItemType() => ChangeInput(new SellPriceInput(Input.BasePrice,
            Input.ItemType == TradeItemType.Normal ? TradeItemType.LocalSpecialty : TradeItemType.Normal,
            Input.Season, Input.Distance, Input.Event), "Toggle Item Type");
        public void Reset() => ChangeInput(new SellPriceInput(SellPriceRules.DefaultBasePrice,
            TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None), "Reset");
    }
}
