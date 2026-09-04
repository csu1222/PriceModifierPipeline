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
    public class PriceDebugPanelRuntimeTests
    {
        [UnityTest]
        public IEnumerator SceneButtonsRenderFixturesAndRetainSingleSubscription()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/00_DebugPanelTest.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceModifierDebugPanel>();
            var runtime = Object.FindFirstObjectByType<DebugPanelTestRuntime>();
            Assert.That(panel.enabled, Is.True);
            Assert.That(Text(panel, "Architecture"), Does.Contain("UI Test Fixture"));
            Assert.That(Text(panel, "Input"), Does.Contain("Favored"));
            Assert.That(Text(panel, "Results"), Does.Contain("PASS"));
            Click(panel, "Preview");
            Assert.That(Text(panel, "Results"), Does.Contain("235").And.Contain("FAIL"));
            Click(panel, "Commit");
            Assert.That(Text(panel, "Results"), Does.Contain("PASS"));
            Click(panel, "Next Season");
            Assert.That(runtime.CaptureSnapshot().Season, Is.EqualTo(SeasonPriceState.Unfavored));
            Assert.That(Text(panel, "Results"), Does.Contain("N/A"));
            Click(panel, "Toggle Distance");
            Assert.That(Text(panel, "Input"), Does.Contain("Short"));
            Click(panel, "Toggle Event");
            Assert.That(Text(panel, "Input"), Does.Contain("None"));
            Click(panel, "Toggle Item Type");
            Assert.That(Text(panel, "Input"), Does.Contain("LocalSpecialty"));
            Assert.That(Text(panel, "Modifiers"), Does.Contain("Season   OFF").And.Contain("Distance   OFF").And.Contain("Event   ON"));
            Click(panel, "Preview");
            Assert.That(Text(panel, "Results"), Does.Contain("N/A"));
            Click(panel, "Commit");
            Assert.That(Text(panel, "Results"), Does.Contain("PASS"));
            Click(panel, "Reset");
            Assert.That(Text(panel, "Results"), Does.Contain("234").And.Contain("PASS"));
            panel.gameObject.SetActive(false);
            panel.gameObject.SetActive(true);
            Click(panel, "Next Season");
            Assert.That(runtime.CaptureSnapshot().Season, Is.EqualTo(SeasonPriceState.Unfavored));
            Click(panel, "Reset");
            Canvas.ForceUpdateCanvases();
            foreach (var label in panel.GetComponentsInChildren<Text>())
            {

                Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height), label.name);
            }
            ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("Logs/Phase2-panel.png"));
            yield return null;
            yield return null;
            Object.DestroyImmediate(runtime.gameObject);
            LogAssert.Expect(LogType.Error, "PriceModifierDebugPanel: Source or Command is missing or destroyed.");
            panel.Refresh();
            Assert.That(panel.enabled, Is.False);
            Assert.That(panel.GetComponentsInChildren<Button>().All(button => !button.interactable), Is.True);
            yield return new ExitPlayMode();
        }

        private static string Text(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Text>().Single(label => label.name == name).text;

        private static void Click(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(button => button.name == name).onClick.Invoke();
    }
}
