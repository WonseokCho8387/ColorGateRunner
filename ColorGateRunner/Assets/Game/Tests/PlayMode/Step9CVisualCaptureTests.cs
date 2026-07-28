using System;
using System.Collections;
using System.IO;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
            _controller.PlayFromLobby();
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
            StartWithoutItems();
            ClearCurrentStage();
            _controller.Tick(_controller.ClearPanelDelaySeconds + 0.1f);
            yield return Capture("08-Clear-Result.png");

            yield return LoadCleanScene();
            StartWithoutItems();
            FailCurrentGate();
            _controller.Tick(_controller.FailurePanelDelaySeconds + 0.1f);
            yield return Capture("09-Failed-Result.png");

            _controller.ContinueAfterFailure();
            yield return Capture("10-Continue-Countdown.png");
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
                0,
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

        private void StartWithoutItems()
        {
            _controller.PlayFromLobby();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
        }

        private void StartWithBooster()
        {
            _controller.PlayFromLobby();
            _controller.ToggleBoosterSelection();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
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

            Vector3 position = _controller.PlayerTransform.position;
            position.z = _controller.Goal.transform.position.z - 1f;
            _controller.PlayerTransform.position = position;
            _controller.TickMovement(2f);
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

        private sealed class CaptureProgressStore : IStageProgressStore
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
        }
    }
}
