using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class PlaytestFlowPlayModeTests
    {
        private AppRoot _root;
        private StageSceneController _controller;
        private CampaignPlayerPrefsSnapshot _snapshot;
        private bool[] _laterExists;
        private string[] _laterValues;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _snapshot = CampaignPlayerPrefsSnapshot.Capture();
            _laterExists = new bool[6];
            _laterValues = new string[6];
            for (int i = 0; i < 6; i++)
            {
                string key = "ColorGateRunner.Stage.Record." + (21 + i);
                _laterExists[i] = PlayerPrefs.HasKey(key);
                _laterValues[i] = PlayerPrefs.GetString(key, string.Empty);
            }
            foreach (AppRoot root in Object.FindObjectsByType<AppRoot>())
                Object.DestroyImmediate(root.gameObject);
            AppRoot.ClearTestState();
            AppRoot.SetTestGraphFactory(null, true);
            _root = new GameObject("PlaytestTestRoot").AddComponent<AppRoot>();
            Assert.That(_root.Initialize().Succeeded, Is.True);
            Assert.That(_root.TryQueueCampaignLaunch("stage-01"), Is.True);
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/SampleScene.unity");
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (_root != null) Object.DestroyImmediate(_root.gameObject);
                AppRoot.ClearTestState();
            }
            finally
            {
                _snapshot?.Restore();
                if (_laterExists != null)
                    for (int i = 0; i < 6; i++)
                    {
                        string key = "ColorGateRunner.Stage.Record." + (21 + i);
                        if (_laterExists[i]) PlayerPrefs.SetString(key, _laterValues[i]);
                        else PlayerPrefs.DeleteKey(key);
                    }
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void FailureRetry_IsOneAction_Unlimited_WithoutProductMutation()
        {
            int hearts = _root.Graph.Progression.Economy.HeartCount;
            for (int retry = 0; retry < 8; retry++)
            {
                Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Countdown));
                _controller.Tick(3.1f);
                StageGateView gate = _controller.GetGate(0);
                if (_controller.Session.CurrentColor == gate.AssignedColor)
                    _controller.HandleGameplayTap();
                Assert.That(_controller.HandleGateCrossed(gate), Is.EqualTo(GateOutcome.Mismatched));
                _controller.Tick(1.1f);
                Assert.That(_controller.FailDetailsText.text, Does.Contain("COLOR MISMATCH"));
                Assert.That(_controller.CoinContinueButton.gameObject.activeSelf, Is.False);
                _controller.RetryButton.onClick.Invoke();
                Assert.That(_root.Playtest.RetryCount, Is.EqualTo(retry + 1));
                Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Countdown));
            }
            Assert.That(_root.Graph.Progression.Economy.HeartCount, Is.EqualTo(hearts));
            AssertCampaignPrefsUnchanged();
        }

        [UnityTest]
        public IEnumerator Frontend_OnePlayActionStartsRoute_WithoutAccountOrItems()
        {
            Assert.That(_root.TryQueueCampaignLaunch("stage-26"), Is.False);
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/Frontend.unity");
            yield return null;
            FrontendSceneController frontend = Object.FindAnyObjectByType<FrontendSceneController>();
            PlaytestEntryView entry = Object.FindAnyObjectByType<PlaytestEntryView>();
            Assert.That(entry, Is.Not.Null);
            Assert.That(frontend.TitlePageRoot.activeInHierarchy, Is.False);
            Assert.That(frontend.LobbyPageRoot.activeInHierarchy, Is.False);
            Assert.That(_root.AccountChoiceCompleted, Is.False);
            entry.GetComponentInChildren<Button>().onClick.Invoke();
            float deadline = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity" &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.Session.Stage.StageId, Is.EqualTo("stage-01"));
            Text guide = _controller.GetComponentInChildren<Text>(true);
            foreach (Text text in _controller.GetComponentsInChildren<Text>(true))
                if (text.name == "PlaytestGuide") guide = text;
            Assert.That(guide.name, Is.EqualTo("PlaytestGuide"));
            Assert.That(guide.gameObject.activeInHierarchy, Is.True);
            Assert.That(guide.raycastTarget, Is.False);
            Assert.That(_root.AccountChoiceCompleted, Is.False);
            AssertCampaignPrefsUnchanged();
        }

        [UnityTest]
        public IEnumerator FullRoute_ReusesStageRules_AndNeverUnlocksProductionProgress()
        {
            string[] ids = { "stage-01", "stage-02", "stage-03", "stage-06",
                "stage-09", "stage-21", "stage-18", "stage-24" };
            foreach (string id in ids)
            {
                Assert.That(_controller.Session.Stage.StageId, Is.EqualTo(id));
                _controller.Tick(3.1f);
                Assert.That(_controller.Session.StageProvidesShield, Is.EqualTo(id == "stage-06"));
                int guard = 0;
                while (_controller.Session.RemainingGates > 0 && guard++ < 60)
                {
                    StageGateView gate = _controller.GetGate((guard - 1) % 6);
                    RunnerColor color = gate.ActivePlan.Modifier.IsFlicker
                        ? _controller.Session.UpdateFlickerGate(gate.ActivePlan, 0f, 0f).CurrentColor
                        : gate.AssignedColor;
                    for (int taps = 0; taps < 3 && _controller.Session.CurrentColor != color; taps++)
                        _controller.HandleGameplayTap();
                    Assert.That(_controller.HandleGateCrossed(gate), Is.Not.EqualTo(GateOutcome.Mismatched));
                }
                Assert.That(_controller.Session.RemainingGates, Is.Zero);
                float remaining = _controller.GoalPathDistance - _controller.CampaignDistance + 1f;
                _controller.TickMovement(remaining / _controller.Session.CurrentSpeed);
                Assert.That(_root.Playtest.StageCompleted, Is.True);
                _controller.Tick(1.3f);
                Assert.That(_controller.ClearContinueButton.gameObject.activeInHierarchy, Is.False);
                _controller.Tick(2.7f);
                _controller.Tick(0.4f);
                Assert.That(_controller.ClearContinueButton.gameObject.activeInHierarchy, Is.True);
                Assert.That(_controller.ClearContinueButton.IsInteractable(), Is.True);
                Assert.That(_controller.ResultSequenceView.VisibleRewardRowCount, Is.Zero);
                Assert.That(_controller.ClearContinueButton.GetComponentInChildren<Text>().text,
                    Is.EqualTo(id == "stage-24" ? "FINISH PLAYTEST" : "NEXT STAGE"));
                if (id != "stage-24")
                {
                    _controller.ClearContinueButton.onClick.Invoke();
                    Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Countdown));
                }
            }
            Assert.That(_root.Playtest.Completed, Is.True);
            Assert.That(_root.Playtest.TutorialCompleted, Is.True);
            Assert.That(_root.Graph.Progression.Campaign.StageRecords.Count, Is.Zero);
            AssertCampaignPrefsUnchanged();
            _controller.ClearContinueButton.onClick.Invoke();
            float deadline = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().path != "Assets/Scenes/Frontend.unity" &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            yield return null;
            PlaytestEntryView entry = Object.FindAnyObjectByType<PlaytestEntryView>();
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text,
                Is.EqualTo("PLAY AGAIN"));
            Assert.That(_root.Playtest.Completed, Is.True);
            AssertCampaignPrefsUnchanged();
        }

        private void AssertCampaignPrefsUnchanged()
        {
            Assert.That(_snapshot.MatchesCurrentState(), Is.True);
            for (int i = 0; i < 6; i++)
            {
                string key = "ColorGateRunner.Stage.Record." + (21 + i);
                Assert.That(PlayerPrefs.HasKey(key), Is.EqualTo(_laterExists[i]), key);
                Assert.That(PlayerPrefs.GetString(key, string.Empty), Is.EqualTo(_laterValues[i]), key);
            }
        }
    }
}
