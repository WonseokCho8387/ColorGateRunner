using System;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorGateRunner.Editor
{
    internal sealed class DeveloperConsoleWindow : EditorWindow
    {
        private const string WindowTitle = "CGR Developer Console";
        private Vector2 _scroll;
        private int _selectedStage = 1;
        private int _coins;
        private int _shields;
        private int _boosters;
        private int _continueTickets;
        private int _hearts = HeartStatePolicy.MaximumHearts;
        private int _unlimitedMinutes;
        private string _status = "Press Reload to inspect the current save.";
        private MessageType _statusType = MessageType.Info;

        [MenuItem("Tools/Color Gate Runner/Developer Console", priority = 1)]
        internal static void Open()
        {
            DeveloperConsoleWindow window =
                GetWindow<DeveloperConsoleWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(390f, 540f);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            StageCatalogProvider.EnsureConfigured();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "COLOR GATE RUNNER",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                EditorApplication.isPlaying
                    ? "PLAY MODE · changes use the active Product session"
                    : "EDIT MODE · changes are saved before Play",
                EditorStyles.miniLabel);
            EditorGUILayout.Space(8f);

            DrawCampaignSection();
            EditorGUILayout.Space(10f);
            DrawEconomySection();
            EditorGUILayout.Space(10f);
            DrawStatusSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawCampaignSection()
        {
            EditorGUILayout.LabelField("CAMPAIGN", EditorStyles.boldLabel);
            string[] stages = new string[StageCatalog.Count];
            for (int index = 0; index < stages.Length; index++)
            {
                StageDefinition stage = StageCatalog.GetByIndex(index);
                stages[index] = $"STAGE {stage.DisplayNumber} · {stage.Title}";
            }
            _selectedStage = EditorGUILayout.Popup(
                "Stage",
                Mathf.Clamp(_selectedStage - 1, 0, stages.Length - 1),
                stages) + 1;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("PLAY STAGE", GUILayout.Height(28f)))
                {
                    StageDefinition stage =
                        StageCatalog.GetByDisplayNumber(_selectedStage);
                    DeveloperConsolePlayBridge.Request(stage.StageId);
                    SetStatus(
                        EditorApplication.isPlaying
                            ? $"Opening Stage {_selectedStage}."
                            : $"Stage {_selectedStage} queued. Entering Play Mode.",
                        MessageType.Info);
                }
                if (GUILayout.Button("UNLOCK THROUGH", GUILayout.Height(28f)))
                {
                    Mutate(session => session.SetHighestUnlockedForDevelopment(
                        StageCatalog.GetByDisplayNumber(_selectedStage).StageId),
                        $"Unlocked campaign through Stage {_selectedStage}.");
                }
            }

            if (GUILayout.Button("RESET CAMPAIGN PROGRESS"))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Campaign Progress?",
                    "Stage clears, records, unlocks, and lobby milestone " +
                    "progress will reset. Economy values are preserved.",
                    "Reset Campaign",
                    "Cancel"))
                {
                    Mutate(
                        session => session.ResetCampaignForDevelopment(),
                        "Campaign progress reset. Economy preserved.");
                }
            }
        }

        private void DrawEconomySection()
        {
            EditorGUILayout.LabelField("ECONOMY", EditorStyles.boldLabel);
            _coins = Mathf.Max(0, EditorGUILayout.IntField("Coins", _coins));
            _shields = Mathf.Max(
                0,
                EditorGUILayout.IntField("Shield", _shields));
            _boosters = Mathf.Max(
                0,
                EditorGUILayout.IntField("Booster", _boosters));
            _continueTickets = Mathf.Max(
                0,
                EditorGUILayout.IntField(
                    "Continue Tickets",
                    _continueTickets));
            _hearts = EditorGUILayout.IntSlider(
                "Hearts",
                _hearts,
                0,
                HeartStatePolicy.MaximumHearts);
            _unlimitedMinutes = Mathf.Clamp(
                EditorGUILayout.IntField(
                    "Unlimited Hearts (min)",
                    _unlimitedMinutes),
                0,
                60 * 24 * 30);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("APPLY ECONOMY", GUILayout.Height(28f)))
                {
                    Mutate(
                        session => session.SetEconomyForDevelopment(
                            _coins,
                            _shields,
                            _boosters,
                            _continueTickets,
                            _hearts,
                            TimeSpan.FromMinutes(_unlimitedMinutes)),
                        "Economy applied and saved.");
                }
                if (GUILayout.Button("RESET ECONOMY", GUILayout.Height(28f)) &&
                    EditorUtility.DisplayDialog(
                        "Reset Economy?",
                        "Coins, items, Continue Tickets, Hearts, unlimited " +
                        "Hearts, and commerce transaction history will reset. " +
                        "Campaign progress is preserved.",
                        "Reset Economy",
                        "Cancel"))
                {
                    Mutate(
                        session => session.ResetEconomyForDevelopment(),
                        "Economy reset. Campaign preserved.");
                }
            }
        }

        private void DrawStatusSection()
        {
            EditorGUILayout.LabelField("STATUS", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_status, _statusType);
            if (GUILayout.Button("RELOAD CURRENT VALUES"))
            {
                ReloadValues();
            }
            EditorGUILayout.SelectableLabel(
                System.IO.Path.Combine(
                    Application.persistentDataPath,
                    AppRoot.SaveFileName),
                EditorStyles.miniLabel,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        private void ReloadValues()
        {
            if (!TryReadProgression(
                out ProgressionService progression,
                out ProductError error))
            {
                SetStatus(error.Diagnostic, MessageType.Error);
                return;
            }

            LocalEconomyData economy = progression.Economy ??
                LocalEconomyData.CreateDefaults();
            _coins = economy.Coins;
            _shields = economy.ShieldCount;
            _boosters = economy.BoosterCount;
            _continueTickets = economy.ContinueTicketCount;
            HeartStateSnapshot heart = HeartStatePolicy.Read(
                economy,
                DateTime.UtcNow);
            _hearts = heart.Count;
            _unlimitedMinutes = heart.Unlimited
                ? Mathf.Max(
                    1,
                    Mathf.CeilToInt((float)(
                        heart.UnlimitedUntilUtc - DateTime.UtcNow).TotalMinutes))
                : 0;
            if (progression.Campaign != null)
            {
                try
                {
                    _selectedStage = StageCatalog.GetById(
                        progression.Campaign.HighestUnlockedStageId)
                        .DisplayNumber;
                }
                catch (ArgumentException)
                {
                    _selectedStage = 1;
                }
            }
            SetStatus("Current values loaded.", MessageType.Info);
        }

        private void Mutate(
            Func<LocalProductSession, ProductMutationResult> mutation,
            string successMessage)
        {
            if (!TryGetMutableSession(
                out LocalProductSession session,
                out ProductError error))
            {
                SetStatus(error.Diagnostic, MessageType.Error);
                return;
            }

            ProductMutationResult result = mutation(session);
            if (!result.Succeeded)
            {
                SetStatus(result.Error.Diagnostic, MessageType.Error);
                return;
            }

            RefreshRuntimePresentation();
            ReloadValues();
            SetStatus(
                result.Changed ? successMessage : "Values were already current.",
                MessageType.Info);
        }

        private static bool TryGetMutableSession(
            out LocalProductSession session,
            out ProductError error)
        {
            if (EditorApplication.isPlaying &&
                AppRoot.TryGetActive(out AppRoot active) &&
                active.Graph?.ProductSession?.IsReady == true)
            {
                session = active.Graph.ProductSession;
                error = ProductError.None;
                return true;
            }
            if (EditorApplication.isPlaying)
            {
                session = null;
                error = new ProductError(
                    ProductErrorCode.Initialization,
                    "Play Mode does not have a ready AppRoot. Start through " +
                    "Boot or use PLAY STAGE from Edit Mode.",
                    true);
                return false;
            }

            AppServiceGraph graph = AppRoot.CreateDevelopmentGraph();
            AppInitializationResult initialized = graph.Initialization.Initialize();
            if (!initialized.Succeeded)
            {
                session = null;
                error = initialized.Error;
                return false;
            }
            session = graph.ProductSession;
            error = ProductError.None;
            return true;
        }

        private static bool TryReadProgression(
            out ProgressionService progression,
            out ProductError error)
        {
            if (EditorApplication.isPlaying &&
                AppRoot.TryGetActive(out AppRoot active) &&
                active.Graph?.ProductSession?.IsReady == true)
            {
                progression = active.Graph.Progression;
                error = ProductError.None;
                return true;
            }
            if (EditorApplication.isPlaying)
            {
                progression = null;
                error = new ProductError(
                    ProductErrorCode.Initialization,
                    "Play Mode does not have a ready AppRoot. Start through " +
                    "Boot or use PLAY STAGE from Edit Mode.",
                    true);
                return false;
            }

            AppServiceGraph graph = AppRoot.CreateDevelopmentGraph();
            LocalSaveLoadResult load = graph.Save.Load();
            if (!load.Succeeded)
            {
                progression = null;
                error = load.Error;
                return false;
            }
            var loaded = new ProgressionService();
            loaded.LoadOrCreateDefaults(load.Data);
            progression = loaded;
            error = ProductError.None;
            return true;
        }

        private static void RefreshRuntimePresentation()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }
            StageSceneController stage =
                UnityEngine.Object.FindAnyObjectByType<StageSceneController>();
            stage?.RefreshProductStateForDevelopment();
            FrontendSceneController frontend =
                UnityEngine.Object.FindAnyObjectByType<FrontendSceneController>();
            frontend?.RefreshProductStateForDevelopment();
        }

        private void SetStatus(string message, MessageType type)
        {
            _status = string.IsNullOrWhiteSpace(message)
                ? "No diagnostic was provided."
                : message;
            _statusType = type;
            Repaint();
        }
    }

    [InitializeOnLoad]
    internal static class DeveloperConsolePlayBridge
    {
        private const string PendingStageKey =
            "ColorGateRunner.DeveloperConsole.PendingStage";
        private static bool _loadRequested;

        static DeveloperConsolePlayBridge()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        internal static void Request(string stageId)
        {
            SessionState.SetString(PendingStageKey, stageId);
            _loadRequested = false;
            if (!EditorApplication.isPlaying)
            {
                SceneAsset boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    BootSceneBuilder.ScenePath);
                EditorSceneManager.playModeStartScene = boot;
                EditorApplication.isPlaying = true;
            }
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || _loadRequested)
            {
                return;
            }
            string stageId = SessionState.GetString(PendingStageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return;
            }

            StageSceneController stage =
                UnityEngine.Object.FindAnyObjectByType<StageSceneController>();
            if (stage != null)
            {
                if (stage.SelectStageForDevelopment(stageId))
                {
                    SessionState.EraseString(PendingStageKey);
                }
                return;
            }
            if (!AppRoot.TryGetActive(out AppRoot root) ||
                root.Graph?.ProductSession?.IsReady != true ||
                !root.TryQueueDevelopmentCampaignLaunch(stageId))
            {
                return;
            }

            _loadRequested = true;
            SessionState.EraseString(PendingStageKey);
            SceneManager.LoadSceneAsync(
                GrayboxSceneBuilder.ScenePath,
                LoadSceneMode.Single);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                _loadRequested = false;
                SessionState.EraseString(PendingStageKey);
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}
