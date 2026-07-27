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

        [UnitySetUp]
        public IEnumerator LoadGrayboxScene()
        {
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            yield return null;

            _controller = Object.FindAnyObjectByType<GameSceneController>();
            Assert.That(_controller, Is.Not.Null, "SampleScene must contain the graybox controller.");
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
        public void MismatchingTrigger_EndsRun()
        {
            GateView gate = StartAndMismatchFirstGate();

            gate.TryResolveCrossing();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Dead));
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

            Assert.That(_controller.GameOverPanel.activeSelf, Is.True);
        }

        [Test]
        public void Restart_ReturnsStateToReady()
        {
            KillSession();

            _controller.RestartButton.onClick.Invoke();

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Ready));
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

            Assert.That(_controller.Session.CurrentState, Is.EqualTo(RunState.Ready));
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

        private static int CountGateViews()
        {
            return Object.FindObjectsByType<GateView>(
                FindObjectsInactive.Include).Length;
        }

        private static void AssertInsideViewport(Camera camera, Vector3 worldPosition)
        {
            Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
            Assert.That(viewport.z, Is.GreaterThan(0f));
            Assert.That(viewport.x, Is.InRange(0f, 1f));
            Assert.That(viewport.y, Is.InRange(0f, 1f));
        }
    }
}
