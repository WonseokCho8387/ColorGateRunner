using ColorGateRunner.Core;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class RivalRaceSceneTests
    {
        [Test]
        public void CampaignScene_HasHiddenRivalsAndComboHudWithoutRank()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GrayboxSceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                RivalRaceView view = null;
                int shellCount = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    RivalRaceView found =
                        root.GetComponentInChildren<RivalRaceView>(true);
                    if (found != null)
                    {
                        Assert.That(view, Is.Null, "Duplicate RivalRaceView.");
                        view = found;
                    }
                    foreach (Transform child in
                        root.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == "ColorShell")
                        {
                            shellCount++;
                        }
                    }
                }

                Assert.That(view, Is.Not.Null);
                Assert.That(view.HasRequiredReferences, Is.True);
                Assert.That(view.gameObject.activeSelf, Is.False);
                Assert.That(shellCount, Is.EqualTo(1));
                ComboHudView combo = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    combo = combo ?? root.GetComponentInChildren<ComboHudView>(true);
                    foreach (Text text in root.GetComponentsInChildren<Text>(true))
                    {
                        Assert.That(text.name, Is.Not.EqualTo("RaceRankText"));
                    }
                }
                Assert.That(combo, Is.Not.Null);
                Assert.That(combo.HasRequiredReferences, Is.True);
                Assert.That(combo.ComboText.transform.parent.name,
                    Is.EqualTo("StageHud"));
                Assert.That(combo.ComboText.gameObject.activeSelf, Is.False);
                Assert.That(combo.JudgmentRoot.activeSelf, Is.False);
                Assert.That(combo.JudgmentRoot.name, Is.EqualTo("GateJudgmentFlash"));
                Assert.That(combo.JudgmentText.fontSize, Is.GreaterThanOrEqualTo(96));

                for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
                {
                    foreach (Transform part in
                        view.GetRunner(rival).GetComponentsInChildren<Transform>(true))
                    {
                        bool formPart =
                            part.name.StartsWith("Rival_Power") ||
                            part.name.StartsWith("Rival_Stable") ||
                            part.name.StartsWith("Rival_Wing");
                        if (formPart)
                        {
                            Assert.That(part.gameObject.activeSelf, Is.False,
                                part.name);
                        }
                    }
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
