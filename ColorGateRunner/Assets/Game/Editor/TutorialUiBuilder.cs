using ColorGateRunner.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    // Builds the Stage 1 guide line and the modal rule card. The Campaign
    // Canvas uses a 1080 x 1920 reference (scale 2), the Frontend 540 x 960.
    internal static class TutorialUiBuilder
    {
        internal static Text CreateGuide(Transform parent)
        {
            Text guide = Label("TutorialGuide", parent, 36,
                new Vector2(0.04f, 0.15f), new Vector2(0.96f, 0.25f));
            guide.color = NeonUiPalette.TextPrimary;
            Outline outline = guide.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);
            guide.gameObject.SetActive(false);
            return guide;
        }

        internal static TutorialCardView CreateCard(Transform parent, int scale)
        {
            RectTransform root = Rect("TutorialCard", parent, Vector2.zero, Vector2.one);
            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.82f);
            dim.raycastTarget = true;

            RectTransform panel = Rect("TutorialCardPanel", root,
                new Vector2(0.07f, 0.24f), new Vector2(0.93f, 0.76f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = NeonUiPalette.Night;
            Theme01UiSkinBuilder.ApplyPanel(panelImage, "SettingsModal");

            Text title = Label("TutorialCardTitle", panel, 30 * scale,
                new Vector2(0.08f, 0.83f), new Vector2(0.92f, 0.95f));
            title.color = NeonUiPalette.Signal;
            Text body = Label("TutorialCardBody", panel, 17 * scale,
                new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.81f));
            body.color = NeonUiPalette.TextPrimary;
            body.lineSpacing = 1.15f;
            Text page = Label("TutorialCardPage", panel, 14 * scale,
                new Vector2(0.08f, 0.19f), new Vector2(0.92f, 0.25f));
            page.color = NeonUiPalette.TextSecondary;

            RectTransform action = Rect("TutorialCardButton", panel,
                new Vector2(0.22f, 0.05f), new Vector2(0.78f, 0.18f));
            action.gameObject.AddComponent<Image>();
            Button button = action.gameObject.AddComponent<Button>();
            Text label = Label("TutorialCardButtonLabel", action, 20 * scale,
                Vector2.zero, Vector2.one);
            label.color = NeonUiPalette.TextPrimary;
            Theme01UiSkinBuilder.ApplyButton(button, "ConfirmApply", label);

            TutorialCardView view = root.gameObject.AddComponent<TutorialCardView>();
            view.Configure(title, body, page, button, label);
            root.gameObject.SetActive(false);
            return view;
        }

        private static Text Label(string name, Transform parent, int size, Vector2 min, Vector2 max)
        {
            Text text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            Theme01UiSkinBuilder.ApplyFont(text);
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, size / 2);
            text.resizeTextMaxSize = size;
            return text;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
