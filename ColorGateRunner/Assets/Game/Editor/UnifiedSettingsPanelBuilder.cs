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
        internal const string FontCredit =
            "FONT: CHAKRA PETCH - SIL OPEN FONT LICENSE 1.1";

        private const float SectionHeight = 44f;
        private const float ToggleHeight = 104f;
        private const float InfoHeight = 40f;
        private const float LinkRowHeight = 60f;

        internal static SettingsPanelController Create(Transform parent)
        {
            ProductLinkConfiguration links = LoadOrCreateConfiguration();
            GameObject panel = CreatePanel(
                "SettingsPanel",
                parent,
                new Vector2(0.07f, 0.08f),
                new Vector2(0.93f, 0.92f),
                NeonUiPalette.Night);
            Theme01UiSkinBuilder.ApplyPanel(
                panel.GetComponent<Image>(),
                "SettingsModal");
            SettingsPanelController controller =
                panel.AddComponent<SettingsPanelController>();
            Text title = CreateText(
                "SettingsTitle",
                panel.transform,
                "SETTINGS",
                34,
                new Vector2(0.08f, 0.89f),
                new Vector2(0.92f, 0.96f));
            title.color = NeonUiPalette.TextPrimary;

            Transform content = CreateContentColumn(panel.transform);
            GameObject audioSection = CreateSectionLabel(
                "AudioSectionLabel",
                content,
                "AUDIO");
            Toggle music = CreateToggle(
                "MusicToggle",
                content,
                "MUSIC",
                out Text musicState);
            Toggle sfx = CreateToggle(
                "SfxToggle",
                content,
                "SFX",
                out Text sfxState);
            GameObject gameplaySection = CreateSectionLabel(
                "GameplaySectionLabel",
                content,
                "GAMEPLAY");
            Toggle vibration = CreateToggle(
                "VibrationToggle",
                content,
                "VIBRATION",
                out Text vibrationState);
            Toggle notifications = CreateToggle(
                "NotificationToggle",
                content,
                "NOTIFICATIONS",
                out Text notificationState);
            Text notificationNotice = CreateRowText(
                "NotificationNoticeText",
                content,
                "PREFERENCE ONLY - NOTIFICATIONS ARE NOT SENT YET",
                13,
                InfoHeight,
                NeonUiPalette.TextSecondary);

            CreateSectionLabel("AboutSectionLabel", content, "ABOUT");
            Text version = CreateRowText(
                "SettingsVersionText",
                content,
                "VERSION",
                16,
                InfoHeight,
                NeonUiPalette.TextPrimary);
            CreateRowText(
                "SettingsFontCreditText",
                content,
                FontCredit,
                13,
                InfoHeight,
                NeonUiPalette.TextSecondary);
            GameObject linkRow = CreateLinkRow(
                content,
                out Button terms,
                out Button privacy,
                out Button support);
            Text linkStatus = CreateRowText(
                "LinkStatusText",
                content,
                "URL NOT CONFIGURED",
                13,
                InfoHeight,
                NeonUiPalette.TextSecondary);

            Text status = CreateText(
                "SettingsStatusText",
                panel.transform,
                string.Empty,
                15,
                new Vector2(0.08f, 0.14f),
                new Vector2(0.92f, 0.19f));
            status.color = NeonUiPalette.Signal;
            Button apply = CreateButton(
                "SettingsApplyButton",
                "ConfirmApply",
                panel.transform,
                "APPLY",
                new Vector2(0.08f, 0.035f),
                new Vector2(0.48f, 0.125f));
            Button cancel = CreateButton(
                "SettingsCancelButton",
                "CancelSecondary",
                panel.transform,
                "CANCEL",
                new Vector2(0.52f, 0.035f),
                new Vector2(0.92f, 0.125f));

            controller.Configure(
                notifications,
                music,
                sfx,
                vibration,
                notificationState,
                musicState,
                sfxState,
                vibrationState,
                notificationNotice,
                terms,
                privacy,
                support,
                linkStatus,
                status,
                apply,
                cancel,
                links);
            controller.ConfigureSections(
                audioSection,
                gameplaySection,
                linkRow,
                version);
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

        private static Transform CreateContentColumn(Transform parent)
        {
            var root = new GameObject(
                "SettingsContent",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup));
            root.transform.SetParent(parent, false);
            SetAnchors(
                root.GetComponent<RectTransform>(),
                new Vector2(0.07f, 0.2f),
                new Vector2(0.93f, 0.87f));
            VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return root.transform;
        }

        private static GameObject CreateSectionLabel(
            string name,
            Transform parent,
            string value)
        {
            Text label = CreateRowText(
                name,
                parent,
                value,
                17,
                SectionHeight,
                NeonUiPalette.TextSecondary);
            label.alignment = TextAnchor.LowerLeft;
            return label.gameObject;
        }

        private static Text CreateRowText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            float height,
            Color color)
        {
            Text text = CreateText(
                name,
                parent,
                value,
                fontSize,
                Vector2.zero,
                Vector2.one);
            text.color = color;
            AddHeight(text.gameObject, height);
            return text;
        }

        private static Toggle CreateToggle(
            string name,
            Transform parent,
            string label,
            out Text stateLabel)
        {
            GameObject root = CreatePanel(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                Color.white);
            Image card = root.GetComponent<Image>();
            Theme01UiSkinBuilder.ApplyPanel(card, "SettingsToggleCard");
            AddHeight(root, ToggleHeight);
            Toggle toggle = root.AddComponent<Toggle>();
            Text labelText = CreateText(
                "Label",
                root.transform,
                label,
                20,
                new Vector2(0.07f, 0f),
                new Vector2(0.6f, 1f));
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = NeonUiPalette.TextPrimary;
            stateLabel = CreateText(
                "StateLabel",
                root.transform,
                "ON",
                16,
                new Vector2(0.58f, 0f),
                new Vector2(0.72f, 1f));
            stateLabel.alignment = TextAnchor.MiddleRight;
            stateLabel.color = NeonUiPalette.Signal;

            Image track = CreatePanel(
                "SwitchTrack",
                root.transform,
                new Vector2(0.75f, 0.27f),
                new Vector2(0.93f, 0.73f),
                NeonUiPalette.Magenta).GetComponent<Image>();
            track.raycastTarget = false;
            Image knob = CreatePanel(
                "SwitchKnob",
                track.transform,
                new Vector2(0.54f, 0.14f),
                new Vector2(0.94f, 0.86f),
                NeonUiPalette.TextPrimary).GetComponent<Image>();
            knob.raycastTarget = false;
            root.AddComponent<SettingsSwitchView>().Configure(track, knob);

            toggle.targetGraphic = card;
            toggle.graphic = null;
            toggle.isOn = true;
            return toggle;
        }

        private static GameObject CreateLinkRow(
            Transform parent,
            out Button terms,
            out Button privacy,
            out Button support)
        {
            var root = new GameObject(
                "SettingsLinkRow",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            root.transform.SetParent(parent, false);
            AddHeight(root, LinkRowHeight);
            HorizontalLayoutGroup layout =
                root.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            terms = CreateButton(
                "TermsButton",
                "TermsLink",
                root.transform,
                "TERMS",
                Vector2.zero,
                Vector2.one);
            privacy = CreateButton(
                "PrivacyButton",
                "PrivacyLink",
                root.transform,
                "PRIVACY",
                Vector2.zero,
                Vector2.one);
            support = CreateButton(
                "SupportButton",
                "SupportLink",
                root.transform,
                "SUPPORT",
                Vector2.zero,
                Vector2.one);
            return root;
        }

        private static Button CreateButton(
            string name,
            string skinName,
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
                Color.white);
            Button button = root.AddComponent<Button>();
            Text text = CreateText(
                name + "Label",
                root.transform,
                label,
                17,
                Vector2.zero,
                Vector2.one);
            text.color = NeonUiPalette.TextPrimary;
            // Skin names avoid the "Settings" icon route so these stay text
            // buttons inside the chamfered stroke.
            Theme01UiSkinBuilder.ApplyButton(button, skinName, text);
            return button;
        }

        private static void AddHeight(GameObject target, float height)
        {
            LayoutElement element = target.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            // Rows never absorb spare column height; hidden rows collapse.
            element.flexibleHeight = 0f;
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
            Theme01UiSkinBuilder.ApplyFont(text);
            text.text = value;
            text.fontSize = fontSize;
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
