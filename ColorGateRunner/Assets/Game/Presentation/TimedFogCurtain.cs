using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal sealed class TimedFogCurtainState
    {
        internal const float FullOpacitySeconds = 1.5f;
        internal const float FadeSeconds = 0.5f;
        internal const float TotalSeconds =
            FullOpacitySeconds + FadeSeconds;

        private float _elapsedSeconds;

        internal bool HasTriggered { get; private set; }
        internal bool IsActive => HasTriggered && Alpha > 0f;
        internal float ElapsedSeconds => _elapsedSeconds;
        internal float Alpha
        {
            get
            {
                if (!HasTriggered || _elapsedSeconds >= TotalSeconds)
                {
                    return 0f;
                }
                if (_elapsedSeconds <= FullOpacitySeconds)
                {
                    return 1f;
                }
                return 1f -
                    ((_elapsedSeconds - FullOpacitySeconds) / FadeSeconds);
            }
        }

        internal bool TryTrigger()
        {
            if (HasTriggered)
            {
                return false;
            }
            HasTriggered = true;
            _elapsedSeconds = 0f;
            return true;
        }

        internal void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (!HasTriggered || _elapsedSeconds >= TotalSeconds)
            {
                return;
            }
            _elapsedSeconds = Math.Min(
                TotalSeconds,
                _elapsedSeconds + deltaSeconds);
        }

        internal void Reset()
        {
            HasTriggered = false;
            _elapsedSeconds = 0f;
        }
    }

    public sealed class TimedFogCurtainView : MonoBehaviour
    {
        internal const float ForwardTravelSeconds = 1.5f;
        internal const float HeightOffset = 3.5f;

        [SerializeField] private Renderer curtainRenderer;

        private readonly TimedFogCurtainState _state =
            new TimedFogCurtainState();
        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor = Color.white;

        internal bool HasTriggered => _state.HasTriggered;
        internal bool IsVisible => gameObject.activeSelf;
        internal float Alpha => _state.Alpha;
        internal float ElapsedSeconds => _state.ElapsedSeconds;

        private void Awake()
        {
            EnsurePresentationState();
        }

        internal void Configure(Renderer renderer)
        {
            curtainRenderer = renderer;
            EnsurePresentationState();
            ResetCurtain();
        }

        internal bool HasRequiredReferences()
        {
            return curtainRenderer != null;
        }

        internal bool TryActivate(Vector3 playerPosition, float speed)
        {
            bool activated = _state.TryTrigger();
            Synchronize(playerPosition, speed);
            return activated;
        }

        internal void Tick(
            float deltaSeconds,
            Vector3 playerPosition,
            float speed)
        {
            _state.Advance(Mathf.Max(0f, deltaSeconds));
            Synchronize(playerPosition, speed);
        }

        internal void Synchronize(Vector3 playerPosition, float speed)
        {
            if (!_state.IsActive)
            {
                ApplyAlpha(0f);
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.position = new Vector3(
                playerPosition.x,
                playerPosition.y + HeightOffset,
                playerPosition.z +
                    Mathf.Max(0f, speed) * ForwardTravelSeconds);
            ApplyAlpha(_state.Alpha);
        }

        internal void ResetCurtain()
        {
            _state.Reset();
            ApplyAlpha(0f);
            gameObject.SetActive(false);
        }

        private void EnsurePresentationState()
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            if (curtainRenderer != null &&
                curtainRenderer.sharedMaterial != null)
            {
                _baseColor = curtainRenderer.sharedMaterial.color;
            }
        }

        private void ApplyAlpha(float alpha)
        {
            if (curtainRenderer == null)
            {
                return;
            }
            EnsurePresentationState();
            Color color = _baseColor;
            color.a = Mathf.Clamp01(alpha);
            _propertyBlock.Clear();
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            curtainRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
