using System.Collections;
using System.Linq;
using NUnit.Framework;
using PriceModifierPipeline.Common;
using PriceModifierPipeline.Debugging;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PriceModifierPipeline.Tests
{
    public class Phase6WeatherDebugSceneTests
    {
        [UnityTest] public IEnumerator Direct720() => Validate("01_DirectCalculation", 1280, 720);
        [UnityTest] public IEnumerator Direct1080() => Validate("01_DirectCalculation", 1920, 1080);
        [UnityTest] public IEnumerator Pipeline720() => Validate("02_ModifierPipeline", 1280, 720);
        [UnityTest] public IEnumerator Pipeline1080() => Validate("02_ModifierPipeline", 1920, 1080);

        private static IEnumerator Validate(string scene, int width, int height)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + scene + ".unity");
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
            foreach (string command in new[] { "Next Season", "Toggle Distance", "Toggle Event" }) Click(panel, command);
            foreach (int item in new[] { 0, 1 })
            {
                if (item == 1) Click(panel, "Toggle Item Type");
                for (int weather = 0; weather < 3; weather++)
                {
                    Assert.That(Label(panel, "Input"), Does.Contain(((WeatherPriceState)weather).ToString()));
                    Click(panel, "Preview"); Click(panel, "Commit");
                    int expected = item == 0 ? new[] { 234, 257, 281 }[weather] : new[] { 150, 165, 180 }[weather];
                    Assert.That(Label(panel, "Results"), Does.Contain("Preview Price   " + expected)
                        .And.Contain("Commit Price   " + expected).And.Contain("PASS"));
                    Assert.That(Label(panel, "Modifiers"), Does.Contain("Weather   ON"));
                    if (item == 1) Assert.That(Label(panel, "Modifiers"), Does.Contain("Season   OFF").And.Contain("Distance   OFF"));
                    Capture(panel, camera, target, scene + "-" + item + "-" + weather);
                    Click(panel, "Next Weather");
                    Assert.That(Label(panel, "Results"), Does.Contain("N/A"));
                    Assert.That(Label(panel, "Modifiers"), Does.Contain("Weather   OFF"));
                }
            }
            panel.gameObject.SetActive(false); panel.gameObject.SetActive(true);
            Click(panel, "Next Weather");
            Assert.That(Label(panel, "Input"), Does.Contain("Rain"));
            Click(panel, "Reset");
            Assert.That(Label(panel, "Input"), Does.Contain("Clear"));
            camera.targetTexture = null;
            target.Release(); Object.Destroy(target);
            yield return new ExitPlayMode();
        }

        private static void Capture(PriceModifierDebugPanel panel, Camera camera, RenderTexture target, string state)
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
                System.IO.File.WriteAllBytes($"Logs/Phase6/{target.width}x{target.height}-{state}.png", texture.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Object.Destroy(texture); }
        }

        private static string Label(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Text>().Single(t => t.name == name).text;
        private static void Click(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(b => b.name == name).onClick.Invoke();
    }
}
