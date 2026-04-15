using UnityEngine;
using UnityEngine.UI;

namespace RuneDrop.Core
{
    /// <summary>
    /// Shared UI creation helpers for consistent look across all screens.
    /// Dark fantasy theme with purple/gold accents.
    /// </summary>
    public static class UIHelper
    {
        // ── Theme Colors ───���────────────────────────────────────────
        public static readonly Color BgDark = new Color(0.02f, 0.01f, 0.05f);
        public static readonly Color BgPanel = new Color(0.08f, 0.05f, 0.12f, 0.95f);
        public static readonly Color BgButton = new Color(0.15f, 0.08f, 0.25f, 0.9f);
        public static readonly Color BgButtonHover = new Color(0.25f, 0.12f, 0.4f, 0.9f);
        public static readonly Color AccentPurple = new Color(0.7f, 0.4f, 1f);
        public static readonly Color AccentGold = new Color(1f, 0.8f, 0.3f);
        public static readonly Color AccentGreen = new Color(0.3f, 1f, 0.4f);
        public static readonly Color AccentRed = new Color(1f, 0.3f, 0.3f);
        public static readonly Color AccentCyan = new Color(0.3f, 0.9f, 1f);
        public static readonly Color TextWhite = new Color(0.95f, 0.92f, 1f);
        public static readonly Color TextDim = new Color(0.5f, 0.45f, 0.6f);
        public static readonly Color TextMuted = new Color(0.35f, 0.3f, 0.4f);
        public static readonly Color Divider = new Color(0.4f, 0.2f, 0.6f, 0.4f);

        // ── Canvas Factory ──────────────────────────────────────────

        public static Canvas CreateCanvas(Transform parent, string name, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ── Panel Factory ───────────────────────────────────────────

        public static GameObject MakePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            return go;
        }

        // ── Text Factory ────────────────────────────────────────────

        public static Text MakeText(Transform parent, string name,
            Vector2 anchor, string text, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            float width = 900, float height = 80)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(width, height);

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null)
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        // ── Button Factory (panel + text) ───────────────────────────

        public static (GameObject panel, Text text) MakeButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, string label, int fontSize,
            Color bgColor, Color textColor)
        {
            var panel = MakePanel(parent, name + "BG", anchorMin, anchorMax, bgColor);

            // Border glow
            var border = MakePanel(parent, name + "Border",
                new Vector2(anchorMin.x - 0.005f, anchorMin.y - 0.003f),
                new Vector2(anchorMax.x + 0.005f, anchorMax.y + 0.003f),
                new Color(bgColor.r + 0.2f, bgColor.g + 0.1f, bgColor.b + 0.3f, 0.3f));
            border.transform.SetSiblingIndex(panel.transform.GetSiblingIndex());

            float cx = (anchorMin.x + anchorMax.x) / 2f;
            float cy = (anchorMin.y + anchorMax.y) / 2f;
            var text = MakeText(parent, name + "Text", new Vector2(cx, cy),
                label, fontSize, textColor);

            return (panel, text);
        }

        // ── Divider Line ──���─────────────────────────────────────────

        public static void MakeDivider(Transform parent, string name, float yAnchor)
        {
            MakePanel(parent, name,
                new Vector2(0.15f, yAnchor), new Vector2(0.85f, yAnchor + 0.002f), Divider);
        }

        // ── Tap Detection ──��────────────────────────────────────────

        public static bool GetTap(out Vector2 pos)
        {
            pos = Vector2.zero;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                pos = RuneDrop.Core.ScreenSetup.FixTouchPos(Input.GetTouch(0).position);
                return true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                pos = RuneDrop.Core.ScreenSetup.FixTouchPos(Input.mousePosition);
                return true;
            }
            return false;
        }

        public static bool TapInRect(Vector2 tapPos, float xMin, float yMin, float xMax, float yMax)
        {
            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;
            return nx >= xMin && nx <= xMax && ny >= yMin && ny <= yMax;
        }
    }
}
