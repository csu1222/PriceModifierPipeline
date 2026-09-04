using PriceModifierPipeline.Common;
using UnityEngine;

namespace PriceModifierPipeline.Debugging
{
    // UI 검증 전용 fixture. 실제 가격 계산이나 Option Runtime으로 재사용하지 않는다.
    public sealed class DebugPanelTestRuntime : MonoBehaviour, IPriceArchitectureDebugSource, IPriceArchitectureDebugCommand
    {
        private SeasonPriceState season = SeasonPriceState.Favored;
        private DistancePriceState distance = DistancePriceState.Long;
        private TradeEventState tradeEvent = TradeEventState.Lucky;
        private TradeItemType itemType;
        private bool hasPreview = true;
        private bool hasCommit = true;
        private int preview = 234;
        private int commit = 234;
        private string lastAction = "Initial fixture";

        public PriceDebugSnapshot CaptureSnapshot()
        {
            bool normal = itemType == TradeItemType.Normal;
            return new PriceDebugSnapshot("UI Test Fixture (no calculation)",
                new SellPriceInput(100, itemType, season, distance, tradeEvent),
                season == SeasonPriceState.Favored ? 1.2m : season == SeasonPriceState.Unfavored ? 0.8m : 1m,
                distance == DistancePriceState.Long ? 1.3m : 1m,
                tradeEvent == TradeEventState.Lucky ? 1.5m : 1m,
                normal, normal, true, preview, commit, hasPreview, hasCommit, lastAction);
        }

        // 두 번째 Preview는 의도적 불일치 fixture. Commit으로 다시 PASS를 검증한다.
        public void Preview()
        {
            preview = hasPreview && preview == 234 ? 235 : 234;
            hasPreview = true;
            lastAction = "Preview fixture (234 / 235)";
        }

        public void Commit()
        {
            commit = hasPreview ? preview : 234;
            hasCommit = true;
            lastAction = "Commit fixture";
        }

        public void NextSeason()
        {
            season = (SeasonPriceState)(((int)season + 1) % 3);
            ClearResults("Next Season");
        }

        public void ToggleDistance()
        {
            distance = distance == DistancePriceState.Short ? DistancePriceState.Long : DistancePriceState.Short;
            ClearResults("Toggle Distance");
        }

        public void ToggleEvent()
        {
            tradeEvent = tradeEvent == TradeEventState.None ? TradeEventState.Lucky : TradeEventState.None;
            ClearResults("Toggle Event");
        }

        public void ToggleItemType()
        {
            itemType = itemType == TradeItemType.Normal ? TradeItemType.LocalSpecialty : TradeItemType.Normal;
            ClearResults("Toggle Item Type");
        }

        public void Reset()
        {
            season = SeasonPriceState.Favored;
            distance = DistancePriceState.Long;
            tradeEvent = TradeEventState.Lucky;
            itemType = TradeItemType.Normal;
            preview = commit = 234;
            hasPreview = hasCommit = true;
            lastAction = "Reset fixture";
        }

        private void ClearResults(string action)
        {
            hasPreview = hasCommit = false;
            lastAction = action + " (results cleared)";
        }
    }
}
