using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class JourneyLeagueView : MonoBehaviour
    {
        [SerializeField] private Text stateText;
        [SerializeField] private Text descriptionText;

        internal void Configure(Text state, Text description)
        {
            stateText = state;
            descriptionText = description;
        }

        internal void Bind(bool active, int liveStageCount)
        {
            stateText.text = active ? "LEAGUE ACTIVE" : "LEAGUE LOCKED";
            descriptionText.text = active
                ? $"ENDLESS SHUFFLE · {liveStageCount} STAGES"
                : $"CLEAR ALL {liveStageCount} LIVE STAGES";
        }

        internal void SetVerticalPosition(float centerY, float halfHeight)
        {
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(
                rect.anchorMin.x,
                centerY - halfHeight);
            rect.anchorMax = new Vector2(
                rect.anchorMax.x,
                centerY + halfHeight);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal bool HasRequiredReferences()
        {
            return stateText != null && descriptionText != null;
        }
    }
}
