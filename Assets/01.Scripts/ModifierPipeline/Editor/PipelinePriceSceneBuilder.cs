using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PriceModifierPipeline.Debugging;

namespace PriceModifierPipeline.ModifierPipeline.Editor
{
    public static class PipelinePriceSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/02_ModifierPipeline.unity";

        public static void BuildBatch()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Batch mode required.");
            EditorSceneManager.OpenScene("Assets/Scenes/00_DebugPanelTest.unity");
            Build();
        }

        [MenuItem("Tools/Price Modifier/Create Modifier Pipeline Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
                throw new InvalidOperationException("Pipeline scene already exists; refusing overwrite.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03.Prefabs/PriceModifierDebugPanel.prefab");
            if (!prefab) throw new InvalidOperationException("Common panel prefab is missing.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
                Undo.RegisterCreatedObjectUndo(camera.gameObject, "Create Pipeline camera");
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
                camera.cullingMask = 0;
                var runtime = new GameObject("PipelinePriceRuntime");
                Undo.RegisterCreatedObjectUndo(runtime, "Create Pipeline runtime");
                runtime.AddComponent<PipelinePriceRuntime>();
                var source = runtime.AddComponent<PipelinePriceDebugSource>();
                var command = runtime.AddComponent<PipelinePriceDebugCommand>();
                var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvas, "Create Pipeline canvas");
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
                var panel = instance.GetComponent<PriceModifierDebugPanel>();
                var serialized = new SerializedObject(panel);
                serialized.FindProperty("sourceBehaviour").objectReferenceValue = source;
                serialized.FindProperty("commandBehaviour").objectReferenceValue = command;
                serialized.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(panel);
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(events, "Create Pipeline EventSystem");
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Scene save failed.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            }
        }
    }
}
