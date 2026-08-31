using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    internal sealed class StageResultSequenceView : MonoBehaviour
    {
        private const float CelebrationDuration = 1.05f;
        private const float RewardRevealInterval = 0.22f;
        private const float RewardRevealDuration = 0.18f;

        [SerializeField] private GameObject clearCelebrationRoot;
        [SerializeField] private CanvasGroup clearCelebrationGroup;
        [SerializeField] private RectTransform victoryEmblem;
        [SerializeField] private Button clearSkipButton;
        [SerializeField] private GameObject clearRewardRoot;
        [SerializeField] private CanvasGroup clearRewardGroup;
        [SerializeField] private RectTransform[] fireworkSparks;
        [SerializeField] private CanvasGroup[] fireworkSparkGroups;
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
            Button skipButton,
            GameObject rewardRoot,
            CanvasGroup rewardGroup,
            RectTransform[] sparks,
            CanvasGroup[] sparkGroups,
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
            clearSkipButton = skipButton;
            clearRewardRoot = rewardRoot;
            clearRewardGroup = rewardGroup;
            fireworkSparks = sparks;
            fireworkSparkGroups = sparkGroups;
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
                clearRewardRoot == null || clearRewardGroup == null ||
                fireworkSparks == null || fireworkSparkGroups == null ||
                fireworkSparks.Length != 16 ||
                fireworkSparkGroups.Length != fireworkSparks.Length ||
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

            for (int index = 0; index < fireworkSparks.Length; index++)
            {
                if (fireworkSparks[index] == null ||
                    fireworkSparkGroups[index] == null)
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
            _celebrationElapsed = 0f;
            _rewardElapsed = 0f;
            _rewardCount = 0;
            clearCelebrationRoot.SetActive(false);
            clearRewardRoot.SetActive(false);
            failureContinueRoot.SetActive(false);
            failureExitConfirmationRoot.SetActive(false);
            failureConsequenceRoot.SetActive(false);
            failureFinalChoiceRoot.SetActive(false);
        }

        internal void PlayCampaignClear(StageResultRewardLine[] rewards)
        {
            ConfigureRewardRows(rewards ?? Array.Empty<StageResultRewardLine>());
            CacheClearButtonVisibility();
            SetClearButtonsVisible(false, false, false);
            _clearAnimating = true;
            _celebrationElapsed = 0f;
            _rewardElapsed = 0f;
            clearRewardRoot.SetActive(false);
            clearCelebrationRoot.SetActive(true);
            clearCelebrationGroup.alpha = 0f;
            victoryEmblem.localScale = Vector3.one * 0.55f;
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
                float progress = Mathf.Clamp01(
                    _celebrationElapsed / CelebrationDuration);
                clearCelebrationGroup.alpha = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01(progress * 3f));
                float punch = 1f + Mathf.Sin(progress * Mathf.PI) * 0.14f;
                victoryEmblem.localScale = Vector3.one *
                    Mathf.Lerp(0.55f, punch, Mathf.SmoothStep(0f, 1f, progress));
                UpdateFireworks(progress);
                if (_celebrationElapsed >= CelebrationDuration)
                {
                    FinishClearCelebration();
                }
                return;
            }

            if (!clearRewardRoot.activeSelf)
            {
                return;
            }

            _rewardElapsed += Mathf.Max(0f, deltaTime);
            clearRewardGroup.alpha = Mathf.Clamp01(_rewardElapsed / 0.2f);
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
            if (_clearAnimating)
            {
                FinishClearCelebration();
            }
        }

        private void FinishClearCelebration()
        {
            _clearAnimating = false;
            clearCelebrationRoot.SetActive(false);
            clearRewardRoot.SetActive(true);
            clearRewardGroup.alpha = 0f;
            _rewardElapsed = 0f;
            ClearCelebrationCompleted?.Invoke();
        }

        private void UpdateFireworks(float progress)
        {
            for (int index = 0; index < fireworkSparks.Length; index++)
            {
                float phase = Mathf.Repeat(progress * 1.65f -
                    (index % 4) * 0.08f, 1f);
                float angle = (index % 8) * Mathf.PI * 0.25f +
                    (index >= 8 ? 0.2f : 0f);
                Vector2 origin = index < 8
                    ? new Vector2(-185f, 48f)
                    : new Vector2(185f, 62f);
                float radius = Mathf.SmoothStep(0f, 170f, phase);
                fireworkSparks[index].anchoredPosition = origin +
                    new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                fireworkSparks[index].localRotation =
                    Quaternion.Euler(0f, 0f, index * 23f + progress * 180f);
                fireworkSparkGroups[index].alpha =
                    Mathf.Sin(phase * Mathf.PI);
            }
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
