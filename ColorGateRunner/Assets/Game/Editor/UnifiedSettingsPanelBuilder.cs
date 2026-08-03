using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    internal static class UnifiedSettingsPanelBuilder
    {
        internal const string ConfigurationPath =
            "Assets/Game/Resources/ProductLinkConfiguration.asset";

        internal static SettingsPanelController Create(Transform parent)
        {
            ProductLinkConfiguration links = LoadOrCreateConfiguration();
            GameObject panel = CreatePanel(
                "SettingsPanel",
                parent,
                new Vector2(0.07f, 0.08f),
                new Vector2(0.93f, 0.92f),
                new Color(0.09f, 0.13f, 0.21f, 1f));
            SettingsPanelController controller =
                panel.AddComponent<SettingsPanelController>();
            CreateText(
                "SettingsTitle",
                panel.transform,
                "SETTINGS",
                30,
                new Vector2(0.08f, 0.90f),
                new Vector2(0.92f, 0.98f));

            Toggle notifications = CreateToggle(
                "NotificationToggle",
                panel.transform,
                "NOTIFICATIONS",
                0.75f);
            Toggle music = CreateToggle(
                "MusicToggle",
                panel.transform,
                "MUSIC",
                0.63f);
            Toggle sfx = CreateToggle(
                "SfxToggle",
                panel.transform,
                "SFX",
                0.51f);
            Toggle vibration = CreateToggle(
                "VibrationToggle",
                panel.transform,
                "VIBRATION",
                0.39f);
            Text notificationNotice = CreateText(
                "NotificationNoticeText",
                panel.transform,
                "PREFERENCE ONLY - NOTIFICATIONS ARE NOT SENT YET",
                12,
                new Vector2(0.08f, 0.31f),
                new Vector2(0.92f, 0.37f));
            notificationNotice.color = new Color(1f, 1f, 1f, 0.68f);

            Button terms = CreateButton(
                "TermsButton",
                panel.transform,
                "TERMS",
                new Vector2(0.07f, 0.21f),
                new Vector2(0.35f, 0.29f));
            Button privacy = CreateButton(
                "PrivacyButton",
                panel.transform,
                "PRIVACY",
                new Vector2(0.36f, 0.21f),
                new Vector2(0.64f, 0.29f));
            Button support = CreateButton(
                "SupportButton",
                panel.transform,
                "SUPPORT",
                new Vector2(0.65f, 0.21f),
                new Vector2(0.93f, 0.29f));
            Text linkStatus = CreateText(
                "LinkStatusText",
                panel.transform,
                "URL NOT CONFIGURED",
                12,
                new Vector2(0.08f, 0.16f),
                new Vector2(0.92f, 0.20f));
            Text status = CreateText(
                "SettingsStatusText",
                panel.transform,
                string.Empty,
                13,
                new Vector2(0.08f, 0.11f),
                new Vector2(0.92f, 0.15f));
            Button apply = CreateButton(
                "SettingsApplyButton",
                panel.transform,
                "APPLY",
                new Vector2(0.08f, 0.02f),
                new Vector2(0.48f, 0.10f));
            Button cancel = CreateButton(
                "SettingsCancelButton",
                panel.transform,
                "CANCEL",
                new Vector2(0.52f, 0.02f),
                new Vector2(0.92f, 0.10f));

            controller.Configure(
                notifications,
                music,
                sfx,
                vibration,
                notificationNotice,
                terms,
                privacy,
                support,
                linkStatus,
                status,
                apply,
                cancel,
                links);
            return controller;
        }

        private static ProductLinkConfiguration LoadOrCreateConfiguration()
        {
            ProductLinkConfiguration configuration =
                AssetDatabase.LoadAssetAtPath<ProductLinkConfiguration>(
                    ConfigurationPath);
            if (configuration != null)
            {
                return configuration;
            }

            configuration =
                ScriptableObject.CreateInstance<ProductLinkConfiguration>();
            AssetDatabase.CreateAsset(configuration, ConfigurationPath);
            AssetDatabase.SaveAssets();
            return configuration;
        }

        private static Toggle CreateToggle(
            string name,
            Transform parent,
            string label,
            float bottom)
        {
            GameObject root = CreatePanel(
                name,
                parent,
                new Vector2(0.10f, bottom),
                new Vector2(0.90f, bottom + 0.10f),
                new Color(0.15f, 0.19f, 0.28f, 1f));
            Toggle toggle = root.AddComponent<Toggle>();
            CreateText(
                "Label",
                root.transform,
                label,
                17,
                Vector2.zero,
                new Vector2(0.72f, 1f));
            CreateText(
                "OffLabel",
                root.transform,
                "OFF",
                16,
                new Vector2(0.72f, 0f),
                Vector2.one);
            Text onLabel = CreateText(
                "OnLabel",
                root.transform,
                "ON",
                16,
                new Vector2(0.72f, 0f),
                Vector2.one);
            onLabel.color = new Color(0.32f, 0.90f, 0.56f, 1f);
            toggle.targetGraphic = root.GetComponent<Image>();
            toggle.graphic = onLabel;
            toggle.isOn = true;
            return toggle;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 minimum,
            Vector2 maximum)
        {
            GameObject root = CreatePanel(
                name,
                parent,
                minimum,
                maximum,
                new Color(0.16f, 0.22f, 0.34f, 1f));
            Button button = root.AddComponent<Button>();
            CreateText(
                name + "Label",
                root.transform,
                label,
                16,
                Vector2.zero,
                Vector2.one);
            return button;
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 minimum,
            Vector2 maximum,
            Color color)
        {
            var root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            root.transform.SetParent(parent, false);
            SetAnchors(root.GetComponent<RectTransform>(), minimum, maximum);
            root.GetComponent<Image>().color = color;
            return root;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Vector2 minimum,
            Vector2 maximum)
        {
            var root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Text));
            root.transform.SetParent(parent, false);
            SetAnchors(root.GetComponent<RectTransform>(), minimum, maximum);
            Text text = root.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 minimum,
            Vector2 maximum)
        {
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
