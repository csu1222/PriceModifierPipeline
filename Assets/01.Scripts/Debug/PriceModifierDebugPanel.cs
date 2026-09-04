using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace PriceModifierPipeline.Debugging
{
    public sealed class PriceModifierDebugPanel : MonoBehaviour
    {
        [SerializeField, Tooltip("Snapshot 제공 컴포넌트")]
        private MonoBehaviour sourceBehaviour;
        [SerializeField, Tooltip("명령 처리 컴포넌트")]
        private MonoBehaviour commandBehaviour;
        [SerializeField] private Text architectureText;
        [SerializeField] private Text inputText;
        [SerializeField] private Text modifiersText;
        [SerializeField] private Text resultsText;
        [SerializeField] private Text lastActionText;
        [SerializeField] private Button previewButton;
        [SerializeField] private Button commitButton;
        [SerializeField] private Button seasonButton;
        [SerializeField] private Button distanceButton;
        [SerializeField] private Button eventButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button resetButton;
        private IPriceArchitectureDebugSource source;
        private IPriceArchitectureDebugCommand command;

        private void OnEnable()
        {
            source = sourceBehaviour as IPriceArchitectureDebugSource;
            command = commandBehaviour as IPriceArchitectureDebugCommand;
            if (!sourceBehaviour || !commandBehaviour || source == null || command == null
                || !architectureText || !inputText || !modifiersText || !resultsText || !lastActionText
                || !previewButton || !commitButton || !seasonButton || !distanceButton || !eventButton || !itemButton || !resetButton)
            {
                DisableInvalid("Assign Source/Command interface implementations and all UI references in the Inspector.");
                return;
            }
            SetInteractable(true);
            previewButton.onClick.AddListener(OnPreview);
            commitButton.onClick.AddListener(OnCommit);
            seasonButton.onClick.AddListener(OnSeason);
            distanceButton.onClick.AddListener(OnDistance);
            eventButton.onClick.AddListener(OnEvent);
            itemButton.onClick.AddListener(OnItem);
            resetButton.onClick.AddListener(OnReset);
            Refresh();
        }

        private void OnDisable()
        {
            if (previewButton) previewButton.onClick.RemoveListener(OnPreview);
            if (commitButton) commitButton.onClick.RemoveListener(OnCommit);
            if (seasonButton) seasonButton.onClick.RemoveListener(OnSeason);
            if (distanceButton) distanceButton.onClick.RemoveListener(OnDistance);
            if (eventButton) eventButton.onClick.RemoveListener(OnEvent);
            if (itemButton) itemButton.onClick.RemoveListener(OnItem);
            if (resetButton) resetButton.onClick.RemoveListener(OnReset);
            SetInteractable(false);
        }

        public void Refresh()
        {
            if (!isActiveAndEnabled) return;
            if (!sourceBehaviour || !commandBehaviour || source == null || command == null)
            {
                DisableInvalid("Source or Command is missing or destroyed.");
                return;
            }
            var snapshot = source.CaptureSnapshot();
            if (snapshot == null)
            {
                DisableInvalid("Source returned a null Snapshot.");
                return;
            }
            architectureText.text = snapshot.ArchitectureName;
            inputText.text = $"Base Price   {snapshot.BasePrice}\nItem Type   {snapshot.ItemType}\nSeason   {snapshot.Season}\nDistance   {snapshot.Distance}\nEvent   {snapshot.Event}";
            modifiersText.text = $"Season   {Modifier(snapshot.SeasonApplied, snapshot.SeasonMultiplier)}\nDistance   {Modifier(snapshot.DistanceApplied, snapshot.DistanceMultiplier)}\nEvent   {Modifier(snapshot.EventApplied, snapshot.EventMultiplier)}";
            string consistency = snapshot.ConsistencyState == PriceConsistencyState.NotEvaluated ? "N/A"
                : snapshot.ConsistencyState == PriceConsistencyState.Pass ? "PASS" : "FAIL";
            resultsText.text = $"Preview Price   {(snapshot.HasPreview ? snapshot.PreviewPrice.ToString(CultureInfo.InvariantCulture) : "—")}\nCommit Price   {(snapshot.HasCommit ? snapshot.CommitPrice.ToString(CultureInfo.InvariantCulture) : "—")}\nConsistency   {consistency}";
            lastActionText.text = snapshot.LastAction;
        }

        private static string Modifier(bool applied, decimal multiplier)
            => applied ? "ON   ×" + multiplier.ToString("0.0#", CultureInfo.InvariantCulture) : "OFF";

        private void Execute(Action action)
        {
            if (!isActiveAndEnabled) return;
            if (!sourceBehaviour || !commandBehaviour)
            {
                DisableInvalid("Source or Command is missing or destroyed.");
                return;
            }
            action();
            Refresh();
        }

        private void DisableInvalid(string message)
        {
            UnityEngine.Debug.LogError("PriceModifierDebugPanel: " + message, this);
            SetInteractable(false);
            enabled = false;
        }

        private void SetInteractable(bool value)
        {
            foreach (var button in new[] { previewButton, commitButton, seasonButton, distanceButton, eventButton, itemButton, resetButton })
                if (button) button.interactable = value;
        }

        private void OnPreview() => Execute(command.Preview);
        private void OnCommit() => Execute(command.Commit);
        private void OnSeason() => Execute(command.NextSeason);
        private void OnDistance() => Execute(command.ToggleDistance);
        private void OnEvent() => Execute(command.ToggleEvent);
        private void OnItem() => Execute(command.ToggleItemType);
        private void OnReset() => Execute(command.Reset);
    }
}
