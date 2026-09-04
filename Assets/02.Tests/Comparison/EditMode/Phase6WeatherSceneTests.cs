using System.Collections;
using System.Linq;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.Comparison;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PriceModifierPipeline.Tests
{
    public class Phase6WeatherSceneTests
    {
        [UnityTest] public IEnumerator Comparison720() => Validate(1280, 720);
        [UnityTest] public IEnumerator Comparison1080() => Validate(1920, 1080);

        private static IEnumerator Validate(int width, int height)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/03_ArchitectureComparison.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceArchitectureComparisonPanel>();
            var runtime = Object.FindFirstObjectByType<ArchitectureComparisonRuntime>();
            var canvas = panel.GetComponent<Canvas>();
            var camera = Camera.main;
            var target = new RenderTexture(width, height, 24); target.Create();
            camera.targetTexture = target; camera.cullingMask = -1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            yield return null;
            Assert.That(canvas.pixelRect.width, Is.EqualTo(width));
            Assert.That(canvas.pixelRect.height, Is.EqualTo(height));
            foreach (string command in new[] { "Next Season", "Toggle Distance", "Toggle Event" }) Click(panel, command);
            for (int item = 0; item < 2; item++)
            {
                if (item == 1) Click(panel, "Toggle Item Type");
                for (int weather = 0; weather < 3; weather++)
                {
                    Assert.That(runtime.Direct.Input.Weather, Is.EqualTo((WeatherPriceState)weather));
                    Assert.That(runtime.InputSynchronized, Is.True);
                    Click(panel, "Run Both");
                    int expected = item == 0 ? new[] { 234, 257, 281 }[weather] : new[] { 150, 165, 180 }[weather];
                    var result = runtime.CaptureResult();
                    Assert.That(result.AllEquivalent, Is.True);
                    Assert.That(result.ExpectedPrice, Is.EqualTo(expected));
                    foreach (string name in new[] { "Direct", "Pipeline" })
                    {
                        var text = panel.GetComponentsInChildren<Text>().Single(t => t.name == name).text;
                        Assert.That(text, Does.Contain("Weather   ON").And.Contain("Consistency   PASS"));
                        if (item == 1) Assert.That(text, Does.Contain("Season   OFF").And.Contain("Distance   OFF").And.Contain("Event   ON"));
                    }
                    Capture(panel, camera, target, item + "-" + weather);
                    Click(panel, "Next Weather");
                    Assert.That(runtime.HasResults, Is.False);
                }
            }
            panel.gameObject.SetActive(false); panel.gameObject.SetActive(true);
            Click(panel, "Next Weather");
            Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Rain));
            Click(panel, "Reset");
            Assert.That(runtime.Direct.Input.Weather, Is.EqualTo(WeatherPriceState.Clear));
            Assert.That(runtime.InputSynchronized, Is.True);
            camera.targetTexture = null; target.Release(); Object.Destroy(target);
            yield return new ExitPlayMode();
        }

        private static void Capture(PriceArchitectureComparisonPanel panel, Camera camera, RenderTexture target, string state)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var label in panel.GetComponentsInChildren<Text>())
            {
                Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height), label.name);
                var corners = new Vector3[4]; label.rectTransform.GetWorldCorners(corners);
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
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                System.IO.Directory.CreateDirectory("Logs/Phase6");
                System.IO.File.WriteAllBytes($"Logs/Phase6/{target.width}x{target.height}-comparison-{state}.png", texture.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Object.Destroy(texture); }
        }

        private static void Click(PriceArchitectureComparisonPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(b => b.name == name).onClick.Invoke();
    }
}
