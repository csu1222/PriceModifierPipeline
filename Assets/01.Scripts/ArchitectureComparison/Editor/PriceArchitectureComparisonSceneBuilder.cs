using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PriceModifierPipeline.Comparison.Editor
{
    public static class PriceArchitectureComparisonSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/03_ArchitectureComparison.unity";

        public static void BuildBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Batch mode required.");
            EditorSceneManager.OpenScene("Assets/Scenes/00_DebugPanelTest.unity");
            Build();
        }

        [MenuItem("Tools/Price Modifier/Create Architecture Comparison Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
                throw new InvalidOperationException("Comparison scene already exists; refusing overwrite.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
                Undo.RegisterCreatedObjectUndo(camera.gameObject, "Create comparison camera");
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
                camera.cullingMask = 0;
                var runtime = new GameObject("ArchitectureComparisonRuntime").AddComponent<ArchitectureComparisonRuntime>();
                Undo.RegisterCreatedObjectUndo(runtime.gameObject, "Create comparison runtime");
                var canvas = new GameObject("ComparisonCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvas, "Create comparison canvas");
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
                var panel = canvas.AddComponent<PriceArchitectureComparisonPanel>();
                var title = Label(canvas.transform, "Title", "PRICE ARCHITECTURE COMPARISON", 40, 24, 1200, 60);
                title.fontSize = 32;
                var input = Label(canvas.transform, "Input", "", 40, 100, 1200, 90);
                var direct = Label(canvas.transform, "Direct", "", 40, 200, 580, 270);
                var pipeline = Label(canvas.transform, "Pipeline", "", 660, 200, 580, 270);
                var comparison = Label(canvas.transform, "Comparison", "", 40, 488, 1200, 85);
                var serialized = new SerializedObject(panel);
                serialized.FindProperty("runtime").objectReferenceValue = runtime;
                serialized.FindProperty("input").objectReferenceValue = input;
                serialized.FindProperty("direct").objectReferenceValue = direct;
                serialized.FindProperty("pipeline").objectReferenceValue = pipeline;
                serialized.FindProperty("comparison").objectReferenceValue = comparison;
                serialized.ApplyModifiedProperties();
                string[] names = { "Next Season", "Toggle Distance", "Toggle Event", "Toggle Item Type", "Run Both", "Reset", "Next Weather" };
                UnityAction[] actions = { panel.NextSeason, panel.ToggleDistance, panel.ToggleEvent, panel.ToggleItemType, panel.RunBoth, panel.Reset, panel.NextWeather };
                for (int i = 0; i < names.Length; i++)
                {
                    var button = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button));
                    button.transform.SetParent(canvas.transform, false);
                    Place(button.GetComponent<RectTransform>(), 40 + i * 173, 618, 162, 58);
                    button.GetComponent<Image>().color = new Color(0.12f, 0.26f, 0.36f);
                    var label = Label(button.transform, "Caption", names[i], 0, 0, 162, 58);
                    label.alignment = TextAnchor.MiddleCenter;
                    label.fontSize = 17;
                    UnityEventTools.AddPersistentListener(button.GetComponent<Button>().onClick, actions[i]);
                }
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(events, "Create comparison EventSystem");
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                panel.Refresh();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Scene save failed.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            }
        }

        private static Text Label(Transform parent, string name, string content, float x, float y, float width, float height)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.transform.SetParent(parent, false);
            Place(label.rectTransform, x, y, width, height);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 22;
            label.color = new Color(0.9f, 0.95f, 1);
            label.text = content;
            label.raycastTarget = false;
            return label;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
