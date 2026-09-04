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
    public class DirectPriceSceneTests
    {
        [UnityTest]
        public IEnumerator SceneButtonsCalculateAndReset()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/01_DirectCalculation.unity");
            yield return new EnterPlayMode();
            var panel = Object.FindFirstObjectByType<PriceModifierDebugPanel>();
            Assert.That(panel.enabled, Is.True);
            Assert.That(Label(panel, "Architecture"), Is.EqualTo("Direct Calculation"));
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

        private static string Label(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Text>().Single(label => label.name == name).text;
        private static void Click(PriceModifierDebugPanel panel, string name)
            => panel.GetComponentsInChildren<Button>().Single(button => button.name == name).onClick.Invoke();
    }
}
