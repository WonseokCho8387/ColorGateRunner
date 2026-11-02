using ColorGateRunner.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    internal static class PlaytestUiBuilder
    {
        internal static PlaytestEntryView CreateEntry(Transform parent)
        {
            RectTransform root = Rect("PlaytestEntry", parent, Vector2.zero, Vector2.one);
            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.035f, 0.05f, 0.10f, 1f);
            Text title = Label("Title", root, 30, new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.93f));
            Text description = Label("Description", root, 22, new Vector2(0.07f, 0.23f), new Vector2(0.93f, 0.79f));
            RectTransform action = Rect("Play", root, new Vector2(0.16f, 0.09f), new Vector2(0.84f, 0.18f));
            Image surface = action.gameObject.AddComponent<Image>();
            surface.color = new Color(0.05f, 0.32f, 0.40f, 1f);
            Button button = action.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            Text label = Label("Label", action, 26, Vector2.zero, Vector2.one);
            PlaytestEntryView view = root.gameObject.AddComponent<PlaytestEntryView>();
            view.Configure(title, description, button, label);
            root.gameObject.SetActive(false);
            return view;
        }

        internal static Text CreateGuide(Transform parent)
        {
            // Campaign Canvas uses a 1080 x 1920 reference, twice the Frontend.
            Text guide = Label("PlaytestGuide", parent, 40,
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.24f));
            Outline outline = guide.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);
            guide.gameObject.SetActive(false);
            return guide;
        }

        private static Text Label(string name, Transform parent, int size, Vector2 min, Vector2 max)
        {
            Text text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
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
