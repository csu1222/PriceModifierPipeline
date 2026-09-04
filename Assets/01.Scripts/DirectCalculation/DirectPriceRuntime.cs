using PriceModifierPipeline.Common;
using UnityEngine;

namespace PriceModifierPipeline.DirectCalculation
{
    public sealed class DirectPriceRuntime : MonoBehaviour
    {
        private readonly DirectPreviewController previewController = new DirectPreviewController();
        private readonly DirectCommitController commitController = new DirectCommitController();
        public SellPriceInput Input { get; private set; } = new SellPriceInput(SellPriceRules.DefaultBasePrice,
            TradeItemType.Normal, SeasonPriceState.Normal, DistancePriceState.Short, TradeEventState.None);
        public DirectPriceResult PreviewResult { get; private set; }
        public DirectPriceResult CommitResult { get; private set; }
        public DirectPriceResult LatestResult { get; private set; }
        public bool HasPreview { get; private set; }
        public bool HasCommit { get; private set; }
        public string LastAction { get; private set; } = "Ready (no calculation)";

        public void Preview()
        {
            PreviewResult = previewController.CalculatePreview(Input);
            LatestResult = PreviewResult;
            HasPreview = true;
            LastAction = "Preview";
        }

        public void Commit()
        {
            CommitResult = commitController.CalculateCommit(Input);
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
