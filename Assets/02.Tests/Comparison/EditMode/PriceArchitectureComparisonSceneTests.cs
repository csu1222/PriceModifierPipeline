using System.Collections;
using System.Linq;
using NUnit.Framework;
using PriceModifierPipeline.Comparison;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PriceModifierPipeline.Tests
{
    public class PriceArchitectureComparisonSceneTests
    {
        [UnityTest]
        public IEnumerator ComparisonAt1280x720() => ValidateScene(1280, 720);

        [UnityTest]
        public IEnumerator ComparisonAt1920x1080() => ValidateScene(1920, 1080);

        private static IEnumerator ValidateScene(int width, int height)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/03_ArchitectureComparison.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceArchitectureComparisonPanel>();
            var runtime = Object.FindFirstObjectByType<ArchitectureComparisonRuntime>();
            var canvas = panel.GetComponent<Canvas>();
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
            Assert.That(Label(panel, "Comparison"), Does.Contain("Cross Option   N/A"));
            Capture(panel, camera, target, "initial");
            foreach (string command in new[] { "Next Season", "Toggle Distance", "Toggle Event" })
            {
                Click(panel, command);
                Assert.That(runtime.InputSynchronized, Is.True);
                Assert.That(Label(panel, "Direct"), Does.Contain("Consistency   N/A"));
                Click(panel, "Run Both");
                Assert.That(runtime.CaptureResult().AllEquivalent, Is.True);
            }
            AssertPrices(panel, "234");
            Capture(panel, camera, target, "normal");
            Click(panel, "Toggle Item Type"); Click(panel, "Run Both");
            AssertPrices(panel, "150");
            foreach (string name in new[] { "Direct", "Pipeline" })
                Assert.That(Label(panel, name), Does.Contain("Season   OFF").And.Contain("Distance   OFF").And.Contain("Event   ON"));
            Capture(panel, camera, target, "specialty");
            runtime.Direct.ToggleEvent(); panel.Refresh();
            Assert.That(Label(panel, "Comparison"), Does.Contain("Input Synchronized   FAIL").And.Contain("Cross Option   N/A"));
            Click(panel, "Run Both");
            Assert.That(runtime.HasResults, Is.False);
            Click(panel, "Reset");
            Assert.That(runtime.Direct.Input, Is.EqualTo(runtime.Pipeline.Input));
            Assert.That(Label(panel, "Input"), Does.Contain("Season  Normal").And.Contain("Distance  Short").And.Contain("Event  None"));
            Assert.That(Label(panel, "Comparison"), Does.Contain("Cross Option   N/A"));
            Capture(panel, camera, target, "reset");
            panel.gameObject.SetActive(false); panel.gameObject.SetActive(true);
            Click(panel, "Next Season");
            Assert.That(Label(panel, "Input"), Does.Contain("Favored"));
            camera.targetTexture = null;
            target.Release();
            Object.Destroy(target);
            yield return new ExitPlayMode();
        }

        private static void AssertPrices(PriceArchitectureComparisonPanel panel, string expected)
        {
            foreach (string name in new[] { "Direct", "Pipeline" })
                Assert.That(Label(panel, name), Does.Contain("Preview   " + expected).And.Contain("Commit    " + expected).And.Contain("Consistency   PASS"));
            Assert.That(Label(panel, "Comparison"), Does.Contain("Cross Option   PASS").And.Contain("All vs Expected   PASS"));
        }

        private static void Capture(PriceArchitectureComparisonPanel panel, Camera camera, RenderTexture target, string state)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var label in panel.GetComponentsInChildren<Text>())
            {
                Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height), label.name);
                var corners = new Vector3[4];
                label.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    var screen = RectTransformUtility.WorldToScreenPoint(camera, corner);
                    Assert.That(screen.x, Is.InRange(-1f, target.width + 1f), label.name);
                    Assert.That(screen.y, Is.InRange(-1f, target.height + 1f), label.name);
                }
            }
            camera.Render();
            var previous = RenderTexture.active;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                texture.Apply();
                System.IO.Directory.CreateDirectory("Logs/Phase5");
                System.IO.File.WriteAllBytes($"Logs/Phase5/{target.width}x{target.height}-{state}.png", texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(texture);
            }
        }

        private static string Label(PriceArchitectureComparisonPanel panel, string name)
            => panel.GetComponentsInChildren<Text>().Single(label => label.name == name).text;
        private static void Click(PriceArchitectureComparisonPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(button => button.name == name).onClick.Invoke();
    }
}
