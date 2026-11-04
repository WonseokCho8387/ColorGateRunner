using System;
using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    // Collects the synthesized cues (Tools/Music/synth_ui_sfx.py) and the
    // lobby loop (Tools/Music/synth_lobby_music.py), keeps their import
    // settings and gives every Builder-made control its click cue.
    public static class SoundLibraryBuilder
    {
        public const string SfxFolder = "Assets/Game/Audio/Sfx";
        public const string LobbyMusicPath = "Assets/Game/Audio/Lobby/Lobby.wav";
        public const string AssetPath = "Assets/Game/Audio/SoundLibrary.asset";

        [MenuItem("Tools/Color Gate Runner/Build Sound Library")]
        public static SoundLibraryAsset EnsureAndConfigure()
        {
            var cues = new AudioClip[SoundLibraryAsset.CueCount];
            for (int index = 0; index < cues.Length; index++)
            {
                string path = GetCuePath((SoundCue)index);
                ConfigureImporter(
                    path,
                    AudioClipLoadType.DecompressOnLoad,
                    AudioCompressionFormat.ADPCM,
                    true);
                cues[index] = LoadClip(path);
            }
            ConfigureImporter(
                LobbyMusicPath,
                AudioClipLoadType.Streaming,
                AudioCompressionFormat.Vorbis,
                false);
            AudioClip lobby = LoadClip(LobbyMusicPath);

            SoundLibraryAsset asset =
                AssetDatabase.LoadAssetAtPath<SoundLibraryAsset>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SoundLibraryAsset>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            asset.Configure(cues, lobby);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        internal static string GetCuePath(SoundCue cue)
        {
            return $"{SfxFolder}/{cue}.wav";
        }

        internal static SoundEffectPlayer CreatePlayer(Transform parent)
        {
            var root = new GameObject(
                "SoundEffects",
                typeof(AudioSource),
                typeof(SoundEffectPlayer));
            root.transform.SetParent(parent, false);
            AudioSource source = root.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.priority = 32;
            SoundEffectPlayer player = root.GetComponent<SoundEffectPlayer>();
            player.Configure(source, EnsureAndConfigure());
            return player;
        }

        // Adds one UiButtonSound to every Button and Toggle under root.
        internal static int BindControls(Transform root, SoundEffectPlayer player)
        {
            Selectable[] controls =
                root.GetComponentsInChildren<Selectable>(true);
            int bound = 0;
            for (int index = 0; index < controls.Length; index++)
            {
                Selectable control = controls[index];
                if (!(control is Button) && !(control is Toggle))
                {
                    continue;
                }
                UiButtonSound sound = control.GetComponent<UiButtonSound>();
                if (sound == null)
                {
                    sound = control.gameObject.AddComponent<UiButtonSound>();
                }
                Image image = control.GetComponent<Image>();
                sound.Configure(
                    CueFor(
                        control.gameObject.name,
                        image != null && image.sprite != null
                            ? image.sprite.name
                            : string.Empty,
                        control is Toggle),
                    player);
                bound++;
            }
            return bound;
        }

        // Click cue from the control's Builder name and surface sprite.
        internal static SoundCue CueFor(
            string objectName,
            string spriteName,
            bool isToggle)
        {
            string name = objectName ?? string.Empty;
            if (isToggle)
            {
                return SoundCue.UiToggleOn;
            }
            if (name.EndsWith("NavigationButton", StringComparison.Ordinal))
            {
                return SoundCue.UiTab;
            }
            if (spriteName == "ButtonDanger" ||
                Contains(name, "ConfirmResetProgress") ||
                Contains(name, "ExitConfirm"))
            {
                return SoundCue.UiDanger;
            }
            if (Contains(name, "Cancel") || Contains(name, "Back") ||
                Contains(name, "Close") || Contains(name, "Leave") ||
                Contains(name, "Exit") ||
                name.EndsWith("LobbyButton", StringComparison.Ordinal))
            {
                return SoundCue.UiBack;
            }
            if (Contains(name, "Purchase") || Contains(name, "Coin") ||
                Contains(name, "Ticket") || Contains(name, "Rewarded"))
            {
                return SoundCue.UiPurchase;
            }
            if (spriteName == "ButtonPrimary" || Contains(name, "Play") ||
                Contains(name, "Start") || Contains(name, "Replay") ||
                Contains(name, "Retry") || Contains(name, "Continue") ||
                Contains(name, "Confirm") || Contains(name, "Apply") ||
                Contains(name, "Resume"))
            {
                return SoundCue.UiConfirm;
            }
            return SoundCue.UiTap;
        }

        private static bool Contains(string value, string token)
        {
            return value.IndexOf(token, StringComparison.Ordinal) >= 0;
        }

        private static AudioClip LoadClip(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                throw new InvalidOperationException($"Sound is missing: {path}");
            }
            return clip;
        }

        private static void ConfigureImporter(
            string path,
            AudioClipLoadType loadType,
            AudioCompressionFormat format,
            bool preload)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (!(AssetImporter.GetAtPath(path) is AudioImporter importer))
            {
                return;
            }
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType == loadType &&
                settings.compressionFormat == format &&
                settings.preloadAudioData == preload &&
                importer.loadInBackground == !preload &&
                !importer.forceToMono)
            {
                return;
            }
            settings.loadType = loadType;
            settings.compressionFormat = format;
            settings.quality = 0.5f;
            settings.preloadAudioData = preload;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = !preload;
            importer.forceToMono = false;
            importer.SaveAndReimport();
        }
    }
}
