using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Cue clips and the lobby loop; built by SoundLibraryBuilder.
    public sealed class SoundLibraryAsset : ScriptableObject
    {
        [SerializeField] private AudioClip[] cueClips = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip lobbyMusic;

        internal static int CueCount => Enum.GetValues(typeof(SoundCue)).Length;
        internal AudioClip LobbyMusic => lobbyMusic;

        internal void Configure(AudioClip[] cues, AudioClip lobby)
        {
            cueClips = cues ?? Array.Empty<AudioClip>();
            lobbyMusic = lobby;
        }

        internal AudioClip GetClip(SoundCue cue)
        {
            int index = (int)cue;
            return index >= 0 && index < cueClips.Length
                ? cueClips[index]
                : null;
        }

        internal bool HasAllClips
        {
            get
            {
                if (cueClips.Length != CueCount || lobbyMusic == null)
                {
                    return false;
                }
                for (int index = 0; index < cueClips.Length; index++)
                {
                    if (cueClips[index] == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
