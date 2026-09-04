using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PriceModifierPipeline.Debugging.Editor
{
    public static class PriceDebugPanelBuilder
    {
        public const string ScenePath = "Assets/Scenes/00_DebugPanelTest.unity";
        public const string PrefabPath = "Assets/03.Prefabs/PriceModifierDebugPanel.prefab";
        private static Font font;

        public static void BuildBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("BuildBatch is for isolated batch validation only.");
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Build();
        }

        public static void RepairCameraBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("RepairCameraBatch is for isolated batch validation only.");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Camera camera = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                camera = root.GetComponentInChildren<Camera>(true);
                if (camera) break;
            }
            if (!camera) camera = CreateCamera();
            if (!camera.isActiveAndEnabled || camera.targetTexture || camera.targetDisplay != 0)
                throw new InvalidOperationException("The test camera must render to the main display.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save the test scene camera.");
            UnityEngine.Debug.Log("Debug Panel camera verified: active, enabled, main display, no target texture.");
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera));
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Debug Panel Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
            // Overlay UI는 Canvas가 그리며, Camera는 GameView의 배경을 렌더링한다.
            camera.cullingMask = 0;
            return camera;
        }

        [MenuItem("Tools/Price Modifier/Create Debug Panel Test Assets")]
        public static void Build()
        {
            // 기존 asset과 열려 있는 Scene의 미저장 상태를 보존한다.
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
                throw new InvalidOperationException("Debug assets already exist. Builder does not overwrite existing assets.");
            if (!AssetDatabase.IsValidFolder("Assets/03.Prefabs")) AssetDatabase.CreateFolder("Assets", "03.Prefabs");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                CreateCamera();
                var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
                var root = Box("PriceModifierDebugPanel", canvasObject.transform, 0, 0, 1280, 720, new Color(0.035f, 0.055f, 0.09f));
                root.SetActive(false);
                var panel = root.AddComponent<PriceModifierDebugPanel>();
                Label("Title", root.transform, "PRICE MODIFIER / ARCHITECTURE DEMO", 40, 26, 1200, 44, 30);
                var architecture = Label("Architecture", root.transform, "Architecture", 40, 78, 1200, 36, 23);
                var inputCard = Box("InputCard", root.transform, 40, 140, 380, 285, new Color(0.07f, 0.1f, 0.15f));
                Label("Heading", inputCard.transform, "INPUT", 22, 16, 330, 30, 20);
                var input = Label("Input", inputCard.transform, "", 22, 60, 340, 215, 24);
                var modifierCard = Box("ModifierCard", root.transform, 442, 140, 386, 285, new Color(0.07f, 0.1f, 0.15f));
                Label("Heading", modifierCard.transform, "APPLIED MODIFIERS", 22, 16, 345, 30, 20);
                var modifiers = Label("Modifiers", modifierCard.transform, "", 22, 60, 345, 215, 24);
                var resultCard = Box("ResultCard", root.transform, 850, 140, 390, 285, new Color(0.07f, 0.1f, 0.15f));
                Label("Heading", resultCard.transform, "RESULT", 22, 16, 340, 30, 20);
                var results = Label("Results", resultCard.transform, "", 22, 60, 345, 215, 24);
                Label("CommandsHeading", root.transform, "COMMANDS", 40, 449, 1200, 30, 20);
                var serialized = new SerializedObject(panel);
                Assign(serialized, "architectureText", architecture);
                Assign(serialized, "inputText", input);
                Assign(serialized, "modifiersText", modifiers);
                Assign(serialized, "resultsText", results);
                string[] fields = { "previewButton", "commitButton", "seasonButton", "distanceButton", "eventButton", "itemButton", "resetButton" };
                string[] titles = { "Preview", "Commit", "Next Season", "Toggle Distance", "Toggle Event", "Toggle Item Type", "Reset" };
                for (int i = 0; i < fields.Length; i++)
                {
                    int row = i < 4 ? 0 : 1;
                    int column = i < 4 ? i : i - 4;
                    var buttonObject = Box(titles[i], root.transform, 40 + column * 305, 494 + row * 66, 285, 52, new Color(0.12f, 0.28f, 0.38f));
                    var button = buttonObject.AddComponent<Button>();
                    button.targetGraphic = buttonObject.GetComponent<Image>();
                    var label = Label("Label", buttonObject.transform, titles[i], 0, 0, 285, 52, 21);
                    label.alignment = TextAnchor.MiddleCenter;
                    Assign(serialized, fields[i], button);
                }
                Assign(serialized, "lastActionText", Label("LastAction", root.transform, "", 40, 632, 1200, 30, 19));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                root.SetActive(true);
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
                var runtime = new GameObject("DebugPanelTestRuntime").AddComponent<DebugPanelTestRuntime>();
                serialized.Update();
                Assign(serialized, "sourceBehaviour", runtime);
                Assign(serialized, "commandBehaviour", runtime);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            }
            File.WriteAllText("Logs/Phase2-assets.txt", "Created " + PrefabPath + " and " + ScenePath);
        }

        private static void Assign(SerializedObject target, string property, UnityEngine.Object value)
            => target.FindProperty(property).objectReferenceValue = value;

        private static GameObject Box(string name, Transform parent, float x, float y, float width, float height, Color color)
        {
            var result = new GameObject(name, typeof(RectTransform), typeof(Image));
            Position(result, parent, x, y, width, height);
            result.GetComponent<Image>().color = color;
            return result;
        }

        private static Text Label(string name, Transform parent, string value, float x, float y, float width, float height, int size)
        {
            var result = new GameObject(name, typeof(RectTransform), typeof(Text));
            Position(result, parent, x, y, width, height);
            var label = result.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.color = new Color(0.87f, 0.93f, 0.98f);
            label.text = value;
            label.supportRichText = false;
            label.raycastTarget = false;
            return label;
        }

        private static void Position(GameObject target, Transform parent, float x, float y, float width, float height)
        {
            target.transform.SetParent(parent, false);
            var rect = (RectTransform)target.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
