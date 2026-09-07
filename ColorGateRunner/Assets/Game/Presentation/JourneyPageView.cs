using System;
using System.Collections;
using System.Collections.Generic;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class JourneyPageView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Text progressText;
        [SerializeField] private JourneyMilestoneView[] milestoneViews;
        [SerializeField] private JourneyChapterView[] chapterViews;
        [SerializeField] private JourneyLeagueView leagueView;

        private int _focusMilestoneIndex;

        internal event Action<string> ChapterSelectionRequested;

        internal RectTransform ContentRoot => contentRoot;
        internal ScrollRect ScrollRect => scrollRect;
        internal JourneyMilestoneView[] MilestoneViews => milestoneViews;
        internal JourneyChapterView[] ChapterViews => chapterViews;
        internal JourneyLeagueView LeagueView => leagueView;

        internal void Configure(
            ScrollRect configuredScrollRect,
            RectTransform configuredContentRoot,
            Text configuredProgressText,
            JourneyMilestoneView[] configuredMilestoneViews,
            JourneyChapterView[] configuredChapterViews,
            JourneyLeagueView configuredLeagueView)
        {
            scrollRect = configuredScrollRect;
            contentRoot = configuredContentRoot;
            progressText = configuredProgressText;
            milestoneViews = configuredMilestoneViews;
            chapterViews = configuredChapterViews;
            leagueView = configuredLeagueView;
        }

        private void Awake()
        {
            SetChapterListeners(true);
        }

        private void OnDestroy()
        {
            SetChapterListeners(false);
        }

        internal bool HasRequiredReferences()
        {
            return string.IsNullOrEmpty(FindMissingReferenceGroup());
        }

        internal string FindMissingReferenceGroup()
        {
            if (scrollRect == null || contentRoot == null ||
                scrollRect.content != contentRoot || progressText == null)
            {
                return "scroll or progress";
            }
            if (
                milestoneViews == null ||
                milestoneViews.Length !=
                    JourneyMilestonePresentation.TotalMilestones)
            {
                return "milestone array";
            }
            if (chapterViews == null ||
                chapterViews.Length != LobbyChapterPolicy.ChapterCount ||
                leagueView == null || !leagueView.HasRequiredReferences())
            {
                return "chapter or league array";
            }
            for (int index = 0; index < milestoneViews.Length; index++)
            {
                if (milestoneViews[index] == null ||
                    !milestoneViews[index].HasRequiredReferences())
                {
                    return $"milestone {index}";
                }
            }
            for (int index = 0; index < chapterViews.Length; index++)
            {
                if (chapterViews[index] == null ||
                    !chapterViews[index].HasRequiredReferences())
                {
                    return $"chapter {index}";
                }
            }
            return string.Empty;
        }

        internal void Bind(
            ProgressionService progression,
            int availableStageCount,
            bool leagueActive)
        {
            if (progression?.Lobby == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int applied = Mathf.Clamp(
                progression.Lobby.AppliedMilestoneCount,
                0,
                JourneyMilestonePresentation.TotalMilestones);
            IReadOnlyList<JourneyMilestoneCardModel> models =
                JourneyMilestonePresentation.Create(applied, availableStageCount);
            _focusMilestoneIndex = Mathf.Clamp(
                applied,
                0,
                JourneyMilestonePresentation.TotalMilestones - 1);
            for (int index = 0; index < milestoneViews.Length; index++)
            {
                milestoneViews[index].Bind(models[index]);
            }
            string selectedThemeId = LobbyChapterPolicy.ResolveSelectedThemeId(
                progression.Lobby.SelectedLobbyThemeId,
                applied);
            for (int index = 0; index < chapterViews.Length; index++)
            {
                chapterViews[index].Bind(applied, selectedThemeId);
            }
            leagueView.Bind(leagueActive, availableStageCount);
            ApplyDynamicLayout(availableStageCount);
            progressText.text = applied >=
                JourneyMilestonePresentation.TotalMilestones
                ? "18 / 18  JOURNEY COMPLETE"
                : $"{applied} / 18  •  NEXT STAGE {(applied + 1) * 2}";
        }

        internal void Bind(
            ProgressionService progression,
            int availableStageCount)
        {
            Bind(progression, availableStageCount, false);
        }

        private void ApplyDynamicLayout(int availableStageCount)
        {
            int availableMilestones = Mathf.Clamp(
                availableStageCount / 2,
                0,
                JourneyMilestonePresentation.TotalMilestones);
            int totalSlots =
                JourneyMilestonePresentation.TotalMilestones +
                LobbyChapterPolicy.ChapterCount + 1;
            int slot = 0;
            bool leaguePlaced = false;
            for (int chapter = 0; chapter < chapterViews.Length; chapter++)
            {
                PositionChapter(chapter, slot++, totalSlots);
                int first = chapter *
                    JourneyMilestonePresentation.MilestonesPerChapter;
                int end = first +
                    JourneyMilestonePresentation.MilestonesPerChapter;
                for (int milestone = first; milestone < end; milestone++)
                {
                    PositionMilestone(milestone, slot++, totalSlots);
                    if (milestone + 1 == availableMilestones)
                    {
                        PositionLeague(slot++, totalSlots);
                        leaguePlaced = true;
                    }
                }
            }
            if (!leaguePlaced)
            {
                PositionLeague(1, totalSlots);
            }
        }

        private void PositionChapter(int chapter, int slot, int totalSlots)
        {
            chapterViews[chapter].SetVerticalPosition(
                GetSlotCenter(slot, totalSlots),
                0.0205f);
        }

        private void PositionMilestone(int milestone, int slot, int totalSlots)
        {
            milestoneViews[milestone].SetVerticalPosition(
                GetSlotCenter(slot, totalSlots),
                0.0175f);
        }

        private void PositionLeague(int slot, int totalSlots)
        {
            leagueView.SetVerticalPosition(
                GetSlotCenter(slot, totalSlots),
                0.0185f);
        }

        private static float GetSlotCenter(int slot, int totalSlots)
        {
            return 0.025f + (slot * (0.95f / (totalSlots - 1)));
        }

        private void SetChapterListeners(bool add)
        {
            if (chapterViews == null)
            {
                return;
            }
            for (int index = 0; index < chapterViews.Length; index++)
            {
                if (chapterViews[index] == null)
                {
                    continue;
                }
                if (add)
                {
                    chapterViews[index].SelectionRequested +=
                        ForwardChapterSelection;
                }
                else
                {
                    chapterViews[index].SelectionRequested -=
                        ForwardChapterSelection;
                }
            }
        }

        private void ForwardChapterSelection(string themeId)
        {
            ChapterSelectionRequested?.Invoke(themeId);
        }

        internal void OnPageShown()
        {
            if (!HasRequiredReferences())
            {
                return;
            }
            StartCoroutine(FocusNextFrame());
        }

        private IEnumerator FocusNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            scrollRect.verticalNormalizedPosition =
                JourneyMilestonePresentation.TotalMilestones <= 1
                    ? 1f
                    : _focusMilestoneIndex /
                        (float)(JourneyMilestonePresentation.TotalMilestones - 1);
        }
    }

}
