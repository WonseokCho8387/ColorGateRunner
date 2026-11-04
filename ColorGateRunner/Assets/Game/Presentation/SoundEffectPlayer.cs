using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // One-shot cues at the persisted SFX volume.
    public sealed class SoundEffectPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private SoundLibraryAsset library;

        private readonly int[] _counts = new int[SoundLibraryAsset.CueCount];

        internal SoundCue LastCue { get; private set; }
        internal int PlayCount { get; private set; }
        internal int CountOf(SoundCue cue) => _counts[(int)cue];
        internal AudioSource Source => source;
        internal SoundLibraryAsset Library => library;
        internal bool HasRequiredReferences =>
            source != null && library != null;

        internal void Configure(AudioSource audioSource, SoundLibraryAsset sounds)
        {
            source = audioSource;
            library = sounds;
        }

        internal void Play(SoundCue cue)
        {
            LastCue = cue;
            PlayCount++;
            _counts[(int)cue]++;
            AudioClip clip = library != null ? library.GetClip(cue) : null;
            float volume = SfxVolume;
            if (clip == null || source == null || volume <= 0f)
            {
                return;
            }
            source.PlayOneShot(clip, volume);
        }

        // Gate judgment sound for one recorded combo step.
        internal void PlayGateStep(ComboStep step)
        {
            switch (step)
            {
                case ComboStep.Perfect:
                    Play(SoundCue.GatePerfect);
                    break;
                case ComboStep.Great:
                    Play(SoundCue.GateGreat);
                    break;
                case ComboStep.Good:
                    Play(SoundCue.GateGood);
                    break;
            }
        }

        internal static float SfxVolume =>
            AppRoot.TryGetActive(out AppRoot root) &&
            root.CurrentSettings != null
                ? Mathf.Clamp01(root.CurrentSettings.SfxVolume)
                : 1f;

        internal static float MusicVolume =>
            AppRoot.TryGetActive(out AppRoot root) &&
            root.CurrentSettings != null
                ? Mathf.Clamp01(root.CurrentSettings.MusicVolume)
                : 1f;
    }
}
