using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class GrayboxScenePlayModeTests
    {
        private const string SceneName = "SampleScene";
        private const string GeneratedRootName = "ColorGateRunner_Graybox";
        private const int ExpectedGatePoolSize = 5;

        private GameSceneController _controller;
        private InMemoryBestScoreStore _bestScoreStore;
        private InMemoryScoreHistoryStore _historyStore;

        [UnitySetUp]
        public IEnumerator LoadGrayboxScene()
        {
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            yield return null;

            _controller = Object.FindAnyObjectByType<GameSceneController>();
            Assert.That(_controller, Is.Not.Null, "SampleScene must contain the graybox controller.");
            _bestScoreStore = new InMemoryBestScoreStore();
            _controller.SetBestScoreStoreForTests(_bestScoreStore);
            _historyStore = new InMemoryScoreHistoryStore();
            _controller.SetScoreHistoryStoreForTests(_historyStore);
        }

        [Test]
        public void Ready_ShowsDimmedStartOverlay()
        {
            Image dimmer = _controller.ReadyOverlay.GetComponent<Image>();

            Assert.That(_controller.ReadyOverlay.activeSelf, Is.True);
            Assert.That(dimmer, Is.Not.Null);
            Assert.That(dimmer.color.a, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(_controller.RestartButton.gameObject.activeInHierarchy, Is.False);
        }

        [Test]
        public void Ready_ShowsTitleAndStartInstruction()
        {
            Assert.That(_controller.ReadyTitleText.text, Is.EqualTo("COLOR GATE"));
            Assert.That(_controller.ReadyInstructionText.text, Does.Contain("MATCH"));
            Assert.That(_controller.ReadyInstructionText.text, Does.Contain("COLOR"));
            Assert.That(_controller.ReadyTapText.text, Is.EqualTo("TAP TO START"));
        }

        [Test]
        public void GameplayTap_HidesReadyOverlay()
        {
            SendGameplayTap();

            Assert.That(_controller.ReadyOverlay.activeSelf, Is.False);
        }

        [Test]
        public void ScoreHud_HasLabelAndFramedPanel()
        {
            Image panelImage = _controller.ScorePanel.GetComponent<Image>();
            Outline outline = _controller.ScorePanel.GetComponent<Outline>();

            Assert.That(_controller.ScorePanel.activeInHierarchy, Is.True);
            Assert.That(panelImage, Is.Not.Null);
            Assert.That(panelImage.color.a, Is.GreaterThan(0.5f));
            Assert.That(outline, Is.Not.Null);
            Assert.That(_controller.ScoreLabel.text, Is.EqualTo("SCORE"));
            Assert.That(_controller.ScoreText.fontSize, Is.GreaterThan(_controller.ScoreLabel.fontSize));
        }

        [Test]
        public void Scene_StartsInReady()
        {
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Ready));
        }

        [Test]
        public void Scene_StartsWithRedPlayer()
        {
            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(RunnerColor.Red));
            Assert.That(
                _controller.PlayerRenderer.sharedMaterial.name,
                Does.StartWith("Red"));
        }

        [Test]
        public void FirstGameplayTap_StartsRun()
        {
            SendGameplayTap();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void FirstGameplayTap_DoesNotTogglePlayerColor()
        {
            SendGameplayTap();

            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void PlayingTap_TogglesPlayerColor()
        {
            SendGameplayTap();
            SendGameplayTap();

            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(RunnerColor.Blue));
        }

        [Test]
        public void MatchingTrigger_DoesNotEndRun()
        {
            GateView gate = StartAndMatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void MatchingTrigger_IncrementsHudScore()
        {
            GateView gate = StartAndMatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.Session.CurrentScore, Is.EqualTo(1));
            Assert.That(_controller.ScoreText.text, Is.EqualTo("1"));
        }

        [Test]
        public void CorrectGate_TriggersFeedback()
        {
            Vector3 initialScale = _controller.PlayerTransform.localScale;
            GateView gate = StartAndMatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.IsSuccessFeedbackActive, Is.True);
            Assert.That(_controller.PlayerTransform.localScale.x, Is.GreaterThan(initialScale.x));
            Assert.That(_controller.SuccessParticles.particleCount, Is.GreaterThan(0));
        }

        [Test]
        public void CorrectGate_PulsesScore()
        {
            GateView gate = StartAndMatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.IsScorePulsing, Is.True);
            Assert.That(_controller.ScorePulseTarget.localScale.x, Is.GreaterThan(1f));
        }

        [Test]
        public void MismatchingTrigger_EndsRun()
        {
            GateView gate = StartAndMismatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [Test]
        public void IncorrectGate_TriggersFailureFeedback()
        {
            GateView gate = StartAndMismatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.IsFailureFeedbackActive, Is.True);
            Assert.That(gate.IsShowingFailure, Is.True);
            Assert.That(
                _controller.PlayerRenderer.sharedMaterial.name,
                Does.StartWith("Failure"));
            Assert.That(_controller.GameOverPanel.activeSelf, Is.False);
        }

        [Test]
        public void CameraShake_RestoresOriginalPosition()
        {
            GateView gate = StartAndMismatchFirstGate();
            Vector3 originalPosition = _controller.GameplayCamera.transform.position;
            Quaternion originalRotation = _controller.GameplayCamera.transform.rotation;

            gate.TryResolveCrossing();

            _controller.Tick(0.05f);
            Assert.That(_controller.GameplayCamera.transform.position, Is.Not.EqualTo(originalPosition));
            _controller.Tick(0.25f);

            Assert.That(_controller.GameplayCamera.transform.position, Is.EqualTo(originalPosition));
            Assert.That(_controller.GameplayCamera.transform.rotation, Is.EqualTo(originalRotation));
        }

        [Test]
        public void Death_StopsMovementImmediately()
        {
            GateView gate = StartAndMismatchFirstGate();
            Vector3 beforeDeath = _controller.PlayerTransform.position;

            gate.TryResolveCrossing();
            _controller.TickMovement(1f);

            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(beforeDeath));
        }

        [Test]
        public void Death_ShowsGameOverPanel()
        {
            GateView gate = StartAndMismatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.GameOverPanel.activeSelf, Is.False);
            _controller.Tick(0.88f);
            Assert.That(_controller.GameOverPanel.activeSelf, Is.False);
            _controller.Tick(0.03f);
            Assert.That(_controller.GameOverPanel.activeSelf, Is.True);
        }

        [Test]
        public void GameOver_ShowsCurrentScore()
        {
            GateView gate = StartAndMatchFirstGate();
            gate.TryResolveCrossing();
            KillSessionFromPlaying();
            _controller.Tick(0.9f);

            Assert.That(_controller.GameOverScoreText.text, Is.EqualTo("CURRENT  1"));
        }

        [Test]
        public void GameOver_ShowsBestScore()
        {
            GateView gate = StartAndMatchFirstGate();
            gate.TryResolveCrossing();
            KillSessionFromPlaying();
            _controller.Tick(0.9f);

            Assert.That(_controller.BestScoreText.text, Does.Contain("BEST  1"));
        }

        [Test]
        public void HigherScore_UpdatesBestScore()
        {
            SendGameplayTap();
            ResolveGateAsMatch(_controller.GetGate(0));
            ResolveGateAsMatch(_controller.GetGate(1));
            KillSessionFromPlaying();

            Assert.That(_controller.BestScore, Is.EqualTo(2));
            Assert.That(_bestScoreStore.StoredScore, Is.EqualTo(2));
        }

        [Test]
        public void LowerScore_DoesNotReplaceBestScore()
        {
            _bestScoreStore = new InMemoryBestScoreStore(5);
            _controller.SetBestScoreStoreForTests(_bestScoreStore);
            GateView gate = StartAndMatchFirstGate();
            gate.TryResolveCrossing();
            KillSessionFromPlaying();

            Assert.That(_controller.BestScore, Is.EqualTo(5));
            Assert.That(_bestScoreStore.StoredScore, Is.EqualTo(5));
            Assert.That(_controller.BestScoreText.text, Does.Contain("BEST  5"));
        }

        [Test]
        public void Retry_EntersCountdown()
        {
            KillSession();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
            Assert.That(_controller.CountdownPanel.activeSelf, Is.True);
        }

        [Test]
        public void Restart_RestoresPlayerPosition()
        {
            Vector3 initialPosition = _controller.PlayerTransform.position;
            SendGameplayTap();
            _controller.TickMovement(1f);
            KillSessionFromPlaying();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(initialPosition));
        }

        [Test]
        public void Restart_RestoresRedColor()
        {
            SendGameplayTap();
            SendGameplayTap();
            KillSessionFromPlaying();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(RunnerColor.Red));
            Assert.That(
                _controller.PlayerRenderer.sharedMaterial.name,
                Does.StartWith("Red"));
        }

        [Test]
        public void Restart_ResetsHudScore()
        {
            GateView gate = StartAndMatchFirstGate();
            gate.TryResolveCrossing();
            KillSessionFromPlaying();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.Session.CurrentScore, Is.Zero);
            Assert.That(_controller.ScoreText.text, Is.EqualTo("0"));
        }

        [Test]
        public void Restart_ReplaysGateSequence()
        {
            RunnerColor[] initialColors = CaptureGateColors();
            SendGameplayTap();
            ResolveGateAsMatch(_controller.GetGate(0));
            KillSessionFromPlaying();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(CaptureGateColors(), Is.EqualTo(initialColors));
        }

        [Test]
        public void Retry_ClearsFailureFeedback()
        {
            KillSession();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.IsFailureFeedbackActive, Is.False);
            Assert.That(_controller.PlayerTransform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(
                _controller.PlayerRenderer.sharedMaterial.name,
                Does.StartWith("Red"));
            Assert.That(_controller.SuccessParticles.particleCount, Is.Zero);
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                Assert.That(_controller.GetGate(index).IsShowingFailure, Is.False);
            }
        }

        [Test]
        public void Retry_DoesNotAlsoStartGameplay()
        {
            KillSession();
            _controller.Tick(0.9f);
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };

            ExecuteEvents.Execute(
                _controller.RestartButton.gameObject,
                pointer,
                ExecuteEvents.pointerClickHandler);

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
            Assert.That(_controller.ReadyOverlay.activeSelf, Is.False);
        }

        [Test]
        public void GateSpacing_RespectsReactionTimeMinimum()
        {
            float previousZ = _controller.PlayerTransform.position.z;
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                float gateZ = _controller.GetGate(index).transform.position.z;
                float spacing = gateZ - previousZ;
                Assert.That(
                    spacing,
                    Is.GreaterThanOrEqualTo(
                        GameRules.CalculateMinimumSafeSpacing(
                            _controller.Session.CurrentSpeed)));
                previousZ = gateZ;
            }
        }

        [Test]
        public void Restart_ReplaysIdenticalGateLayout()
        {
            RunnerColor[] initialColors = CaptureGateColors();
            float[] initialPositions = CaptureGatePositions();
            KillSession();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(CaptureGateColors(), Is.EqualTo(initialColors));
            Assert.That(CaptureGatePositions(), Is.EqualTo(initialPositions));
        }

        [Test]
        public void Gate_ResolvesOnlyOncePerActivation()
        {
            GateView gate = StartAndMismatchFirstGate();

            bool firstResolution = gate.TryResolveCrossing();
            bool secondResolution = gate.TryResolveCrossing();

            Assert.That(firstResolution, Is.True);
            Assert.That(secondResolution, Is.False);
            Assert.That(gate.HasResolved, Is.True);
        }

        [Test]
        public void GatePool_ObjectCountDoesNotGrow()
        {
            int initialCount = CountGateViews();
            SendGameplayTap();

            for (int iteration = 0; iteration < 20; iteration++)
            {
                ResolveGateAsMatch(_controller.GetGate(iteration % ExpectedGatePoolSize));
            }

            Assert.That(CountGateViews(), Is.EqualTo(initialCount));
            Assert.That(initialCount, Is.EqualTo(ExpectedGatePoolSize));
        }

        [Test]
        public void CenterTap_IsAccepted()
        {
            RectTransform tapRect =
                _controller.TapSurface.GetComponent<RectTransform>();
            Assert.That(tapRect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(tapRect.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(tapRect.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(tapRect.offsetMax, Is.EqualTo(Vector2.zero));

            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };
            _controller.TapSurface.OnPointerClick(pointer);

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void RestartClick_DoesNotAlsoTriggerGameplayTap()
        {
            KillSession();
            _controller.Tick(0.9f);
            Transform gameOverPanel = _controller.RestartButton.transform.parent;
            Assert.That(
                gameOverPanel.GetSiblingIndex(),
                Is.GreaterThan(_controller.TapSurface.transform.GetSiblingIndex()));
            Assert.That(
                _controller.RestartButton.targetGraphic.raycastTarget,
                Is.True);

            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };

            ExecuteEvents.Execute(
                _controller.RestartButton.gameObject,
                pointer,
                ExecuteEvents.pointerClickHandler);

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
        }

        [Test]
        public void RequiredSerializedReferences_AreAssigned()
        {
            Assert.That(_controller.HasRequiredReferences(), Is.True);
        }

        [Test]
        public void Scene_HasNoMissingMonoBehaviours()
        {
            int missingScriptCount = 0;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                Transform[] transforms = roots[index].GetComponentsInChildren<Transform>(true);
                for (int childIndex = 0; childIndex < transforms.Length; childIndex++)
                {
                    missingScriptCount +=
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            transforms[childIndex].gameObject);
                }
            }

            Assert.That(missingScriptCount, Is.Zero);
        }

        [Test]
        public void CameraRotation_DoesNotChangeDuringPlay()
        {
            Quaternion initialRotation = _controller.GameplayCamera.transform.rotation;
            SendGameplayTap();

            _controller.TickMovement(1f);

            Assert.That(_controller.GameplayCamera.transform.rotation, Is.EqualTo(initialRotation));
        }

        [Test]
        public void PlayerAndNextTwoGates_AreInsideCameraViewportAtPortraitAspect()
        {
            Camera camera = _controller.GameplayCamera;
            float originalAspect = camera.aspect;
            camera.aspect = 9f / 16f;

            AssertInsideViewport(camera, _controller.PlayerTransform.position);
            AssertInsideViewport(camera, _controller.GetGate(0).transform.position + Vector3.up);
            AssertInsideViewport(camera, _controller.GetGate(1).transform.position + Vector3.up);

            camera.aspect = originalAspect;
        }

        [Test]
        public void Scene_HasSingleGeneratedRoot()
        {
            int generatedRootCount = 0;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == GeneratedRootName)
                {
                    generatedRootCount++;
                }
            }

            Assert.That(generatedRootCount, Is.EqualTo(1));
        }

        [Test]
        public void Scene_UsesPortraitOrientation()
        {
            Assert.That(
                PlayerSettings.defaultInterfaceOrientation,
                Is.EqualTo(UIOrientation.Portrait));
        }

        [Test]
        public void Scene_HasNoActivePostProcessingVolume()
        {
            Component[] components = Object.FindObjectsByType<Component>(
                FindObjectsInactive.Include);

            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component != null &&
                    component.GetType().FullName == "UnityEngine.Rendering.Volume")
                {
                    Assert.That(
                        component.gameObject.activeInHierarchy && component is Behaviour behaviour &&
                        behaviour.enabled,
                        Is.False,
                        "An active post-processing Volume remains in SampleScene.");
                }
            }
        }

        [Test]
        public void Camera_PostProcessingIsDisabled()
        {
            Component[] cameraComponents =
                _controller.GameplayCamera.GetComponents<Component>();

            for (int index = 0; index < cameraComponents.Length; index++)
            {
                Component component = cameraComponents[index];
                if (component == null ||
                    component.GetType().FullName !=
                    "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData")
                {
                    continue;
                }

                var serializedComponent = new SerializedObject(component);
                SerializedProperty postProcessing =
                    serializedComponent.FindProperty("m_RenderPostProcessing");
                Assert.That(postProcessing, Is.Not.Null);
                Assert.That(postProcessing.boolValue, Is.False);
                return;
            }

            Assert.Fail("The graybox camera is missing UniversalAdditionalCameraData.");
        }

        [Test]
        public void Scene_HasFixedGatePool()
        {
            Assert.That(_controller.GatePoolSize, Is.EqualTo(ExpectedGatePoolSize));
            Assert.That(CountGateViews(), Is.EqualTo(ExpectedGatePoolSize));
        }

        [Test]
        public void Scene_HasNoDuplicateStep4PresentationObjects()
        {
            GameObject generatedRoot = GameObject.Find(GeneratedRootName);
            Assert.That(generatedRoot, Is.Not.Null);
            string[] uniqueNames =
            {
                "Canvas",
                "SafeAreaRoot",
                "ReadyOverlay",
                "ReadyTitle",
                "ReadyInstruction",
                "TapToStartVisual",
                "ScorePanel",
                "ScoreLabel",
                "ScoreValue",
                "ShieldIndicator",
                "ShieldVisual",
                "ShieldMessage",
                "SpeedStage",
                "CountdownPanel",
                "CountdownText",
                "GameOverPanel",
                "GameOverScore",
                "BestScore",
                "TopScores",
                "TopScoresPanel",
                "BestBadge",
                "NewBestText",
                "RestartButton",
                "SuccessParticles",
                "ShieldParticles",
                "SpeedLines",
                "TrackPool",
                "CurveSegmentExperiment",
                "EventSystem"
            };

            for (int index = 0; index < uniqueNames.Length; index++)
            {
                Assert.That(
                    CountNamedTransforms(generatedRoot, uniqueNames[index]),
                    Is.EqualTo(1),
                    $"{uniqueNames[index]} must exist exactly once.");
            }
        }

        [Test]
        public void SafeArea_CalculatesNormalizedAnchors()
        {
            SafeAreaLayout.CalculateAnchors(
                new Rect(0f, 100f, 1080f, 1720f),
                1080,
                1920,
                out Vector2 min,
                out Vector2 max);

            Assert.That(min, Is.EqualTo(new Vector2(0f, 100f / 1920f)));
            Assert.That(max, Is.EqualTo(new Vector2(1f, 1820f / 1920f)));
        }

        [Test]
        public void Shield_ProtectsOneMismatchThenNextMismatchFails()
        {
            SendGameplayTap();
            for (int index = 0; index < GameRules.ShieldScoreMilestone; index++)
            {
                ResolveGateAsMatch(_controller.GetGate(index));
            }
            Assert.That(_controller.Session.ShieldActive, Is.True);

            GateView firstMismatch = _controller.GetGate(3);
            MismatchCurrentColorFrom(firstMismatch.AssignedColor);
            Assert.That(firstMismatch.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.Session.CurrentState,
                Is.EqualTo(RunState.ShieldRecovery));
            Assert.That(_controller.Session.ShieldActive, Is.False);
            Assert.That(_controller.Session.CurrentScore, Is.EqualTo(3));

            GateView secondMismatch = _controller.GetGate(4);
            MismatchCurrentColorFrom(secondMismatch.AssignedColor);
            secondMismatch.TryResolveCrossing();
            Assert.That(
                _controller.Session.CurrentState,
                Is.EqualTo(RunState.ShieldRecovery));

            _controller.Tick(0.13f);
            _controller.Tick(GameRules.ShieldRecoveryDuration);
            MismatchCurrentColorFrom(secondMismatch.AssignedColor);
            secondMismatch.TryResolveCrossing();
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [Test]
        public void ScoreHistory_SortsTruncatesAndAllowsDuplicates()
        {
            int[] scores = System.Array.Empty<int>();
            foreach (int score in new[] { 4, 9, 2, 9, 7, 1 })
            {
                scores = ScoreHistory.Insert(scores, score);
            }

            Assert.That(scores, Is.EqualTo(new[] { 9, 9, 7, 4, 2 }));
        }

        [Test]
        public void ScoreHistory_CorruptedDataFallsBackToEmpty()
        {
            Assert.That(ScoreHistory.Parse("12,broken,7"), Is.Empty);
            Assert.That(ScoreHistory.Parse(string.Empty), Is.Empty);
        }

        [Test]
        public void ScoreHistory_RejectsZeroAndLowSixthScore()
        {
            int[] full = { 10, 9, 8, 7, 6 };

            Assert.That(ScoreHistory.Insert(full, 0), Is.EqualTo(full));
            Assert.That(ScoreHistory.Insert(full, 5), Is.EqualTo(full));
        }

        [Test]
        public void GameOver_ShowsCompletedRunInTopScores()
        {
            GateView gate = StartAndMatchFirstGate();
            gate.TryResolveCrossing();
            KillSessionFromPlaying();
            _controller.Tick(0.9f);

            Assert.That(_controller.TopScoresText.text, Does.Contain("1."));
            Assert.That(_controller.TopScoresText.text, Does.Contain("1"));
            Assert.That(_controller.TopScoresText.text, Does.Contain("►"));
            Assert.That(_controller.CompletedRunRank, Is.Zero);
            Assert.That(_controller.NewBestText.text, Does.Contain("RANK 1"));
        }

        [Test]
        public void BestScore_IsConsistentWithTopScoreRankOne()
        {
            _bestScoreStore = new InMemoryBestScoreStore(2);
            _controller.SetBestScoreStoreForTests(_bestScoreStore);
            _historyStore.Save(new[] { 8, 5, 3 });
            _controller.SetScoreHistoryStoreForTests(_historyStore);

            Assert.That(_controller.BestScore, Is.EqualTo(8));
            Assert.That(_controller.BestScoreText.text, Does.Contain("BEST  8"));
        }

        [Test]
        public void Countdown_DoesNotAdvanceGameplayOrAcceptInput()
        {
            KillSession();
            _controller.RestartButton.onClick.Invoke();
            RunnerColor initialColor = _controller.Session.CurrentColor;

            SendGameplayTap();
            _controller.Tick(2f);

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
            Assert.That(_controller.Session.ElapsedPlayingSeconds, Is.Zero);
            Assert.That(_controller.Session.CurrentScore, Is.Zero);
            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(initialColor));
            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(new Vector3(0f, 1f, 0f)));
        }

        [Test]
        public void Countdown_CompletesOnceIntoPlaying()
        {
            KillSession();
            _controller.RestartButton.onClick.Invoke();

            _controller.Tick(_controller.CountdownDuration - 0.3f);
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
            Assert.That(_controller.CountdownText.text, Is.EqualTo("1"));
            _controller.Tick(0.1f);
            Assert.That(_controller.CountdownText.text, Is.EqualTo("GO"));
            _controller.Tick(0.3f);

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Playing));
            Assert.That(_controller.CountdownPanel.activeSelf, Is.False);
            float elapsed = _controller.Session.ElapsedPlayingSeconds;
            _controller.Tick(0f);
            Assert.That(_controller.Session.ElapsedPlayingSeconds, Is.EqualTo(elapsed));
        }

        [Test]
        public void Countdown_RepeatedRetryDoesNotOverlap()
        {
            KillSession();
            _controller.RestartButton.onClick.Invoke();
            _controller.Tick(1f);
            float remaining = _controller.CountdownRemaining;

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.CountdownRemaining, Is.EqualTo(remaining));
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Countdown));
        }

        [Test]
        public void SpeedStages_ChangeFovTrailAndSpeedLines()
        {
            SendGameplayTap();
            float startingFov = _controller.GameplayCamera.fieldOfView;
            float startingTrail = _controller.PlayerTrail.time;

            _controller.Tick(13f);
            ParticleSystem.EmissionModule emission = _controller.SpeedLines.emission;

            Assert.That(_controller.GameplayCamera.fieldOfView, Is.GreaterThan(startingFov));
            Assert.That(_controller.PlayerTrail.time, Is.GreaterThan(startingTrail));
            Assert.That(emission.rateOverTime.constant, Is.GreaterThan(0f));
            Assert.That(_controller.SpeedStageText.text, Is.EqualTo("SPEED  III"));
        }

        [Test]
        public void ShieldAcquisition_ShowsPersistentPlayerAndHudFeedback()
        {
            SendGameplayTap();
            for (int index = 0; index < GameRules.ShieldScoreMilestone; index++)
            {
                ResolveGateAsMatch(_controller.GetGate(index));
            }

            Assert.That(_controller.Session.ShieldActive, Is.True);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.True);
            Assert.That(_controller.ShieldText.text, Does.Contain("READY"));
            Assert.That(_controller.ShieldMessageText.gameObject.activeSelf, Is.True);
            Assert.That(_controller.ShieldMessageText.text, Is.EqualTo("SHIELD"));
            Assert.That(_controller.ShieldParticles.particleCount, Is.GreaterThan(0));
        }

        [Test]
        public void ShieldBreak_ShowsDistinctRecoveryFeedback()
        {
            SendGameplayTap();
            for (int index = 0; index < GameRules.ShieldScoreMilestone; index++)
            {
                ResolveGateAsMatch(_controller.GetGate(index));
            }
            GateView gate = _controller.GetGate(3);
            MismatchCurrentColorFrom(gate.AssignedColor);

            gate.TryResolveCrossing();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.ShieldRecovery));
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);
            Assert.That(_controller.ShieldText.text, Does.Contain("BROKEN"));
            Assert.That(_controller.ShieldMessageText.text, Is.EqualTo("SHIELD BREAK"));
            Assert.That(_controller.ShieldParticles.particleCount, Is.GreaterThan(0));
        }

        [Test]
        public void Failure_AnimatesProgressivelyBeforeGameOver()
        {
            GateView gate = StartAndMismatchFirstGate();
            Vector3 startPosition = _controller.PlayerTransform.position;
            Quaternion startRotation = _controller.PlayerTransform.rotation;
            gate.TryResolveCrossing();

            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(startPosition));
            Assert.That(_controller.PlayerTransform.rotation, Is.EqualTo(startRotation));
            _controller.Tick(0.45f);
            Assert.That(_controller.PlayerTransform.position.y, Is.LessThan(startPosition.y));
            Assert.That(_controller.PlayerTransform.rotation, Is.Not.EqualTo(startRotation));
            Assert.That(_controller.GameOverPanel.activeSelf, Is.False);
            _controller.Tick(0.46f);
            Assert.That(_controller.GameOverPanel.activeSelf, Is.True);
        }

        [Test]
        public void TrackPool_LongRunRecyclesWithoutGapsOrGrowth()
        {
            TrackPoolController pool = _controller.TrackPool;
            int initialCount = pool.SegmentCount;
            const float TravelDistance = 5000f;

            for (float distance = 0f; distance <= TravelDistance; distance += 10f)
            {
                pool.Tick(distance);
                AssertTrackContinuityAndCoverage(pool, distance);
            }

            Assert.That(pool.SegmentCount, Is.EqualTo(initialCount));
            Assert.That(
                Object.FindObjectsByType<TrackSegmentView>(
                    FindObjectsInactive.Include).Length,
                Is.EqualTo(initialCount + 1));
        }

        [Test]
        public void CurveExperiment_IsPresentButDisabledForLongRunSafety()
        {
            GameObject curve = GameObject.Find("CurveSegmentExperiment");
            if (curve == null)
            {
                TrackSegmentView[] all =
                    Object.FindObjectsByType<TrackSegmentView>(
                        FindObjectsInactive.Include);
                for (int index = 0; index < all.Length; index++)
                {
                    if (all[index].name == "CurveSegmentExperiment")
                    {
                        curve = all[index].gameObject;
                        break;
                    }
                }
            }

            Assert.That(curve, Is.Not.Null);
            Assert.That(curve.activeSelf, Is.False);
            Assert.That(
                curve.GetComponent<TrackSegmentView>().IsCurveExperiment,
                Is.True);
        }

        [Test]
        public void TrackMarkers_UseDistinctMaterialFromGround()
        {
            TrackSegmentView segment = _controller.TrackPool.GetSegment(0);
            Renderer ground =
                segment.transform.Find("Ground").GetComponent<Renderer>();
            Renderer marker =
                segment.transform.Find("LaneMarker_00").GetComponent<Renderer>();

            Assert.That(marker.sharedMaterial, Is.Not.SameAs(ground.sharedMaterial));
        }

        [Test]
        public void ScoreHistory_RankPolicyHandlesBestDuplicateAndRejection()
        {
            int[] existing = { 20, 15, 15, 10, 5 };

            ScoreHistoryUpdate best = ScoreHistory.InsertWithResult(existing, 25);
            ScoreHistoryUpdate duplicate = ScoreHistory.InsertWithResult(existing, 15);
            ScoreHistoryUpdate rejected = ScoreHistory.InsertWithResult(existing, 4);

            Assert.That(best.InsertedRank, Is.Zero);
            Assert.That(best.IsNewBest, Is.True);
            Assert.That(duplicate.InsertedRank, Is.EqualTo(3));
            Assert.That(duplicate.IsNewBest, Is.False);
            Assert.That(rejected.InsertedRank, Is.EqualTo(-1));
            Assert.That(rejected.Scores, Is.EqualTo(existing));
        }

        private static void AssertTrackContinuityAndCoverage(
            TrackPoolController pool,
            float playerZ)
        {
            var segments = new TrackSegmentView[pool.SegmentCount];
            for (int index = 0; index < segments.Length; index++)
            {
                segments[index] = pool.GetSegment(index);
                Assert.That(float.IsNaN(segments[index].transform.position.z), Is.False);
            }

            System.Array.Sort(
                segments,
                (left, right) =>
                    left.StartAnchorPosition.z.CompareTo(right.StartAnchorPosition.z));
            bool covered = false;
            for (int index = 0; index < segments.Length; index++)
            {
                if (index > 0)
                {
                    Assert.That(
                        segments[index].StartAnchorPosition,
                        Is.EqualTo(segments[index - 1].EndAnchorPosition));
                }

                covered |=
                    playerZ >= segments[index].StartAnchorPosition.z &&
                    playerZ <= segments[index].EndAnchorPosition.z;
            }

            Assert.That(covered, Is.True, $"No track segment covered z={playerZ}.");
        }

        private void SendGameplayTap()
        {
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };
            _controller.TapSurface.OnPointerClick(pointer);
        }

        private GateView StartAndMatchFirstGate()
        {
            SendGameplayTap();
            GateView gate = _controller.GetGate(0);
            MatchCurrentColorTo(gate.AssignedColor);
            return gate;
        }

        private GateView StartAndMismatchFirstGate()
        {
            SendGameplayTap();
            GateView gate = _controller.GetGate(0);
            MismatchCurrentColorFrom(gate.AssignedColor);
            return gate;
        }

        private void ResolveGateAsMatch(GateView gate)
        {
            MatchCurrentColorTo(gate.AssignedColor);
            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        private void MatchCurrentColorTo(RunnerColor gateColor)
        {
            if (_controller.Session.CurrentColor != gateColor)
            {
                SendGameplayTap();
            }
        }

        private void MismatchCurrentColorFrom(RunnerColor gateColor)
        {
            if (_controller.Session.CurrentColor == gateColor)
            {
                SendGameplayTap();
            }
        }

        private void KillSession()
        {
            GateView gate = StartAndMismatchFirstGate();
            gate.TryResolveCrossing();
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        private void KillSessionFromPlaying()
        {
            GateView gate = _controller.GetGate(1);
            MismatchCurrentColorFrom(gate.AssignedColor);
            gate.TryResolveCrossing();
            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        private RunnerColor[] CaptureGateColors()
        {
            var colors = new RunnerColor[_controller.GatePoolSize];
            for (int index = 0; index < colors.Length; index++)
            {
                colors[index] = _controller.GetGate(index).AssignedColor;
            }

            return colors;
        }

        private float[] CaptureGatePositions()
        {
            var positions = new float[_controller.GatePoolSize];
            for (int index = 0; index < positions.Length; index++)
            {
                positions[index] = _controller.GetGate(index).transform.position.z;
            }

            return positions;
        }

        private static int CountGateViews()
        {
            return Object.FindObjectsByType<GateView>(
                FindObjectsInactive.Include).Length;
        }

        private static int CountNamedTransforms(GameObject root, string objectName)
        {
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == objectName)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertInsideViewport(Camera camera, Vector3 worldPosition)
        {
            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            Assert.That(viewport.z, Is.GreaterThan(0f));
            Assert.That(viewport.x, Is.InRange(0f, 1f));
            Assert.That(viewport.y, Is.InRange(0f, 1f));
        }

        private sealed class InMemoryBestScoreStore : IBestScoreStore
        {
            internal InMemoryBestScoreStore(int initialScore = 0)
            {
                StoredScore = initialScore;
            }

            internal int StoredScore { get; private set; }

            public int Load()
            {
                return StoredScore;
            }

            public void Save(int score)
            {
                StoredScore = score;
            }
        }

        private sealed class InMemoryScoreHistoryStore : IScoreHistoryStore
        {
            private int[] _scores = System.Array.Empty<int>();

            public int[] Load()
            {
                return _scores;
            }

            public void Save(int[] scores)
            {
                _scores = scores;
            }
        }
    }
}
