using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.PlayMode
{
    // Iteration 69 readability contract: every visible label sits inside the
    // safe area of an opaque plate drawn before it, touches no other label or
    // later image, is not truncated and renders at MinFontSize or larger.
    internal static class UiReadabilityAudit
    {
        internal const float MinFontSize = 14f;
        private const float OpaqueAlpha = 0.98f;
        private const float VisibleAlpha = 0.05f;
        private const float Tolerance = 1.5f;
        private const float StrokeInset = 8f;
        private const float MaxPlainPlateLuminance = 0.35f;

        // Normalized (x, y from bottom) content windows of authored frame art.
        // Panel and button sprites are opaque inside their slice borders.
        private static readonly Dictionary<string, Rect[]> AuthoredSafeAreas =
            new Dictionary<string, Rect[]>
            {
                {
                    "PageHeader",
                    new[] { Rect.MinMaxRect(0.10f, 0.36f, 0.90f, 0.68f) }
                },
                {
                    "TopResourceBar",
                    new[]
                    {
                        Rect.MinMaxRect(0.05f, 0.42f, 0.28f, 0.65f),
                        Rect.MinMaxRect(0.28f, 0.42f, 0.50f, 0.65f),
                        Rect.MinMaxRect(0.50f, 0.42f, 0.72f, 0.65f),
                        Rect.MinMaxRect(0.72f, 0.42f, 0.95f, 0.65f)
                    }
                },
                {
                    "BottomNavigationDock",
                    new[]
                    {
                        Rect.MinMaxRect(0.055f, 0.40f, 0.215f, 0.70f),
                        Rect.MinMaxRect(0.24f, 0.40f, 0.40f, 0.70f),
                        Rect.MinMaxRect(0.425f, 0.40f, 0.585f, 0.70f),
                        Rect.MinMaxRect(0.605f, 0.40f, 0.765f, 0.70f),
                        Rect.MinMaxRect(0.79f, 0.40f, 0.95f, 0.70f)
                    }
                },
                {
                    "LobbyStageCard",
                    new[]
                    {
                        Rect.MinMaxRect(0.06f, 0.43f, 0.94f, 0.85f),
                        Rect.MinMaxRect(0.27f, 0.16f, 0.73f, 0.38f)
                    }
                }
            };

        private static readonly HashSet<string> OpaqueSlicedSprites =
            new HashSet<string>
            {
                "ItemCard", "Panel", "Modal", "ResourceChip",
                "ButtonPrimary", "ButtonSecondary", "ButtonDanger"
            };

        // Iteration 70: the gate judgment flash is a translucent, additive
        // feeling effect rather than text to read, so it is exempt.
        private static readonly HashSet<string> DecorativeRoots =
            new HashSet<string> { "GateJudgmentFlash" };

        private static RectTransform _root;

        // Audits the current UI at a fixed 1080x1920 portrait frame and adds
        // every problem to the sink so one run lists all of them.
        internal static IEnumerator Check(string state, List<string> sink)
        {
            using (var frame = new UiPortraitFrame(1080, 1920))
            {
                yield return frame.Settle();
                sink.AddRange(Inspect(state));
            }
        }

        internal static void AssertNone(List<string> problems)
        {
            if (problems.Count == 0)
            {
                return;
            }
            var message = new StringBuilder();
            message.AppendLine($"{problems.Count} readability problem(s):");
            for (int index = 0; index < problems.Count; index++)
            {
                message.AppendLine(problems[index]);
            }
            NUnit.Framework.Assert.Fail(message.ToString());
        }

        internal static List<string> Inspect(string state)
        {
            Canvas.ForceUpdateCanvases();
            var problems = new List<string>();
            Canvas[] canvases =
                Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int index = 0; index < canvases.Length; index++)
            {
                if (canvases[index].isRootCanvas &&
                    canvases[index].isActiveAndEnabled)
                {
                    InspectCanvas(state, canvases[index], problems);
                }
            }
            return problems;
        }

        private static void InspectCanvas(
            string state,
            Canvas canvas,
            List<string> problems)
        {
            _root = (RectTransform)canvas.transform;
            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(false);
            var visible = new List<Graphic>(graphics.Length);
            var bounds = new List<Rect>(graphics.Length);
            for (int index = 0; index < graphics.Length; index++)
            {
                Graphic graphic = graphics[index];
                // A graphic a mask culls is not drawn and builds no
                // geometry, so it is off screen rather than unreadable.
                if (!graphic.isActiveAndEnabled ||
                    graphic.canvasRenderer.cull ||
                    EffectiveAlpha(graphic) < VisibleAlpha ||
                    IsDecorative(graphic.transform))
                {
                    continue;
                }
                Rect rect;
                if (graphic is Text text)
                {
                    if (string.IsNullOrWhiteSpace(text.text))
                    {
                        continue;
                    }
                    if (!TryGetGlyphBounds(text, out rect))
                    {
                        if (Overlaps(_root.rect, WorldRect(text.rectTransform)))
                        {
                            problems.Add(
                                $"[{state}] {PathOf(text.transform)} \"{Shorten(text.text)}\": " +
                                "renders no glyphs (rect too small)");
                        }
                        continue;
                    }
                }
                else
                {
                    rect = WorldRect(graphic.rectTransform);
                }
                if (!TryClipToMasks(graphic, ref rect, graphic is Text))
                {
                    continue;
                }
                visible.Add(graphic);
                bounds.Add(rect);
            }

            for (int index = 0; index < visible.Count; index++)
            {
                if (!(visible[index] is Text text))
                {
                    continue;
                }
                string label = $"[{state}] {PathOf(text.transform)} \"{Shorten(text.text)}\"";
                Rect glyphs = bounds[index];
                if (!Overlaps(_root.rect, glyphs) ||
                    IsBehindModalDim(visible, bounds, index, glyphs))
                {
                    continue;
                }
                int cover = FindLaterCover(visible, bounds, index, glyphs);
                if (cover >= 0)
                {
                    problems.Add(
                        $"{label}: hidden under {PathOf(visible[cover].transform)}");
                    continue;
                }

                float used = UsedFontSize(text);
                if (used < MinFontSize - 0.5f)
                {
                    problems.Add($"{label}: font {used:0.#} < {MinFontSize}");
                }
                if (IsTruncated(text))
                {
                    problems.Add($"{label}: text is truncated");
                }
                if (!Contains(WorldRect(text.rectTransform), glyphs))
                {
                    problems.Add($"{label}: glyphs overflow their rect");
                }

                int plate = FindPlate(visible, index, glyphs);
                if (plate < 0)
                {
                    problems.Add($"{label}: no opaque plate behind text");
                }

                for (int other = 0; other < visible.Count; other++)
                {
                    if (other == index || !Overlaps(glyphs, bounds[other]))
                    {
                        continue;
                    }
                    Graphic graphic = visible[other];
                    if (graphic is Text otherText)
                    {
                        if (other > index)
                        {
                            problems.Add(
                                $"{label}: overlaps text {PathOf(otherText.transform)}");
                        }
                        continue;
                    }
                    bool drawnAbove = other > index;
                    bool drawnBetween = plate >= 0 && other > plate && other < index;
                    if ((drawnAbove || drawnBetween) &&
                        !IsPlate(graphic, glyphs) &&
                        !IsNeutralBackdrop(graphic))
                    {
                        problems.Add(
                            $"{label}: {(drawnAbove ? "covered by" : "drawn over")} " +
                            PathOf(graphic.transform));
                    }
                }
            }
        }

        // Labels under a modal dim are background, not something the player
        // is asked to read.
        private static bool IsBehindModalDim(
            List<Graphic> visible,
            List<Rect> bounds,
            int textIndex,
            Rect glyphs)
        {
            for (int index = textIndex + 1; index < visible.Count; index++)
            {
                if (!(visible[index] is Text) &&
                    IsNeutralBackdrop(visible[index]) &&
                    EffectiveAlpha(visible[index]) >= 0.3f &&
                    Overlaps(bounds[index], glyphs))
                {
                    return true;
                }
            }
            return false;
        }

        // A later opaque surface over a label makes it unreadable.
        private static int FindLaterCover(
            List<Graphic> visible,
            List<Rect> bounds,
            int textIndex,
            Rect glyphs)
        {
            for (int index = textIndex + 1; index < visible.Count; index++)
            {
                // bounds are already clipped by any scroll viewport mask.
                if (!(visible[index] is Text) &&
                    Contains(bounds[index], glyphs) &&
                    IsPlate(visible[index], glyphs))
                {
                    return index;
                }
            }
            return -1;
        }

        private static int FindPlate(List<Graphic> visible, int textIndex, Rect glyphs)
        {
            for (int index = textIndex - 1; index >= 0; index--)
            {
                if (IsPlate(visible[index], glyphs))
                {
                    return index;
                }
            }
            return -1;
        }

        private static bool IsPlate(Graphic graphic, Rect glyphs)
        {
            if (!(graphic is Image image) || EffectiveAlpha(image) < OpaqueAlpha)
            {
                return false;
            }
            Rect rect = WorldRect(image.rectTransform);
            if (image.sprite == null)
            {
                Color color = image.color;
                float luminance = (0.2126f * color.r) + (0.7152f * color.g) +
                    (0.0722f * color.b);
                return !IsNeutralBackdrop(image) &&
                    luminance <= MaxPlainPlateLuminance &&
                    Contains(rect, glyphs);
            }
            string name = image.sprite.name;
            if (OpaqueSlicedSprites.Contains(name))
            {
                // Keep glyphs clear of the neon stroke: 4 reference units.
                float inset = image.type == Image.Type.Sliced
                    ? StrokeInset / ReferenceScale()
                    : 0f;
                return Contains(
                    Rect.MinMaxRect(
                        rect.xMin + inset,
                        rect.yMin + inset,
                        rect.xMax - inset,
                        rect.yMax - inset),
                    glyphs);
            }
            if (AuthoredSafeAreas.TryGetValue(name, out Rect[] areas))
            {
                for (int index = 0; index < areas.Length; index++)
                {
                    Rect area = Rect.MinMaxRect(
                        Mathf.Lerp(rect.xMin, rect.xMax, areas[index].xMin),
                        Mathf.Lerp(rect.yMin, rect.yMax, areas[index].yMin),
                        Mathf.Lerp(rect.xMin, rect.xMax, areas[index].xMax),
                        Mathf.Lerp(rect.yMin, rect.yMax, areas[index].yMax));
                    if (Contains(area, glyphs))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsDecorative(Transform transform)
        {
            for (Transform node = transform; node != null; node = node.parent)
            {
                if (DecorativeRoots.Contains(node.name))
                {
                    return true;
                }
            }
            return false;
        }

        // Full-screen dims and raycast blockers sit under everything and do
        // not compete with text.
        private static bool IsNeutralBackdrop(Graphic graphic)
        {
            if (!(graphic is Image image))
            {
                return false;
            }
            Rect rect = WorldRect(image.rectTransform);
            return rect.width >= _root.rect.width * 0.95f &&
                rect.height >= _root.rect.height * 0.6f;
        }

        private static bool TryGetGlyphBounds(Text text, out Rect rect)
        {
            IList<UIVertex> verts = text.cachedTextGenerator.verts;
            rect = default;
            if (verts == null || verts.Count < 4)
            {
                return false;
            }
            float scale = 1f / Mathf.Max(0.01f, text.pixelsPerUnit);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            int quads = verts.Count / 4;
            bool any = false;
            for (int quad = 0; quad < quads; quad++)
            {
                Vector3 a = verts[quad * 4].position;
                Vector3 c = verts[(quad * 4) + 2].position;
                if (Mathf.Approximately(a.x, c.x) || Mathf.Approximately(a.y, c.y))
                {
                    continue;
                }
                for (int corner = 0; corner < 4; corner++)
                {
                    Vector3 world = _root.InverseTransformPoint(
                        text.rectTransform.TransformPoint(
                            verts[(quad * 4) + corner].position * scale));
                    min = Vector2.Min(min, world);
                    max = Vector2.Max(max, world);
                }
                any = true;
            }
            if (!any)
            {
                return false;
            }
            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }

        private static bool TryClipToMasks(Graphic graphic, ref Rect rect, bool requireWhole)
        {
            Transform current = graphic.transform.parent;
            while (current != null)
            {
                RectMask2D mask = current.GetComponent<RectMask2D>();
                if (mask != null && mask.isActiveAndEnabled)
                {
                    Rect clip = WorldRect(mask.rectTransform);
                    if (requireWhole)
                    {
                        // Labels scrolled partly out of a viewport are
                        // judged when they scroll fully into view.
                        if (!Contains(clip, rect))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        float xMin = Mathf.Max(rect.xMin, clip.xMin);
                        float yMin = Mathf.Max(rect.yMin, clip.yMin);
                        float xMax = Mathf.Min(rect.xMax, clip.xMax);
                        float yMax = Mathf.Min(rect.yMax, clip.yMax);
                        if (xMax <= xMin || yMax <= yMin)
                        {
                            return false;
                        }
                        rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                    }
                }
                current = current.parent;
            }
            return true;
        }

        private static float UsedFontSize(Text text)
        {
            float units = text.resizeTextForBestFit
                ? text.cachedTextGenerator.fontSizeUsedForBestFit /
                    Mathf.Max(0.01f, text.pixelsPerUnit)
                : text.fontSize;
            return units * ReferenceScale();
        }

        // Sizes are judged in the Frontend's 540-wide reference space; the
        // Campaign canvas authors at 1080, so its units count half.
        private static float ReferenceScale()
        {
            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            if (scaler == null ||
                scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return 1f;
            }
            return 540f / scaler.referenceResolution.x;
        }

        private static bool IsTruncated(Text text)
        {
            if (text.verticalOverflow == VerticalWrapMode.Overflow &&
                text.horizontalOverflow == HorizontalWrapMode.Overflow)
            {
                return false;
            }
            int expected = 0;
            string value = text.supportRichText
                ? System.Text.RegularExpressions.Regex.Replace(text.text, "<[^>]+>", string.Empty)
                : text.text;
            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] != '\n' && value[index] != '\r')
                {
                    expected++;
                }
            }
            return text.cachedTextGenerator.characterCountVisible < expected;
        }

        private static float EffectiveAlpha(Graphic graphic)
        {
            float inherited = graphic.canvasRenderer != null
                ? graphic.canvasRenderer.GetInheritedAlpha()
                : 1f;
            return graphic.color.a * inherited;
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 min = _root.InverseTransformPoint(corners[0]);
            Vector3 max = _root.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool Contains(Rect outer, Rect inner)
        {
            return inner.xMin >= outer.xMin - Tolerance &&
                inner.yMin >= outer.yMin - Tolerance &&
                inner.xMax <= outer.xMax + Tolerance &&
                inner.yMax <= outer.yMax + Tolerance;
        }

        private static bool Overlaps(Rect first, Rect second)
        {
            float width = Mathf.Min(first.xMax, second.xMax) - Mathf.Max(first.xMin, second.xMin);
            float height = Mathf.Min(first.yMax, second.yMax) - Mathf.Max(first.yMin, second.yMin);
            return width > Tolerance && height > Tolerance;
        }

        private static string PathOf(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            int depth = 0;
            while (current != null && depth < 3)
            {
                path = current.name + "/" + path;
                current = current.parent;
                depth++;
            }
            return path;
        }

        private static string Shorten(string value)
        {
            string single = value.Replace("\n", " ");
            return single.Length <= 32 ? single : single.Substring(0, 32) + "…";
        }
    }
}
