using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PriceModifierPipeline.Comparison.Editor
{
    public static class Phase6WeatherSceneUpdater
    {
        public static void UpdateBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Batch mode required.");
            var scene = EditorSceneManager.OpenScene(PriceArchitectureComparisonSceneBuilder.ScenePath);
            var panel = UnityEngine.Object.FindFirstObjectByType<PriceArchitectureComparisonPanel>();
            var buttons = panel.GetComponentsInChildren<Button>();
            if (!buttons.Any(b => b.name == "Next Weather"))
            {
                var template = buttons.Single(b => b.name == "Reset");
                var weather = UnityEngine.Object.Instantiate(template, template.transform.parent);
                weather.name = "Next Weather";
                Undo.RegisterCreatedObjectUndo(weather.gameObject, "Add comparison Weather command");
                weather.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(weather.onClick, panel.NextWeather);
                weather.GetComponentInChildren<Text>().text = "Next Weather";
            }
            string[] names = { "Next Season", "Toggle Distance", "Toggle Event", "Toggle Item Type", "Run Both", "Reset", "Next Weather" };
            buttons = panel.GetComponentsInChildren<Button>();
            for (int i = 0; i < names.Length; i++)
            {
                var button = buttons.Single(b => b.name == names[i]);
                var rect = button.GetComponent<RectTransform>();
                Undo.RecordObject(rect, "Fit seven comparison buttons");
                rect.anchoredPosition = new Vector2(40 + i * 173, -618);
                rect.sizeDelta = new Vector2(162, 58);
                var label = button.GetComponentInChildren<Text>();
                Undo.RecordObject(label.rectTransform, "Fit Weather command caption");
                Undo.RecordObject(label, "Fit Weather command caption");
                label.rectTransform.sizeDelta = new Vector2(162, 58);
                label.fontSize = 17;
            }
            panel.Refresh();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Weather scene save failed.");
            Debug.Log("Phase6 Weather comparison scene updated.");
        }
    }
}
