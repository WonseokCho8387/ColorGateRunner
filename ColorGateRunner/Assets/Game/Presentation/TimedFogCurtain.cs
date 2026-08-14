using System;
using ColorGateRunner.Core;

namespace ColorGateRunner.Presentation
{
    internal sealed class TimedFogCurtainState
    {
        private float _elapsedSeconds;
        private float _fadeInSeconds;
        private float _fullOpacitySeconds;
        private float _fadeOutSeconds;

        internal bool HasTriggered { get; private set; }
        internal bool IsActive =>
            HasTriggered && _elapsedSeconds < TotalSeconds;
        internal float ElapsedSeconds => _elapsedSeconds;
        internal float TotalSeconds =>
            _fadeInSeconds + _fullOpacitySeconds + _fadeOutSeconds;
        internal float Alpha
        {
            get
            {
                if (!IsActive)
                {
                    return 0f;
                }
                if (_elapsedSeconds < _fadeInSeconds)
                {
                    return _elapsedSeconds / _fadeInSeconds;
                }
                float fadeOutStart =
                    _fadeInSeconds + _fullOpacitySeconds;
                if (_elapsedSeconds <= fadeOutStart)
                {
                    return 1f;
                }
                return 1f -
                    ((_elapsedSeconds - fadeOutStart) / _fadeOutSeconds);
            }
        }

        internal bool TryTrigger(FogCurtainSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (HasTriggered)
            {
                return false;
            }
            _fadeInSeconds = settings.FadeInSeconds;
            _fullOpacitySeconds = settings.FullOpacitySeconds;
            _fadeOutSeconds = settings.FadeOutSeconds;
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

}
