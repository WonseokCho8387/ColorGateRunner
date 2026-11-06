using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Plays the Stage track locked to runner distance. While the runner is
    // faster than the composed run (Booster) the track speeds up like a
    // fast-forward; it never controls gameplay.
    public sealed class StageMusicPlayer : MonoBehaviour
    {
        internal const float FadeInSeconds = 0.35f;
        internal const float FailureFadeSeconds = 0.6f;
        internal const float FailurePitch = 0.55f;
        internal const float FreeRunPitchPerSecond = 1.5f;
        internal const float ClearFadeSeconds = 2f;

        internal enum MusicState
        {
            Idle,
            Following,
            FreeRunning,
            FailingOut,
            ClearFading,
            Held
        }

        [SerializeField] private AudioSource source;
        [SerializeField] private StageMusicCatalogAsset catalog;

        private StageMusicTimeline _timeline;
        private float _gain;
        private float _gainTarget;
        private float _clock;
        private bool _paused;
        private float _volume = 1f;
        // WebGL reports AudioSource.time from its own estimate, which breaks
        // once pitch changes meet a seek (it read about twice the real
        // position and caused a seek every frame). There the track counts
        // its own playback on the audio device clock instead.
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool _countOwnClock = true;
#else
        private bool _countOwnClock;
#endif
        private bool _useDeviceClock = true;
        private double _lastDspTime = -1d;

        internal MusicState State { get; private set; }
        internal bool HasTrack => _timeline != null;
        internal bool IsPaused => _paused;
        internal StageMusicTimeline Timeline => _timeline;
        internal float Pitch => source.pitch;
        internal float Gain => _gain;
        internal float PlaybackSeconds => _clock;
        internal AudioSource Source => source;
        internal StageMusicCatalogAsset Catalog => catalog;
        internal bool HasRequiredReferences =>
            source != null && catalog != null;
        internal bool CountsOwnClock => _countOwnClock;

        // Tests step time by hand, so the device clock is left out.
        internal void SetCountOwnClockForTests(bool countOwnClock)
        {
            _countOwnClock = countOwnClock;
            _useDeviceClock = false;
            _lastDspTime = -1d;
        }

        internal void Configure(
            AudioSource audioSource,
            StageMusicCatalogAsset musicCatalog)
        {
            source = audioSource;
            catalog = musicCatalog;
        }

        internal void Prepare(int stageNumber)
        {
            Stop();
            _timeline = null;
            if (catalog == null ||
                !catalog.TryGet(stageNumber, out AudioClip clip,
                    out StageMusicTimeline timeline))
            {
                source.clip = null;
                return;
            }
            _timeline = timeline;
            source.clip = clip;
            clip.LoadAudioData();
        }

        // Starts at the musical moment of this distance: 0 at GO, the
        // respawn point after a Continue.
        internal void Begin(float distance, bool fadeIn)
        {
            if (_timeline == null)
            {
                return;
            }
            _clock = _timeline.TimeAtDistance(distance);
            _gain = fadeIn ? 0f : 1f;
            _gainTarget = 1f;
            _paused = false;
            source.pitch = 1f;
            // Seek after Play: a streamed clip may ignore an earlier seek.
            source.Play();
            source.time = ClampToClip(_clock);
            _lastDspTime = AudioSettings.dspTime;
            State = MusicState.Following;
        }

        internal void Tick(
            float deltaTime,
            float distance,
            float runnerSpeed,
            bool finalGatePassed,
            float volume)
        {
            if (State == MusicState.Idle || State == MusicState.Held ||
                _paused)
            {
                return;
            }

            _volume = Mathf.Clamp01(volume);
            _clock += ElapsedPlaybackSeconds(deltaTime) * source.pitch;
            if (!_countOwnClock && source.isPlaying && source.time > 0f)
            {
                // The audio clock is the truth; it keeps running through
                // frame hitches that cap deltaTime.
                _clock = source.time;
            }

            if (State == MusicState.Following)
            {
                if (finalGatePassed ||
                    distance >= _timeline.FinalGateDistance)
                {
                    State = MusicState.FreeRunning;
                }
                else
                {
                    float target = _timeline.TimeAtDistance(distance);
                    if (StageMusicSync.NeedsResync(target, _clock))
                    {
                        _clock = target;
                        source.time = ClampToClip(target);
                    }
                    source.pitch = StageMusicSync.GetPitch(
                        target,
                        _clock,
                        _timeline.SecondsPerDistanceAt(distance) *
                            runnerSpeed);
                }
            }
            if (State == MusicState.FreeRunning ||
                State == MusicState.ClearFading)
            {
                source.pitch = Mathf.MoveTowards(
                    source.pitch,
                    1f,
                    FreeRunPitchPerSecond * deltaTime);
            }
            else if (State == MusicState.FailingOut)
            {
                float step = deltaTime / FailureFadeSeconds;
                source.pitch = Mathf.MoveTowards(
                    source.pitch,
                    FailurePitch,
                    (1f - FailurePitch) * step);
            }

            float rate = State == MusicState.FailingOut
                ? 1f / FailureFadeSeconds
                : State == MusicState.ClearFading
                    ? 1f / ClearFadeSeconds
                    : 1f / FadeInSeconds;
            _gain = Mathf.MoveTowards(_gain, _gainTarget, rate * deltaTime);
            source.volume = _volume * _gain;
            if (State == MusicState.FailingOut && _gain <= 0f)
            {
                source.Pause();
                State = MusicState.Held;
            }
            else if (State == MusicState.ClearFading && _gain <= 0f)
            {
                Stop();
            }
        }

        // The result screen is up: the final chord fades away.
        internal void FadeOut()
        {
            if (State != MusicState.Following &&
                State != MusicState.FreeRunning)
            {
                return;
            }
            _gainTarget = 0f;
            State = MusicState.ClearFading;
        }

        // Volume follows Settings even while the run is paused.
        internal void ApplyVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            if (source != null && State != MusicState.Idle)
            {
                source.volume = _volume * _gain;
            }
        }

        // The run failed: the track winds down like a stopping tape.
        internal void Fail()
        {
            if (State != MusicState.Following &&
                State != MusicState.FreeRunning)
            {
                return;
            }
            _gainTarget = 0f;
            State = MusicState.FailingOut;
        }

        internal void SetPaused(bool paused)
        {
            if (_paused == paused)
            {
                return;
            }
            _paused = paused;
            if (State == MusicState.Idle || State == MusicState.Held)
            {
                return;
            }
            if (paused)
            {
                source.Pause();
            }
            else
            {
                source.UnPause();
                _lastDspTime = AudioSettings.dspTime;
            }
        }

        // The audio device clock keeps running through frame hitches that
        // cap deltaTime; without a running clock deltaTime is used.
        private float ElapsedPlaybackSeconds(float deltaTime)
        {
            if (!_countOwnClock || !_useDeviceClock)
            {
                return deltaTime;
            }
            double now = AudioSettings.dspTime;
            double last = _lastDspTime;
            _lastDspTime = now;
            return last >= 0d && now > last ? (float)(now - last) : deltaTime;
        }

        internal void Stop()
        {
            if (source != null)
            {
                source.Stop();
                source.pitch = 1f;
            }
            _paused = false;
            _gain = 0f;
            _gainTarget = 0f;
            _clock = 0f;
            State = MusicState.Idle;
        }

        private float ClampToClip(float seconds)
        {
            AudioClip clip = source.clip;
            float length = clip != null ? clip.length : 0f;
            return Mathf.Clamp(seconds, 0f, Mathf.Max(0f, length - 0.01f));
        }
    }
}
