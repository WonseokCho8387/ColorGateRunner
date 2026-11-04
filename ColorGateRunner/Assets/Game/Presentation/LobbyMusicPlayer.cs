using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Calm Frontend loop at the persisted Music volume; fades when a Stage
    // loads and comes back if the load fails.
    public sealed class LobbyMusicPlayer : MonoBehaviour
    {
        internal const float FadeInSeconds = 1.5f;
        internal const float FadeOutSeconds = 0.6f;

        [SerializeField] private AudioSource source;

        private float _gain;
        private float _gainTarget = 1f;

        internal AudioSource Source => source;
        internal float Gain => _gain;
        internal bool FadingOut => _gainTarget <= 0f;
        internal bool HasRequiredReferences =>
            source != null && source.clip != null && source.loop;

        internal void Configure(AudioSource audioSource)
        {
            source = audioSource;
        }

        private void Start()
        {
            if (source != null && source.clip != null)
            {
                _gain = 0f;
                source.volume = 0f;
                source.Play();
            }
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        internal void Tick(float deltaTime)
        {
            if (source == null)
            {
                return;
            }
            float rate = _gainTarget > _gain
                ? 1f / FadeInSeconds
                : 1f / FadeOutSeconds;
            _gain = Mathf.MoveTowards(_gain, _gainTarget, rate * deltaTime);
            source.volume = SoundEffectPlayer.MusicVolume * _gain;
        }

        internal void FadeOut()
        {
            _gainTarget = 0f;
        }

        internal void Resume()
        {
            _gainTarget = 1f;
            if (source != null && source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }
    }
}
