using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public enum RunnerFormPartRole
    {
        ColorShell,
        SideRingLeft,
        SideRingRight,
        SideGlowLeft,
        SideGlowRight,
        SideFinLeft,
        SideFinRight,
        RearBumper,
        RearThrusterLeft,
        RearThrusterRight,
        RearThrusterGlowLeft,
        RearThrusterGlowRight
    }

    public sealed class RunnerFormView : MonoBehaviour
    {
        internal const float TransitionDuration = 0.18f;
        internal const int RequiredPartCount = 12;

        [SerializeField] private Transform artworkRoot;
        [SerializeField] private Transform[] animatedParts;
        [SerializeField] private RunnerFormPartRole[] partRoles;

        private Vector3[] _basePositions;
        private Quaternion[] _baseRotations;
        private Vector3[] _baseScales;
        private Vector3[] _startPositions;
        private Quaternion[] _startRotations;
        private Vector3[] _startScales;
        private Vector3[] _targetPositions;
        private Quaternion[] _targetRotations;
        private Vector3[] _targetScales;
        private float _transitionElapsed;
        private bool _hasTarget;

        internal int PartCount => animatedParts == null ? 0 : animatedParts.Length;
        internal RunnerColor TargetColor { get; private set; }
        internal bool IsTransitioning { get; private set; }
        internal float TransitionProgress => IsTransitioning
            ? Mathf.Clamp01(_transitionElapsed / TransitionDuration)
            : 1f;
        internal bool HasRequiredReferences
        {
            get
            {
                if (artworkRoot == null || animatedParts == null ||
                    partRoles == null ||
                    animatedParts.Length != RequiredPartCount ||
                    partRoles.Length != RequiredPartCount)
                {
                    return false;
                }

                bool[] rolesFound = new bool[RequiredPartCount];
                for (int index = 0; index < RequiredPartCount; index++)
                {
                    Transform part = animatedParts[index];
                    int roleIndex = (int)partRoles[index];
                    if (part == null || part == artworkRoot ||
                        roleIndex < 0 || roleIndex >= RequiredPartCount ||
                        rolesFound[roleIndex])
                    {
                        return false;
                    }

                    for (int other = 0; other < index; other++)
                    {
                        if (animatedParts[other] == part)
                        {
                            return false;
                        }
                    }

                    rolesFound[roleIndex] = true;
                }

                return true;
            }
        }

        internal void Configure(
            Transform visualRoot,
            Transform[] parts,
            RunnerFormPartRole[] roles)
        {
            artworkRoot = visualRoot;
            animatedParts = parts;
            partRoles = roles;
            ClearRuntimeCache();
        }

        internal void SnapToColor(RunnerColor color)
        {
            if (!EnsureRuntimeCache())
            {
                return;
            }

            BuildTargetPose(color);
            for (int index = 0; index < animatedParts.Length; index++)
            {
                animatedParts[index].localPosition = _targetPositions[index];
                animatedParts[index].localRotation = _targetRotations[index];
                animatedParts[index].localScale = _targetScales[index];
            }

            TargetColor = color;
            _hasTarget = true;
            _transitionElapsed = TransitionDuration;
            IsTransitioning = false;
        }

        internal void Retarget(RunnerColor color)
        {
            if (!EnsureRuntimeCache() || (_hasTarget && TargetColor == color))
            {
                return;
            }

            for (int index = 0; index < animatedParts.Length; index++)
            {
                Transform part = animatedParts[index];
                _startPositions[index] = part.localPosition;
                _startRotations[index] = part.localRotation;
                _startScales[index] = part.localScale;
            }

            BuildTargetPose(color);
            TargetColor = color;
            _hasTarget = true;
            _transitionElapsed = 0f;
            IsTransitioning = true;
        }

        internal void Tick(float deltaTime)
        {
            if (!IsTransitioning || animatedParts == null)
            {
                return;
            }

            _transitionElapsed = Mathf.Min(
                TransitionDuration,
                _transitionElapsed + Mathf.Max(0f, deltaTime));
            float progress = Mathf.Clamp01(
                _transitionElapsed / TransitionDuration);
            float eased = progress * progress * (3f - (2f * progress));
            for (int index = 0; index < animatedParts.Length; index++)
            {
                Transform part = animatedParts[index];
                part.localPosition = Vector3.LerpUnclamped(
                    _startPositions[index],
                    _targetPositions[index],
                    eased);
                part.localRotation = Quaternion.SlerpUnclamped(
                    _startRotations[index],
                    _targetRotations[index],
                    eased);
                part.localScale = Vector3.LerpUnclamped(
                    _startScales[index],
                    _targetScales[index],
                    eased);
            }

            if (progress >= 1f)
            {
                IsTransitioning = false;
            }
        }

        internal Vector3 GetPartLocalPosition(RunnerFormPartRole role)
        {
            if (animatedParts == null || partRoles == null)
            {
                return Vector3.zero;
            }

            for (int index = 0; index < animatedParts.Length; index++)
            {
                if (partRoles[index] == role && animatedParts[index] != null)
                {
                    return animatedParts[index].localPosition;
                }
            }

            return Vector3.zero;
        }

        private bool EnsureRuntimeCache()
        {
            if (!HasRequiredReferences)
            {
                return false;
            }
            if (_basePositions != null &&
                _basePositions.Length == animatedParts.Length)
            {
                return true;
            }

            int count = animatedParts.Length;
            _basePositions = new Vector3[count];
            _baseRotations = new Quaternion[count];
            _baseScales = new Vector3[count];
            _startPositions = new Vector3[count];
            _startRotations = new Quaternion[count];
            _startScales = new Vector3[count];
            _targetPositions = new Vector3[count];
            _targetRotations = new Quaternion[count];
            _targetScales = new Vector3[count];
            for (int index = 0; index < count; index++)
            {
                Transform part = animatedParts[index];
                _basePositions[index] = part.localPosition;
                _baseRotations[index] = part.localRotation;
                _baseScales[index] = part.localScale;
            }

            return true;
        }

        private void BuildTargetPose(RunnerColor color)
        {
            for (int index = 0; index < animatedParts.Length; index++)
            {
                Vector3 position = _basePositions[index];
                Quaternion rotation = _baseRotations[index];
                Vector3 scale = _baseScales[index];
                if (color == RunnerColor.Red)
                {
                    ApplyPowerPose(
                        partRoles[index],
                        ref position,
                        ref rotation,
                        ref scale);
                }
                else if (color == RunnerColor.Green)
                {
                    ApplyWingPose(
                        partRoles[index],
                        ref position,
                        ref rotation,
                        ref scale);
                }

                _targetPositions[index] = position;
                _targetRotations[index] = rotation;
                _targetScales[index] = scale;
            }
        }

        private static void ApplyPowerPose(
            RunnerFormPartRole role,
            ref Vector3 position,
            ref Quaternion rotation,
            ref Vector3 scale)
        {
            switch (role)
            {
                case RunnerFormPartRole.ColorShell:
                    scale = Vector3.Scale(scale, new Vector3(1.1f, 0.9f, 1f));
                    break;
                case RunnerFormPartRole.SideRingLeft:
                case RunnerFormPartRole.SideGlowLeft:
                    position += new Vector3(0.09f, -0.04f, -0.03f);
                    scale = Vector3.Scale(scale, Vector3.one * 0.92f);
                    break;
                case RunnerFormPartRole.SideRingRight:
                case RunnerFormPartRole.SideGlowRight:
                    position += new Vector3(-0.09f, -0.04f, -0.03f);
                    scale = Vector3.Scale(scale, Vector3.one * 0.92f);
                    break;
                case RunnerFormPartRole.SideFinLeft:
                    position += new Vector3(0.08f, -0.06f, -0.08f);
                    rotation *= Quaternion.Euler(0f, 16f, 0f);
                    scale = Vector3.Scale(scale, new Vector3(0.95f, 0.95f, 1.18f));
                    break;
                case RunnerFormPartRole.SideFinRight:
                    position += new Vector3(-0.08f, -0.06f, -0.08f);
                    rotation *= Quaternion.Euler(0f, -16f, 0f);
                    scale = Vector3.Scale(scale, new Vector3(0.95f, 0.95f, 1.18f));
                    break;
                case RunnerFormPartRole.RearBumper:
                    scale = Vector3.Scale(scale, new Vector3(1.12f, 1.05f, 1.08f));
                    break;
                case RunnerFormPartRole.RearThrusterLeft:
                case RunnerFormPartRole.RearThrusterGlowLeft:
                    position += new Vector3(-0.08f, 0f, -0.08f);
                    scale = Vector3.Scale(scale, new Vector3(1.1f, 1.06f, 1.12f));
                    break;
                case RunnerFormPartRole.RearThrusterRight:
                case RunnerFormPartRole.RearThrusterGlowRight:
                    position += new Vector3(0.08f, 0f, -0.08f);
                    scale = Vector3.Scale(scale, new Vector3(1.1f, 1.06f, 1.12f));
                    break;
            }
        }

        private static void ApplyWingPose(
            RunnerFormPartRole role,
            ref Vector3 position,
            ref Quaternion rotation,
            ref Vector3 scale)
        {
            switch (role)
            {
                case RunnerFormPartRole.ColorShell:
                    scale = Vector3.Scale(scale, new Vector3(0.9f, 1.12f, 0.94f));
                    break;
                case RunnerFormPartRole.SideRingLeft:
                case RunnerFormPartRole.SideGlowLeft:
                    position += new Vector3(-0.15f, 0.09f, 0f);
                    scale = Vector3.Scale(scale, Vector3.one * 1.08f);
                    break;
                case RunnerFormPartRole.SideRingRight:
                case RunnerFormPartRole.SideGlowRight:
                    position += new Vector3(0.15f, 0.09f, 0f);
                    scale = Vector3.Scale(scale, Vector3.one * 1.08f);
                    break;
                case RunnerFormPartRole.SideFinLeft:
                    position += new Vector3(-0.28f, 0.14f, 0.08f);
                    rotation *= Quaternion.Euler(0f, 20f, -8f);
                    scale = Vector3.Scale(scale, new Vector3(1.34f, 0.9f, 1.08f));
                    break;
                case RunnerFormPartRole.SideFinRight:
                    position += new Vector3(0.28f, 0.14f, 0.08f);
                    rotation *= Quaternion.Euler(0f, -20f, 8f);
                    scale = Vector3.Scale(scale, new Vector3(1.34f, 0.9f, 1.08f));
                    break;
                case RunnerFormPartRole.RearBumper:
                    position += new Vector3(0f, 0.06f, 0.05f);
                    scale = Vector3.Scale(scale, new Vector3(0.92f, 0.92f, 0.94f));
                    break;
                case RunnerFormPartRole.RearThrusterLeft:
                case RunnerFormPartRole.RearThrusterGlowLeft:
                    position += new Vector3(0.06f, 0.11f, 0.07f);
                    scale = Vector3.Scale(scale, new Vector3(0.9f, 0.92f, 0.9f));
                    break;
                case RunnerFormPartRole.RearThrusterRight:
                case RunnerFormPartRole.RearThrusterGlowRight:
                    position += new Vector3(-0.06f, 0.11f, 0.07f);
                    scale = Vector3.Scale(scale, new Vector3(0.9f, 0.92f, 0.9f));
                    break;
            }
        }

        private void ClearRuntimeCache()
        {
            _basePositions = null;
            _baseRotations = null;
            _baseScales = null;
            _startPositions = null;
            _startRotations = null;
            _startScales = null;
            _targetPositions = null;
            _targetRotations = null;
            _targetScales = null;
            _transitionElapsed = 0f;
            _hasTarget = false;
            IsTransitioning = false;
        }
    }
}
