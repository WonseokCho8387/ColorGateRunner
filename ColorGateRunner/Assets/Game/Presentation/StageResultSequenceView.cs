using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    internal sealed class StageResultSequenceView : MonoBehaviour
    {
        internal const float MinimumClearSkipDelay = 0.1f;
        internal const float CelebrationHoldDuration = 2.35f;
        internal const float CelebrationExitDuration = 0.25f;

        private const float EmblemEntryDuration = 0.7f;
        private const float SkipPromptFadeDuration = 0.18f;
        private const float FireworkBurstDuration = 0.72f;
        private const float FireworkBurstInterval = 0.55f;
        private const float FirstFireworkBurstAt = 0.28f;
        private const float FireworkCoreDuration = 0.3f;
        private const int FireworkBurstCount = 3;
        private const int SparksPerBurst = 12;
        private const float RewardRevealInterval = 0.22f;
        private const float RewardRevealDuration = 0.18f;

        [SerializeField] private GameObject clearCelebrationRoot;
        [SerializeField] private CanvasGroup clearCelebrationGroup;
        [SerializeField] private RectTransform victoryEmblem;
        [SerializeField] private RectTransform[] victoryEmblemEchoes;
        [SerializeField] private CanvasGroup[] victoryEmblemEchoGroups;
        [SerializeField] private Button clearSkipButton;
        [SerializeField] private CanvasGroup clearSkipPromptGroup;
        [SerializeField] private GameObject clearRewardRoot;
        [SerializeField] private CanvasGroup clearRewardGroup;
        [SerializeField] private RectTransform[] fireworkSparks;
        [SerializeField] private CanvasGroup[] fireworkSparkGroups;
        [SerializeField] private RectTransform[] fireworkBurstCores;
        [SerializeField] private CanvasGroup[] fireworkBurstCoreGroups;
        [SerializeField] private GameObject[] rewardRows;
        [SerializeField] private CanvasGroup[] rewardRowGroups;
        [SerializeField] private Image[] rewardRowIcons;
        [SerializeField] private Text[] rewardRowTexts;
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Sprite heartSprite;
        [SerializeField] private Sprite shieldSprite;
        [SerializeField] private Sprite boosterSprite;

        [SerializeField] private GameObject failureContinueRoot;
        [SerializeField] private GameObject failureExitConfirmationRoot;
        [SerializeField] private Text failureExitMessage;
        [SerializeField] private Button failureExitConfirmButton;
        [SerializeField] private Button failureExitCancelButton;
        [SerializeField] private GameObject failureConsequenceRoot;
        [SerializeField] private Text failureConsequenceTitle;
        [SerializeField] private Text failureConsequenceMessage;
        [SerializeField] private Button failureConsequenceContinueButton;
        [SerializeField] private GameObject failureFinalChoiceRoot;
        [SerializeField] private Text failureFinalMessage;
        [SerializeField] private Button failureRetryButton;
        [SerializeField] private Button failureLobbyButton;

        private float _celebrationElapsed;
        private float _rewardElapsed;
        private int _rewardCount;
        private bool _clearAnimating;
        private bool _clearExitTransitioning;
        private float _clearExitStartedAt;
        private bool _restoreNext;
        private bool _restoreReplay;
        private bool _restoreLobby;

        internal event Action ClearCelebrationCompleted;

        internal Button FailureExitConfirmButton => failureExitConfirmButton;
        internal Button FailureExitCancelButton => failureExitCancelButton;
        internal Button FailureConsequenceContinueButton =>
            failureConsequenceContinueButton;
        internal Button ClearSkipButton => clearSkipButton;
        internal GameObject ClearCelebrationRoot => clearCelebrationRoot;
        internal GameObject ClearRewardRoot => clearRewardRoot;
        internal bool CanSkipClearCelebration =>
            _clearAnimating && !_clearExitTransitioning &&
            _celebrationElapsed >= MinimumClearSkipDelay;
        internal bool ClearExitTransitioning => _clearExitTransitioning;
        internal float ClearCelebrationAlpha => clearCelebrationGroup.alpha;
        internal float ClearRewardAlpha => clearRewardGroup.alpha;
        internal float ClearSkipPromptAlpha => clearSkipPromptGroup.alpha;
        internal int FireworkSparkCount => fireworkSparks?.Length ?? 0;
        internal int FireworkBurstCoreCount =>
            fireworkBurstCores?.Length ?? 0;
        internal int VictoryEmblemEchoCount =>
            victoryEmblemEchoes?.Length ?? 0;
        internal bool FireworkSpritesAssigned
        {
            get
            {
                for (int index = 0; index < fireworkSparks.Length; index++)
                {
                    Image image = fireworkSparks[index].GetComponent<Image>();
                    if (image == null || image.sprite == null)
                    {
                        return false;
                    }
                }

                for (int index = 0; index < fireworkBurstCores.Length; index++)
                {
                    Image image =
                        fireworkBurstCores[index].GetComponent<Image>();
                    if (image == null || image.sprite == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        internal int VisibleFireworkSparkCount
        {
            get
            {
                int result = 0;
                for (int index = 0; index < fireworkSparkGroups.Length; index++)
                {
                    if (fireworkSparkGroups[index].alpha > 0.01f)
                    {
                        result++;
                    }
                }
                return result;
            }
        }
        internal int VisibleFireworkBurstCoreCount
        {
            get
            {
                int result = 0;
                for (int index = 0;
                    index < fireworkBurstCoreGroups.Length;
                    index++)
                {
                    if (fireworkBurstCoreGroups[index].alpha > 0.01f)
                    {
                        result++;
                    }
                }
                return result;
            }
        }
        internal GameObject FailureContinueRoot => failureContinueRoot;
        internal GameObject FailureExitConfirmationRoot =>
            failureExitConfirmationRoot;
        internal GameObject FailureConsequenceRoot => failureConsequenceRoot;
        internal GameObject FailureFinalChoiceRoot => failureFinalChoiceRoot;
        internal int VisibleRewardRowCount
        {
            get
            {
                int result = 0;
                for (int index = 0; index < rewardRows.Length; index++)
                {
                    if (rewardRows[index].activeSelf &&
                        rewardRowGroups[index].alpha > 0.99f)
                    {
                        result++;
                    }
                }
                return result;
            }
        }

        internal void Configure(
            GameObject celebrationRoot,
            CanvasGroup celebrationGroup,
            RectTransform emblem,
            RectTransform[] emblemEchoes,
            CanvasGroup[] emblemEchoGroups,
            Button skipButton,
            CanvasGroup skipPromptGroup,
            GameObject rewardRoot,
            CanvasGroup rewardGroup,
            RectTransform[] sparks,
            CanvasGroup[] sparkGroups,
            RectTransform[] burstCores,
            CanvasGroup[] burstCoreGroups,
            GameObject[] rows,
            CanvasGroup[] rowGroups,
            Image[] rowIcons,
            Text[] rowTexts,
            Sprite coin,
            Sprite heart,
            Sprite shield,
            Sprite booster,
            GameObject continueRoot,
            GameObject exitConfirmationRoot,
            Text exitMessage,
            Button exitConfirm,
            Button exitCancel,
            GameObject consequenceRoot,
            Text consequenceTitle,
            Text consequenceMessage,
            Button consequenceContinue,
            GameObject finalChoiceRoot,
            Text finalMessage,
            Button retry,
            Button lobby)
        {
            clearCelebrationRoot = celebrationRoot;
            clearCelebrationGroup = celebrationGroup;
            victoryEmblem = emblem;
            victoryEmblemEchoes = emblemEchoes;
            victoryEmblemEchoGroups = emblemEchoGroups;
            clearSkipButton = skipButton;
            clearSkipPromptGroup = skipPromptGroup;
            clearRewardRoot = rewardRoot;
            clearRewardGroup = rewardGroup;
            fireworkSparks = sparks;
            fireworkSparkGroups = sparkGroups;
            fireworkBurstCores = burstCores;
            fireworkBurstCoreGroups = burstCoreGroups;
            rewardRows = rows;
            rewardRowGroups = rowGroups;
            rewardRowIcons = rowIcons;
            rewardRowTexts = rowTexts;
            coinSprite = coin;
            heartSprite = heart;
            shieldSprite = shield;
            boosterSprite = booster;
            failureContinueRoot = continueRoot;
            failureExitConfirmationRoot = exitConfirmationRoot;
            failureExitMessage = exitMessage;
            failureExitConfirmButton = exitConfirm;
            failureExitCancelButton = exitCancel;
            failureConsequenceRoot = consequenceRoot;
            failureConsequenceTitle = consequenceTitle;
            failureConsequenceMessage = consequenceMessage;
            failureConsequenceContinueButton = consequenceContinue;
            failureFinalChoiceRoot = finalChoiceRoot;
            failureFinalMessage = finalMessage;
            failureRetryButton = retry;
            failureLobbyButton = lobby;
            ResetView();
        }

        internal bool HasRequiredReferences()
        {
            if (clearCelebrationRoot == null || clearCelebrationGroup == null ||
                victoryEmblem == null || clearSkipButton == null ||
                victoryEmblemEchoes == null ||
                victoryEmblemEchoGroups == null ||
                victoryEmblemEchoes.Length != 2 ||
                victoryEmblemEchoGroups.Length !=
                    victoryEmblemEchoes.Length ||
                clearSkipPromptGroup == null ||
                clearRewardRoot == null || clearRewardGroup == null ||
                fireworkSparks == null || fireworkSparkGroups == null ||
                fireworkSparks.Length != 36 ||
                fireworkSparkGroups.Length != fireworkSparks.Length ||
                fireworkBurstCores == null ||
                fireworkBurstCoreGroups == null ||
                fireworkBurstCores.Length != FireworkBurstCount ||
                fireworkBurstCoreGroups.Length != fireworkBurstCores.Length ||
                rewardRows == null || rewardRowGroups == null ||
                rewardRowIcons == null || rewardRowTexts == null ||
                rewardRows.Length != 5 ||
                rewardRowGroups.Length != rewardRows.Length ||
                rewardRowIcons.Length != rewardRows.Length ||
                rewardRowTexts.Length != rewardRows.Length ||
                coinSprite == null || heartSprite == null ||
                shieldSprite == null || boosterSprite == null ||
                failureContinueRoot == null ||
                failureExitConfirmationRoot == null ||
                failureExitMessage == null ||
                failureExitConfirmButton == null ||
                failureExitCancelButton == null ||
                failureConsequenceRoot == null ||
                failureConsequenceTitle == null ||
                failureConsequenceMessage == null ||
                failureConsequenceContinueButton == null ||
                failureFinalChoiceRoot == null ||
                failureFinalMessage == null ||
                failureRetryButton == null || failureLobbyButton == null)
            {
                return false;
            }

            for (int index = 0; index < victoryEmblemEchoes.Length; index++)
            {
                if (victoryEmblemEchoes[index] == null ||
                    victoryEmblemEchoGroups[index] == null)
                {
                    return false;
                }
            }

            for (int index = 0; index < fireworkSparks.Length; index++)
            {
                if (fireworkSparks[index] == null ||
                    fireworkSparkGroups[index] == null ||
                    fireworkSparks[index].GetComponent<Image>()?.sprite == null)
                {
                    return false;
                }
            }

            for (int index = 0; index < fireworkBurstCores.Length; index++)
            {
                if (fireworkBurstCores[index] == null ||
                    fireworkBurstCoreGroups[index] == null ||
                    fireworkBurstCores[index].GetComponent<Image>()?.sprite ==
                        null)
                {
                    return false;
                }
            }

            for (int index = 0; index < rewardRows.Length; index++)
            {
                if (rewardRows[index] == null ||
                    rewardRowGroups[index] == null ||
                    rewardRowIcons[index] == null ||
                    rewardRowTexts[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        internal void ResetView()
        {
            _clearAnimating = false;
            _clearExitTransitioning = false;
            _clearExitStartedAt = 0f;
            _celebrationElapsed = 0f;
            _rewardElapsed = 0f;
            _rewardCount = 0;
            clearCelebrationRoot.SetActive(false);
            clearRewardRoot.SetActive(false);
            failureContinueRoot.SetActive(false);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(false);
            clearSkipButton.interactable = false;
            clearSkipPromptGroup.alpha = 0f;
        }

        internal void PlayCampaignClear(StageResultRewardLine[] rewards)
        {
            ConfigureRewardRows(rewards ?? Array.Empty<StageResultRewardLine>());
            CacheClearButtonVisibility();
            SetClearButtonsVisible(false, false, false);
            _clearAnimating = true;
            _clearExitTransitioning = false;
            _clearExitStartedAt = 0f;
            _celebrationElapsed = 0f;
            _rewardElapsed = 0f;
            clearRewardRoot.SetActive(false);
            clearCelebrationRoot.SetActive(true);
            clearCelebrationGroup.alpha = 0f;
            clearSkipButton.interactable = false;
            clearSkipPromptGroup.alpha = 0f;
            victoryEmblem.localScale = Vector3.one * 0.48f;
            victoryEmblem.localRotation = Quaternion.Euler(0f, 0f, -8f);
            for (int index = 0; index < victoryEmblemEchoes.Length; index++)
            {
                victoryEmblemEchoes[index].anchoredPosition =
                    new Vector2(index == 0 ? -26f : 26f, 0f);
                victoryEmblemEchoes[index].localScale = Vector3.one * 1.2f;
                victoryEmblemEchoGroups[index].alpha = 0f;
            }
            UpdateFireworks(0f);
        }

        internal void ShowExperimentClear()
        {
            _clearAnimating = false;
            clearCelebrationRoot.SetActive(false);
            clearRewardRoot.SetActive(true);
            clearRewardGroup.alpha = 1f;
            for (int index = 0; index < rewardRows.Length; index++)
            {
                rewardRows[index].SetActive(false);
            }
        }

        internal void Tick(float deltaTime)
        {
            if (_clearAnimating)
            {
                _celebrationElapsed += Mathf.Max(0f, deltaTime);
                UpdateClearCelebration();
                if (_clearAnimating && !_clearExitTransitioning &&
                    _celebrationElapsed >= CelebrationHoldDuration)
                {
                    BeginClearExit(CelebrationHoldDuration);
                    UpdateClearExit();
                }
                return;
            }

            if (!clearRewardRoot.activeSelf)
            {
                return;
            }

            _rewardElapsed += Mathf.Max(0f, deltaTime);
            // The reward page already faded fully in during the celebration exit.
            // Keep it opaque while the individual reward rows reveal so the page
            // does not flash dark again on its first reward-frame tick.
            clearRewardGroup.alpha = 1f;
            for (int index = 0; index < _rewardCount; index++)
            {
                float local = Mathf.Clamp01(
                    (_rewardElapsed - index * RewardRevealInterval) /
                    RewardRevealDuration);
                rewardRowGroups[index].alpha = local;
                rewardRows[index].transform.localScale =
                    Vector3.one * Mathf.Lerp(0.82f, 1f,
                        Mathf.SmoothStep(0f, 1f, local));
            }

            float buttonsAt = Mathf.Max(0.35f,
                _rewardCount * RewardRevealInterval + 0.12f);
            if (_rewardElapsed >= buttonsAt)
            {
                RestoreClearButtons();
            }
        }

        internal void SkipClearCelebrationNow()
        {
            SkipClearCelebration();
        }

        internal void ShowFailureContinue()
        {
            failureContinueRoot.SetActive(true);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(false);
            failureRetryButton.gameObject.SetActive(true);
            failureLobbyButton.gameObject.SetActive(false);
        }

        internal void ShowFailureExitConfirmation()
        {
            failureContinueRoot.SetActive(false);
            failureExitConfirmationRoot.SetActive(true);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(false);
            failureRetryButton.gameObject.SetActive(false);
            failureLobbyButton.gameObject.SetActive(false);
            failureExitMessage.text =
                "1 HEART USED FOR THIS ATTEMPT WILL NOT BE RETURNED.\n\nGIVE UP?";
        }

        internal void ShowFailureConsequence(FailureConsequencePage page)
        {
            failureContinueRoot.SetActive(false);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(true);
            failureFinalChoiceRoot.SetActive(false);
            failureRetryButton.gameObject.SetActive(false);
            failureLobbyButton.gameObject.SetActive(false);
            failureConsequenceTitle.text = page.Title;
            failureConsequenceMessage.text = page.Message;
        }

        internal void ShowFailureFinalChoice(string heartStatus)
        {
            failureContinueRoot.SetActive(false);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(true);
            failureRetryButton.gameObject.SetActive(true);
            failureLobbyButton.gameObject.SetActive(true);
            failureFinalMessage.text =
                $"{heartStatus}\n\nTHE HEART USED FOR THIS ATTEMPT WAS NOT RETURNED.\n" +
                "TRY AGAIN OR RETURN TO LOBBY.";
        }

        internal void ShowExperimentFailure()
        {
            failureContinueRoot.SetActive(true);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(false);
            failureRetryButton.gameObject.SetActive(true);
            failureLobbyButton.gameObject.SetActive(true);
        }

        private void ConfigureRewardRows(StageResultRewardLine[] rewards)
        {
            _rewardCount = Mathf.Min(rewardRows.Length, rewards.Length);
            for (int index = 0; index < rewardRows.Length; index++)
            {
                bool active = index < _rewardCount;
                rewardRows[index].SetActive(active);
                rewardRowGroups[index].alpha = 0f;
                rewardRows[index].transform.localScale = Vector3.one * 0.82f;
                if (!active)
                {
                    continue;
                }

                StageResultRewardLine reward = rewards[index];
                rewardRowIcons[index].sprite = SpriteFor(reward.Kind);
                rewardRowTexts[index].text =
                    $"{reward.Label}   +{reward.Amount}";
            }
        }

        private Sprite SpriteFor(StageResultRewardKind kind)
        {
            return kind switch
            {
                StageResultRewardKind.Coin => coinSprite,
                StageResultRewardKind.Shield => shieldSprite,
                StageResultRewardKind.Booster => boosterSprite,
                StageResultRewardKind.Heart => heartSprite,
                _ => coinSprite
            };
        }

        private void SkipClearCelebration()
        {
            if (CanSkipClearCelebration)
            {
                BeginClearExit(_celebrationElapsed);
            }
        }

        private void BeginClearExit(float startedAt)
        {
            if (_clearExitTransitioning)
            {
                return;
            }

            _clearExitTransitioning = true;
            _clearExitStartedAt = startedAt;
            clearSkipButton.interactable = false;
            clearRewardRoot.SetActive(true);
            clearRewardGroup.alpha = 0f;
        }

        private void UpdateClearCelebration()
        {
            UpdateEmblem(_celebrationElapsed);
            UpdateFireworks(_celebrationElapsed);

            if (!_clearExitTransitioning)
            {
                clearCelebrationGroup.alpha = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(_celebrationElapsed / 0.16f));
                bool canSkip =
                    _celebrationElapsed >= MinimumClearSkipDelay;
                clearSkipButton.interactable = canSkip;
                clearSkipPromptGroup.alpha = canSkip
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(
                            (_celebrationElapsed - MinimumClearSkipDelay) /
                            SkipPromptFadeDuration))
                    : 0f;
                return;
            }

            UpdateClearExit();
        }

        private void UpdateClearExit()
        {
            float progress = Mathf.Clamp01(
                (_celebrationElapsed - _clearExitStartedAt) /
                CelebrationExitDuration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            clearCelebrationGroup.alpha = 1f - eased;
            clearRewardGroup.alpha = eased;
            if (progress >= 1f)
            {
                FinishClearCelebration();
            }
        }

        private void UpdateEmblem(float elapsed)
        {
            float entry = Mathf.Clamp01(elapsed / EmblemEntryDuration);
            float scale;
            if (entry < 0.58f)
            {
                scale = Mathf.Lerp(
                    0.48f,
                    1.18f,
                    Mathf.SmoothStep(0f, 1f, entry / 0.58f));
            }
            else
            {
                scale = Mathf.Lerp(
                    1.18f,
                    1f,
                    Mathf.SmoothStep(0f, 1f, (entry - 0.58f) / 0.42f));
            }
            if (entry >= 1f)
            {
                scale += Mathf.Sin((elapsed - EmblemEntryDuration) * 4.5f) *
                    0.018f;
            }
            victoryEmblem.localScale = Vector3.one * scale;
            victoryEmblem.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(-8f, 0f, Mathf.SmoothStep(0f, 1f, entry)));

            for (int index = 0; index < victoryEmblemEchoes.Length; index++)
            {
                float side = index == 0 ? -1f : 1f;
                victoryEmblemEchoes[index].anchoredPosition = new Vector2(
                    side * Mathf.Lerp(26f, 0f, entry),
                    Mathf.Sin(elapsed * 3.8f + index) * 3f);
                victoryEmblemEchoes[index].localScale = Vector3.one *
                    (scale + 0.07f + index * 0.035f);
                float entryEcho = Mathf.Sin(entry * Mathf.PI) * 0.42f;
                float settledPulse = entry >= 1f
                    ? 0.08f + Mathf.Sin(elapsed * 5f + index) * 0.025f
                    : 0f;
                victoryEmblemEchoGroups[index].alpha =
                    Mathf.Clamp01(entryEcho + settledPulse);
            }
        }

        private void FinishClearCelebration()
        {
            _clearAnimating = false;
            _clearExitTransitioning = false;
            clearCelebrationRoot.SetActive(false);
            clearRewardRoot.SetActive(true);
            clearRewardGroup.alpha = 1f;
            _rewardElapsed = 0f;
            ClearCelebrationCompleted?.Invoke();
        }

        private void UpdateFireworks(float elapsed)
        {
            for (int index = 0; index < fireworkSparks.Length; index++)
            {
                int burst = index / SparksPerBurst;
                int spark = index % SparksPerBurst;
                float burstStart = FirstFireworkBurstAt +
                    burst * FireworkBurstInterval;
                float phase = (elapsed - burstStart) /
                    FireworkBurstDuration;
                if (phase < 0f || phase > 1f)
                {
                    fireworkSparkGroups[index].alpha = 0f;
                    continue;
                }

                float angle = spark * Mathf.PI * 2f / SparksPerBurst +
                    burst * 0.11f;
                Vector2 origin = FireworkOrigin(burst);
                float radius = Mathf.SmoothStep(28f, 285f, phase);
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle));
                float fall = phase * phase * 48f;
                fireworkSparks[index].anchoredPosition = origin +
                    direction * radius + Vector2.down * fall;
                fireworkSparks[index].localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle * Mathf.Rad2Deg - 90f);
                float pulse = Mathf.Sin(phase * Mathf.PI);
                fireworkSparks[index].localScale = Vector3.one *
                    Mathf.Lerp(0.82f, 1.35f, pulse);
                fireworkSparkGroups[index].alpha =
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(phase * 10f)) *
                    (1f - Mathf.SmoothStep(0f, 1f,
                        Mathf.Clamp01((phase - 0.5f) * 2f)));
            }

            for (int burst = 0; burst < fireworkBurstCores.Length; burst++)
            {
                float burstStart = FirstFireworkBurstAt +
                    burst * FireworkBurstInterval;
                float phase = (elapsed - burstStart) /
                    FireworkCoreDuration;
                if (phase < 0f || phase > 1f)
                {
                    fireworkBurstCoreGroups[burst].alpha = 0f;
                    continue;
                }

                fireworkBurstCores[burst].anchoredPosition =
                    FireworkOrigin(burst);
                fireworkBurstCores[burst].localScale = Vector3.one *
                    Mathf.Lerp(0.45f, 1.5f,
                        Mathf.SmoothStep(0f, 1f, phase));
                fireworkBurstCoreGroups[burst].alpha =
                    Mathf.Sin(phase * Mathf.PI);
            }
        }

        private static Vector2 FireworkOrigin(int burst)
        {
            return burst switch
            {
                0 => new Vector2(-255f, 62f),
                1 => new Vector2(255f, 92f),
                _ => new Vector2(0f, 205f)
            };
        }

        private void CacheClearButtonVisibility()
        {
            Transform panel = clearRewardRoot.transform.parent;
            _restoreNext = FindButton(panel, "ClearContinueButton")?.gameObject
                .activeSelf == true;
            _restoreReplay = FindButton(panel, "ReplayButton")?.gameObject
                .activeSelf == true;
            _restoreLobby = FindButton(panel, "ClearLobbyButton")?.gameObject
                .activeSelf == true;
        }

        private void SetClearButtonsVisible(bool next, bool replay, bool lobby)
        {
            Transform panel = clearRewardRoot.transform.parent;
            SetButtonVisible(panel, "ClearContinueButton", next);
            SetButtonVisible(panel, "ReplayButton", replay);
            SetButtonVisible(panel, "ClearLobbyButton", lobby);
        }

        private void RestoreClearButtons()
        {
            SetClearButtonsVisible(_restoreNext, _restoreReplay, _restoreLobby);
        }

        private static Button FindButton(Transform parent, string name)
        {
            Transform value = parent.Find(name);
            return value == null ? null : value.GetComponent<Button>();
        }

        private static void SetButtonVisible(
            Transform parent,
            string name,
            bool visible)
        {
            Button button = FindButton(parent, name);
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }
    }
}
