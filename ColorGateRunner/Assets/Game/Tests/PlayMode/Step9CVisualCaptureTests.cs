using System;
using System.Collections;
using System.IO;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class Step9CVisualCaptureTests
    {
        private StageSceneController _controller;
        private CaptureProgressStore _store;
        private string _outputPath;

        [UnityTest]
        public IEnumerator CaptureStep9CReferenceScreenshots()
        {
            if (Environment.GetEnvironmentVariable(
                "COLOR_GATE_CAPTURE_VISUALS") != "1")
            {
                yield break;
            }

            _outputPath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "Artifacts",
                "VisualValidation",
                "Step9C"));
            Directory.CreateDirectory(_outputPath);
            Screen.SetResolution(1080, 1920, false);

            yield return LoadCleanScene();
            yield return Capture("01-Lobby.png");

            _controller.PlayFromLobby();
            yield return Capture("02-PreRun.png");

            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            yield return Capture("03-Gameplay-TwoColor.png");

            yield return LoadCleanScene();
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(5);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            yield return Capture("04-Gameplay-ThreeColor.png");

            yield return LoadCleanScene();
            SelectItemEnabledStage();
            _controller.ToggleShieldSelection();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            yield return Capture("05-Shield-Active.png");

            yield return LoadCleanScene();
            StartWithBooster();
            yield return Capture("06-Booster-Active.png");
            _controller.Session.Advance(
                0.1f,
                _controller.Session.Stage.BoosterDistance * 0.85f);
            _controller.Tick(0f);
            yield return Capture("07-Booster-FinalWarning.png");

            yield return LoadCleanScene();
            _store.HighestUnlocked = 2;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(2);
            StartWithoutItems();
            ClearCurrentStage();
            _controller.Tick(_controller.ClearPanelDelaySeconds + 0.1f);
            _controller.Tick(0.62f);
            yield return Capture("08-Clear-Celebration-Impact.png");
            _controller.Tick(0.46f);
            yield return Capture("08B-Clear-Celebration-Fireworks.png");
            _controller.Tick(0.38f);
            yield return Capture("08C-Clear-Celebration-Finale.png");
            _controller.HandleGameplayTap();
            _controller.Tick(
                StageResultSequenceView.CelebrationExitDuration + 0.01f);
            _controller.Tick(2f);
            yield return Capture("09-Clear-Rewards.png");

            yield return LoadCleanScene();
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(heartCount: 4),
                new RewardedAdTestService());
            StartWithoutItems();
            FailCurrentGate();
            _controller.Tick(_controller.FailurePanelDelaySeconds + 0.1f);
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
            AssertSkinnedButtonVisible(_controller.RetryButton);
            yield return Capture("10-Failed-Continue-Offer.png");

            _controller.RequestFailureExit();
            yield return Capture("11-Failed-Exit-Confirmation.png");
            _controller.ConfirmFailureExit();
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
            AssertSkinnedButtonVisible(_controller.RetryButton);
            yield return Capture("12-Failed-Final-Choice.png");

            yield return LoadCleanScene();
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(heartCount: 4),
                new RewardedAdTestService());
            StartWithoutItems();
            FailCurrentGate();
            _controller.Tick(_controller.FailurePanelDelaySeconds + 0.1f);
            _controller.RequestCoinContinue();
            yield return Capture("13-Continue-Countdown.png");
        }

        [UnityTest]
        public IEnumerator CaptureStep10ReferenceScreenshots()
        {
            if (Environment.GetEnvironmentVariable(
                "COLOR_GATE_CAPTURE_STEP10") != "1")
            {
                yield break;
            }

            _outputPath = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "Artifacts",
                "VisualValidation",
                "Step10"));
            Directory.CreateDirectory(_outputPath);
            Screen.SetResolution(1080, 1920, false);

            yield return LoadCleanScene();
            yield return Capture("01-Clean-Lobby.png");

            StartWithoutItems();
            ConfigureStackPreview(2);
            yield return Capture("02-Color-Stack-2.png");
            ConfigureStackPreview(3);
            yield return Capture("03-Color-Stack-3.png");
            ConfigureStackPreview(4);
            yield return Capture("04-Color-Stack-4.png");
            ConfigureStackPreview(5);
            yield return Capture("05-Color-Stack-5.png");
            ConfigureStackPreview(6);
            yield return Capture("06-Color-Stack-6.png");

            yield return LoadCleanScene();
            StartWithBooster();
            yield return Capture("07-Booster-Chase-Camera.png");

            yield return LoadCleanScene();
            _controller.RequestProgressReset();
            yield return Capture("08-Reset-Confirmation.png");

            yield return LoadCleanScene();
            StartWithoutItems();
            ExperimentGatePlan camouflage = FindExperimentPlan(
                4,
                MechanicExperimentType.Camouflage,
                true);
            StageGateView camouflageGate = _controller.GetGate(0);
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            camouflageGate.ActivateExperiment(
                camouflage,
                _controller.GetPresentationMaterial(camouflage.Color),
                neutral,
                35f,
                camouflage.GateIndex - 2,
                10f,
                CamouflageSettings.CreateDefault(),
                HiddenSettings.Disabled(),
                FlickerSettings.Disabled(),
                0f);
            yield return Capture("09-Camouflage-Hidden.png");
            camouflageGate.UpdateExperimentVisibility(
                camouflage.GateIndex - 1,
                CamouflageSettings.CreateDefault().RevealLeadTimeSeconds,
                neutral,
                CamouflageSettings.CreateDefault(),
                HiddenSettings.Disabled(),
                FlickerSettings.Disabled(),
                0f,
                CamouflageSettings.CreateDefault().RevealTransitionSeconds);
            yield return Capture("10-Camouflage-Reveal.png");

            ConfigureFogPreview(false);
            yield return Capture("11-Fog-Active.png");
            ConfigureFogPreview(true);
            yield return Capture("12-Fog-Transition.png");

            Material normal =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            Material ice =
                _controller.GetPresentationMaterial(RunnerColor.Cyan);
            yield return Capture("13-Ice-Entry.png");
            _controller.TrackPool.SetSurfaceMaterial(ice);
            yield return Capture("14-Ice-Active.png");
            _controller.TrackPool.SetSurfaceMaterial(normal);
            yield return Capture("15-Ice-Exit.png");

            yield return LoadCleanScene();
            _controller.ShowStageSelect();
            _controller.StageSelectPanel.SetActive(false);
            yield return Capture("16-Experiment-Launcher.png");
        }

        private IEnumerator LoadCleanScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            _controller = UnityEngine.Object.FindAnyObjectByType<
                StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _store = new CaptureProgressStore();
            _controller.SetProgressStoreForTests(_store);
            _controller.SetStartItemInventoryForTests(
                new StartItemInventoryTestGateway());
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                new RewardedAdTestService());
            Canvas.ForceUpdateCanvases();
            yield return null;
        }

        private IEnumerator Capture(string fileName)
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            Camera camera = _controller.GameplayCamera;
            RenderMode previousMode = canvas.renderMode;
            Camera previousCanvasCamera = canvas.worldCamera;
            float previousPlaneDistance = canvas.planeDistance;
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            Transform[] uiTransforms =
                canvas.GetComponentsInChildren<Transform>(true);
            int[] previousLayers = new int[uiTransforms.Length];
            RenderTexture worldTarget = new RenderTexture(1080, 1920, 24);
            RenderTexture uiTarget = new RenderTexture(
                1080,
                1920,
                24,
                RenderTextureFormat.ARGB32);
            Texture2D worldImage = new Texture2D(
                1080,
                1920,
                TextureFormat.RGBA32,
                false);
            Texture2D uiImage = new Texture2D(
                1080,
                1920,
                TextureFormat.RGBA32,
                false);
            GameObject uiCameraObject =
                new GameObject("Step9CVisualCaptureCamera");
            Camera uiCamera = uiCameraObject.AddComponent<Camera>();
            uiCamera.enabled = false;
            uiCamera.orthographic = true;
            uiCamera.clearFlags = CameraClearFlags.SolidColor;
            uiCamera.backgroundColor = Color.clear;
            uiCamera.cullingMask = 1 << 5;
            uiCamera.depth = camera.depth + 1f;
            uiCamera.targetTexture = uiTarget;
            uiCameraObject.transform.position = new Vector3(0f, 0f, -10f);
            for (int index = 0; index < uiTransforms.Length; index++)
            {
                previousLayers[index] = uiTransforms[index].gameObject.layer;
                uiTransforms[index].gameObject.layer = 5;
            }

            camera.targetTexture = worldTarget;
            camera.Render();
            RenderTexture.active = worldTarget;
            worldImage.ReadPixels(
                new Rect(0f, 0f, 1080f, 1920f),
                0,
                0);
            worldImage.Apply();

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
            uiCamera.Render();
            RenderTexture.active = uiTarget;
            uiImage.ReadPixels(
                new Rect(0f, 0f, 1080f, 1920f),
                0,
                0);
            uiImage.Apply();
            Color32[] worldPixels = worldImage.GetPixels32();
            Color32[] uiPixels = uiImage.GetPixels32();
            for (int index = 0; index < worldPixels.Length; index++)
            {
                int alpha = uiPixels[index].a;
                int inverse = 255 - alpha;
                worldPixels[index] = new Color32(
                    (byte)((uiPixels[index].r * alpha +
                        worldPixels[index].r * inverse) / 255),
                    (byte)((uiPixels[index].g * alpha +
                        worldPixels[index].g * inverse) / 255),
                    (byte)((uiPixels[index].b * alpha +
                        worldPixels[index].b * inverse) / 255),
                    255);
            }
            worldImage.SetPixels32(worldPixels);
            worldImage.Apply();
            File.WriteAllBytes(
                Path.Combine(_outputPath, fileName),
                worldImage.EncodeToPNG());

            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            canvas.renderMode = previousMode;
            canvas.worldCamera = previousCanvasCamera;
            canvas.planeDistance = previousPlaneDistance;
            for (int index = 0; index < uiTransforms.Length; index++)
            {
                uiTransforms[index].gameObject.layer = previousLayers[index];
            }
            UnityEngine.Object.Destroy(uiCameraObject);
            UnityEngine.Object.Destroy(worldImage);
            UnityEngine.Object.Destroy(uiImage);
            UnityEngine.Object.Destroy(worldTarget);
            UnityEngine.Object.Destroy(uiTarget);
            yield return null;
        }

        private static void AssertSkinnedButtonVisible(Button button)
        {
            Assert.That(button, Is.Not.Null);
            Assert.That(button.gameObject.activeInHierarchy, Is.True);

            Image background = button.GetComponent<Image>();
            Assert.That(background, Is.Not.Null);
            Assert.That(background.enabled, Is.True);
            Assert.That(background.sprite, Is.Not.Null);
            Assert.That(background.color.a, Is.GreaterThan(0.99f));
            Assert.That(background.canvasRenderer.GetAlpha(),
                Is.GreaterThan(0.99f));
            Assert.That(background.canvasRenderer.cull, Is.False);
            AssertCanvasRendererIsVisible(background.canvasRenderer);

            Image icon = button.transform.Find("ThemeIcon")
                ?.GetComponent<Image>();
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.enabled, Is.True);
            Assert.That(icon.sprite, Is.Not.Null);
            Assert.That(icon.color.a, Is.GreaterThan(0.99f));
            Assert.That(icon.canvasRenderer.GetAlpha(),
                Is.GreaterThan(0.99f));
            Assert.That(icon.canvasRenderer.cull, Is.False);
            AssertCanvasRendererIsVisible(icon.canvasRenderer);
        }

        private static void AssertCanvasRendererIsVisible(
            CanvasRenderer renderer)
        {
            Color color = renderer.GetColor();
            Assert.That(color.r, Is.GreaterThan(0.1f));
            Assert.That(color.g, Is.GreaterThan(0.1f));
            Assert.That(color.b, Is.GreaterThan(0.1f));
        }

        private void ConfigureStackPreview(int count)
        {
            _controller.enabled = false;
            for (int index = 0; index < 6; index++)
            {
                GameObject tile = _controller.GetColorTile(index);
                tile.SetActive(index < count);
                if (index >= count)
                {
                    continue;
                }
                RunnerColor color = (RunnerColor)index;
                _controller.GetColorTileImage(index).color =
                    _controller.GetPresentationMaterial(color).color;
                _controller.GetColorTileEmblem(index).sprite =
                    _controller.GetColorEmblemSprite(color);
                ((RectTransform)tile.transform).anchoredPosition =
                    new Vector2(0f, -34f - index * 42f);
                tile.transform.localScale = Vector3.one *
                    (index == 0 ? 1f : index == 1 ? 0.82f : 0.62f);
                _controller.GetNextColorMarker(index)
                    .SetActive(index == 1);
            }
        }

        private void ConfigureFogPreview(bool transition)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Fog);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            for (int index = 0; index <= definition.FogStartGate + 3; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (index < definition.FogStartGate)
                {
                    continue;
                }
                int offset = index - definition.FogStartGate;
                StageGateView gate = _controller.GetGate(offset);
                gate.ActivateExperiment(
                    plan,
                    _controller.GetPresentationMaterial(plan.Color),
                    neutral,
                    30f + offset * 15f,
                    definition.FogStartGate + (transition ? 1 : 0),
                    float.PositiveInfinity,
                    definition.Camouflage,
                    definition.Hidden,
                    definition.Flicker,
                    0f);
            }
        }

        private static ExperimentGatePlan FindExperimentPlan(
            int colorCount,
            MechanicExperimentType mechanic,
            bool requireCamouflage)
        {
            ExperimentDefinition definition =
                ExperimentCatalog.Get(colorCount, mechanic);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (!requireCamouflage || plan.IsCamouflage)
                {
                    return plan;
                }
            }
            Assert.Fail("Required experiment plan was not generated.");
            return default;
        }

        private void StartWithoutItems()
        {
            _controller.PlayFromLobby();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
        }

        private void StartWithBooster()
        {
            SelectItemEnabledStage();
            _controller.ToggleBoosterSelection();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
        }

        private void SelectItemEnabledStage()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(8);
        }

        private void FailCurrentGate()
        {
            StageGateView gate = FindGate(_controller.Session.GatesPassed);
            if (_controller.Session.CurrentColor == gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        private void ClearCurrentStage()
        {
            while (_controller.Session.RemainingGates > 0)
            {
                StageGateView gate = FindGate(
                    _controller.Session.GatesPassed);
                Match(gate.AssignedColor);
                Assert.That(gate.TryResolveCrossing(), Is.True);
            }

            float remaining = _controller.GoalPathDistance -
                _controller.CampaignDistance + 1f;
            _controller.TickMovement(
                remaining / _controller.Session.CurrentSpeed);
            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        private void Match(RunnerColor color)
        {
            for (int index = 0;
                index < 3 && _controller.Session.CurrentColor != color;
                index++)
            {
                _controller.HandleGameplayTap();
            }
        }

        private StageGateView FindGate(int planIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    !gate.HasResolved &&
                    gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            Assert.Fail($"No active gate for plan {planIndex}.");
            return null;
        }

        private sealed class CaptureProgressStore :
            IStageProgressStore,
            IAtomicStageProgressStore
        {
            private readonly StageRecord[] _records =
                new StageRecord[StageCatalog.Count];

            internal int HighestUnlocked { get; set; } = 1;

            public int LoadHighestUnlocked() => HighestUnlocked;
            public StageRecord LoadRecord(int stageNumber) =>
                _records[stageNumber - 1];
            public void SaveHighestUnlocked(int stageNumber) =>
                HighestUnlocked = stageNumber;
            public void SaveRecord(int stageNumber, StageRecord record) =>
                _records[stageNumber - 1] = record;

            public ColorGateRunner.Product.ProductMutationResult SaveClearResult(
                int stageNumber,
                StageRecord record,
                int highestUnlocked,
                string heartRefundToken)
            {
                _records[stageNumber - 1] = record;
                HighestUnlocked = highestUnlocked;
                return ColorGateRunner.Product.ProductMutationResult.Success(true);
            }

            public void ClearGameplayProgress()
            {
                HighestUnlocked = 1;
                Array.Clear(_records, 0, _records.Length);
            }
        }
    }
}
