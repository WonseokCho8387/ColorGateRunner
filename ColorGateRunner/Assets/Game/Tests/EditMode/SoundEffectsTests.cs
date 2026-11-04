using System;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class SoundEffectsTests
    {
        private const string FrontendScenePath = "Assets/Scenes/Frontend.unity";
        private const string CampaignScenePath = "Assets/Scenes/SampleScene.unity";

        [TestCase("HomeNavigationButton", "", SoundCue.UiTab)]
        [TestCase("ConfirmResetProgressButton", "", SoundCue.UiDanger)]
        [TestCase("FailureExitConfirmButton", "", SoundCue.UiDanger)]
        [TestCase("StartItemPurchaseCancelButton", "", SoundCue.UiBack)]
        [TestCase("ClearLobbyButton", "", SoundCue.UiBack)]
        [TestCase("ModalCancelButton", "", SoundCue.UiBack)]
        [TestCase("LobbySettingsButton", "", SoundCue.UiTap)]
        [TestCase("CoinContinueButton", "", SoundCue.UiPurchase)]
        [TestCase("StartItemPurchaseConfirmButton", "", SoundCue.UiPurchase)]
        [TestCase("LobbyPlayCampaignButton", "", SoundCue.UiConfirm)]
        [TestCase("ReplayButton", "", SoundCue.UiConfirm)]
        [TestCase("StageButton_04", "ButtonPrimary", SoundCue.UiConfirm)]
        [TestCase("StageButton_05", "ButtonSecondary", SoundCue.UiTap)]
        [TestCase("Anything", "ButtonDanger", SoundCue.UiDanger)]
        public void CueFor_PicksTheCueFromControlNameAndSurface(
            string name,
            string sprite,
            SoundCue expected)
        {
            Assert.That(SoundLibraryBuilder.CueFor(name, sprite, false),
                Is.EqualTo(expected));
        }

        [Test]
        public void SoundLibrary_HasEveryCueAndALoopingLobbyTrack()
        {
            SoundLibraryAsset library =
                AssetDatabase.LoadAssetAtPath<SoundLibraryAsset>(
                    SoundLibraryBuilder.AssetPath);
            Assert.That(library, Is.Not.Null);
            Assert.That(library.HasAllClips, Is.True);
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                AudioClip clip = library.GetClip(cue);
                Assert.That(AssetDatabase.GetAssetPath(clip),
                    Is.EqualTo(SoundLibraryBuilder.GetCuePath(cue)));
                Assert.That(clip.length, Is.LessThan(1.5f), cue.ToString());
                var importer = (AudioImporter)AssetImporter.GetAtPath(
                    SoundLibraryBuilder.GetCuePath(cue));
                Assert.That(importer.defaultSampleSettings.loadType,
                    Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                Assert.That(importer.defaultSampleSettings.preloadAudioData,
                    Is.True);
            }

            Assert.That(library.LobbyMusic.length,
                Is.EqualTo(32f * 4f * 60f / 84f).Within(0.05f));
            var lobby = (AudioImporter)AssetImporter.GetAtPath(
                SoundLibraryBuilder.LobbyMusicPath);
            Assert.That(lobby.defaultSampleSettings.loadType,
                Is.EqualTo(AudioClipLoadType.Streaming));
            Assert.That(lobby.defaultSampleSettings.preloadAudioData,
                Is.False);
        }

        [Test]
        public void Scenes_GiveEveryButtonAndToggleAClickCue()
        {
            foreach (string path in new[] { FrontendScenePath, CampaignScenePath })
            {
                Scene scene = EditorSceneManager.OpenScene(
                    path,
                    OpenSceneMode.Single);
                SoundEffectPlayer[] players =
                    UnityEngine.Object.FindObjectsByType<SoundEffectPlayer>(
                        FindObjectsInactive.Include);
                Assert.That(players.Length, Is.EqualTo(1), path);
                Assert.That(players[0].HasRequiredReferences, Is.True, path);
                Assert.That(UnityEngine.Object.FindObjectsByType<AudioListener>(
                        FindObjectsInactive.Include).Length,
                    Is.EqualTo(1), path);

                Selectable[] controls =
                    UnityEngine.Object.FindObjectsByType<Selectable>(
                        FindObjectsInactive.Include);
                int checkedControls = 0;
                for (int index = 0; index < controls.Length; index++)
                {
                    if (!(controls[index] is Button) &&
                        !(controls[index] is Toggle))
                    {
                        continue;
                    }
                    UiButtonSound sound =
                        controls[index].GetComponent<UiButtonSound>();
                    Assert.That(sound, Is.Not.Null,
                        $"{path} {controls[index].name}");
                    Assert.That(sound.Player, Is.SameAs(players[0]),
                        $"{path} {controls[index].name}");
                    checkedControls++;
                }
                Assert.That(checkedControls, Is.GreaterThan(50), path);
                Assert.That(scene.IsValid(), Is.True);
            }

            EditorSceneManager.OpenScene(FrontendScenePath, OpenSceneMode.Single);
            LobbyMusicPlayer[] music =
                UnityEngine.Object.FindObjectsByType<LobbyMusicPlayer>(
                    FindObjectsInactive.Include);
            Assert.That(music.Length, Is.EqualTo(1));
            Assert.That(music[0].HasRequiredReferences, Is.True);
            Assert.That(music[0].Source.playOnAwake, Is.False);
        }
    }
}
