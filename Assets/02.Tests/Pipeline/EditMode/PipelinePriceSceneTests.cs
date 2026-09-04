using System.Collections;
using System.Linq;
using NUnit.Framework;
using PriceModifierPipeline.Debugging;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PriceModifierPipeline.Tests
{
    public class PipelinePriceSceneTests
    {
        [UnityTest]
        public IEnumerator SceneButtonsCalculateAndReset()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/02_ModifierPipeline.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceModifierDebugPanel>();
            Assert.That(panel.enabled, Is.True);
            Assert.That(Label(panel, "Architecture"), Is.EqualTo("Modifier Pipeline"));
            Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
            Click(panel, "Preview");
            Assert.That(Label(panel, "Results"), Does.Contain("100").And.Contain("N/A"));
            Click(panel, "Commit");
            Assert.That(Label(panel, "Results"), Does.Contain("PASS"));
            foreach (string button in new[] { "Next Season", "Toggle Distance", "Toggle Event" })
            {
                Click(panel, button);
                Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
                Click(panel, "Preview"); Click(panel, "Commit");
                Assert.That(Label(panel, "Results"), Does.Contain("PASS"));
            }
            Assert.That(Label(panel, "Results"), Does.Contain("234"));
            Click(panel, "Toggle Item Type");
            Click(panel, "Commit"); Click(panel, "Preview");
            Assert.That(Label(panel, "Results"), Does.Contain("150").And.Contain("PASS"));
            Assert.That(Label(panel, "Modifiers"), Does.Contain("Season   OFF").And.Contain("Distance   OFF").And.Contain("Event   ON"));
            Canvas.ForceUpdateCanvases();
            foreach (var label in panel.GetComponentsInChildren<Text>())
                Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height), label.name);
            Click(panel, "Reset");
            Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
            panel.gameObject.SetActive(false); panel.gameObject.SetActive(true);
            Click(panel, "Next Season");
            Assert.That(Label(panel, "Input"), Does.Contain("Favored"));
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator PanelAt1280x720() => ValidateResolution(1280, 720);

        [UnityTest]
        public IEnumerator PanelAt1920x1080() => ValidateResolution(1920, 1080);

        private static IEnumerator ValidateResolution(int width, int height)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/02_ModifierPipeline.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceModifierDebugPanel>();
            var canvas = panel.GetComponentInParent<Canvas>();
            var camera = Camera.main;
            var target = new RenderTexture(width, height, 24);
            target.Create();
            camera.targetTexture = target;
            camera.cullingMask = -1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            yield return null;
            Assert.That(canvas.pixelRect.width, Is.EqualTo(width));
            Assert.That(canvas.pixelRect.height, Is.EqualTo(height));
            Assert.That(Label(panel, "Architecture"), Is.EqualTo("Modifier Pipeline"));
            Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
            Capture(panel, camera, target, "initial");
            Click(panel, "Preview"); Click(panel, "Commit");
            Assert.That(Label(panel, "Results"), Does.Contain("100").And.Contain("PASS"));
            Click(panel, "Next Season"); Click(panel, "Toggle Distance"); Click(panel, "Toggle Event");
            Click(panel, "Preview"); Click(panel, "Commit");
            Assert.That(Label(panel, "Results"), Does.Contain("234").And.Contain("PASS"));
            Capture(panel, camera, target, "normal");
            Click(panel, "Toggle Item Type"); Click(panel, "Preview"); Click(panel, "Commit");
            Assert.That(Label(panel, "Results"), Does.Contain("150").And.Contain("PASS"));
            Assert.That(Label(panel, "Modifiers"), Does.Contain("Season   OFF").And.Contain("Distance   OFF").And.Contain("Event   ON"));
            Capture(panel, camera, target, "specialty");
            Click(panel, "Reset");
            Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
            Capture(panel, camera, target, "reset");
            camera.targetTexture = null;
            target.Release();
            Object.Destroy(target);
            yield return new ExitPlayMode();
        }

        private static void Capture(PriceModifierDebugPanel panel, Camera camera, RenderTexture target, string state)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var label in panel.GetComponentsInChildren<Text>())
                Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height), label.name);
            camera.Render();
            var previous = RenderTexture.active;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                texture.Apply();
                System.IO.Directory.CreateDirectory("Logs/Phase4");
                System.IO.File.WriteAllBytes($"Logs/Phase4/{target.width}x{target.height}-{state}.png", texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(texture);
            }
        }

        private static string Label(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Text>().Single(label => label.name == name).text;
        private static void Click(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(button => button.name == name).onClick.Invoke();
    }
}
