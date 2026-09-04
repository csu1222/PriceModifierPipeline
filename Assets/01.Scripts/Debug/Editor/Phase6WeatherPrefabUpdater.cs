using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PriceModifierPipeline.Debugging.Editor
{
    public static class Phase6WeatherPrefabUpdater
    {
        public static void UpdateBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Batch mode required.");
            var root = PrefabUtility.LoadPrefabContents(PriceDebugPanelBuilder.PrefabPath);
            try
            {
                var panel = root.GetComponent<PriceModifierDebugPanel>();
                var serialized = new SerializedObject(panel);
                if (!serialized.FindProperty("weatherButton").objectReferenceValue)
                {
                    var template = root.GetComponentsInChildren<Button>(true).Single(b => b.name == "Reset");
                    var button = UnityEngine.Object.Instantiate(template, template.transform.parent);
                    button.name = "Next Weather";
                    Undo.RegisterCreatedObjectUndo(button.gameObject, "Add Weather command");
                    button.onClick = new Button.ButtonClickedEvent();
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(955, -560);
                    button.GetComponentInChildren<Text>().text = "Next Weather";
                    serialized.FindProperty("weatherButton").objectReferenceValue = button;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(panel);
                }
                PrefabUtility.SaveAsPrefabAsset(root, PriceDebugPanelBuilder.PrefabPath, out bool saved);
                if (!saved) throw new InvalidOperationException("Weather prefab save failed.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("Phase6 Weather prefab updated; existing scenes inherit the added button.");
        }
    }
}
