using UnityEngine;
using UnityEngine.UI;

namespace RuneDrop.Core
{
    /// <summary>
    /// Shared UI creation helpers for a consistent "arcane sci-fi" look.
    /// </summary>
    public static class UIHelper
    {
        // ── Core Theme Colors ───────────────────────────────────────
        public static readonly Color BgDark = new Color(0.015f, 0.018f, 0.04f);
        public static readonly Color BgPanel = new Color(0.08f, 0.12f, 0.2f, 0.95f);
        public static readonly Color BgButton = new Color(0.12f, 0.2f, 0.32f, 0.95f);
        public static readonly Color BgButtonHover = new Color(0.18f, 0.28f, 0.42f, 0.95f);

        public static readonly Color AccentPurple = new Color(0.72f, 0.5f, 1f);
        public static readonly Color AccentGold = new Color(1f, 0.85f, 0.38f);
        public static readonly Color AccentGreen = new Color(0.45f, 1f, 0.7f);
        public static readonly Color AccentRed = new Color(1f, 0.45f, 0.5f);
        public static readonly Color AccentCyan = new Color(0.4f, 0.95f, 1f);

        public static readonly Color TextWhite = new Color(0.96f, 0.98f, 1f);
        public static readonly Color TextDim = new Color(0.64f, 0.72f, 0.88f);
        public static readonly Color TextMuted = new Color(0.45f, 0.5f, 0.65f);
        public static readonly Color Divider = new Color(0.35f, 0.65f, 1f, 0.25f);

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

        public static Transform GetSafeAreaRoot(Canvas canvas)
        {
            if (canvas == null) return null;

            var existing = canvas.transform.Find("SafeAreaRoot");
            RectTransform safeRoot;
            if (existing != null)
            {
                safeRoot = existing as RectTransform;
            }
            else
            {
                var safeGO = new GameObject("SafeAreaRoot");
                safeGO.transform.SetParent(canvas.transform, false);
                safeRoot = safeGO.AddComponent<RectTransform>();
            }

            Rect safe = Screen.safeArea;
            float xMin = safe.xMin / Screen.width;
            float xMax = safe.xMax / Screen.width;
            float yMin = safe.yMin / Screen.height;
            float yMax = safe.yMax / Screen.height;
            safeRoot.anchorMin = new Vector2(xMin, yMin);
            safeRoot.anchorMax = new Vector2(xMax, yMax);
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
            safeRoot.localScale = Vector3.one;
            return safeRoot;
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

        public static GameObject MakeCard(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Color bodyColor, Color borderColor, float borderThickness = 0.004f)
        {
            var border = MakePanel(parent, name + "Border",
                new Vector2(anchorMin.x - borderThickness, anchorMin.y - borderThickness),
                new Vector2(anchorMax.x + borderThickness, anchorMax.y + borderThickness),
                borderColor);

            var body = MakePanel(parent, name, anchorMin, anchorMax, bodyColor);
            body.transform.SetParent(border.transform, false);
            var bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(borderThickness, borderThickness);
            bodyRect.anchorMax = new Vector2(1f - borderThickness, 1f - borderThickness);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            return border;
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
            outline.effectColor = new Color(0, 0, 0, 0.75f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        public static Text MakeGlowText(Transform parent, string name,
            Vector2 anchor, string text, int fontSize, Color color,
            float width = 900, float height = 80)
        {
            var txt = MakeText(parent, name, anchor, text, fontSize, color,
                TextAnchor.MiddleCenter, width, height);
            var shadow = txt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(color.r, color.g, color.b, 0.45f);
            shadow.effectDistance = new Vector2(0f, 0f);
            return txt;
        }

        // ── Button Factory (panel + text) ──────────────────────────

        public static (GameObject panel, Text text) MakeButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, string label, int fontSize,
            Color bgColor, Color textColor)
        {
            MakePanel(parent, name + "Glow",
                new Vector2(anchorMin.x - 0.01f, anchorMin.y - 0.01f),
                new Vector2(anchorMax.x + 0.01f, anchorMax.y + 0.01f),
                new Color(bgColor.r, bgColor.g, bgColor.b, 0.18f));

            var panel = MakePanel(parent, name + "BG", anchorMin, anchorMax, bgColor);

            MakePanel(parent, name + "Edge",
                new Vector2(anchorMin.x, anchorMax.y - 0.003f),
                new Vector2(anchorMax.x, anchorMax.y),
                new Color(textColor.r, textColor.g, textColor.b, 0.35f));

            float cx = (anchorMin.x + anchorMax.x) / 2f;
            float cy = (anchorMin.y + anchorMax.y) / 2f;
            var text = MakeGlowText(parent, name + "Text", new Vector2(cx, cy),
                label, fontSize, textColor);

            return (panel, text);
        }

        // ── Divider Line ────────────────────────────────────────────

        public static void MakeDivider(Transform parent, string name, float yAnchor)
        {
            MakePanel(parent, name,
                new Vector2(0.1f, yAnchor), new Vector2(0.9f, yAnchor + 0.0025f), Divider);
        }

        // ── Tap Detection ───────────────────────────────────────────

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

        public static void LightHaptic()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
