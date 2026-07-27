using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplyIfChanged();
        }

        private void Update()
        {
            ApplyIfChanged();
        }

        internal static void CalculateAnchors(
            Rect safeArea,
            int screenWidth,
            int screenHeight,
            out Vector2 anchorMin,
            out Vector2 anchorMax)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(
                safeArea.xMin / screenWidth,
                safeArea.yMin / screenHeight);
            anchorMax = new Vector2(
                safeArea.xMax / screenWidth,
                safeArea.yMax / screenHeight);
        }

        private void ApplyIfChanged()
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _lastSafeArea && screenSize == _lastScreenSize)
            {
                return;
            }

            CalculateAnchors(
                safeArea,
                screenSize.x,
                screenSize.y,
                out Vector2 anchorMin,
                out Vector2 anchorMax);
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
        }
    }
}
