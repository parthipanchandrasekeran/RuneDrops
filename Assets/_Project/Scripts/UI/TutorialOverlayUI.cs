using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// First-run 3-step tutorial overlay.
    /// Step 1: Drag to move
    /// Step 2: Tap to anchor
    /// Step 3: Collect runes & combos
    /// Tap to advance each step. Pauses game during tutorial.
    /// </summary>
    public class TutorialOverlayUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _stepText;
        private Text _descText;
        private Text _promptText;
        private Text _stepCounterText;
        private int _currentStep;
        private const int TOTAL_STEPS = 6;

        private readonly string[] _titles = {
            "DRAG TO MOVE",
            "TAP TO ANCHOR",
            "RUNE POWERS",
            "RUNE COMBOS",
            "DECISION ROOMS",
            "WEEKLY PRIZE"
        };

        private readonly string[] _descriptions = {
            "Drag your finger left and right\nto dodge RED obstacles.\n\nRED = Danger! Avoid them.",

            "Quick tap (don't drag) to slow\nyour fall for 1.5 seconds.\n\nYou have 2 charges per run.\nUse wisely!",

            "Collect glowing diamonds for powers:\n\n"
            + "FIRE (orange) = Destroys nearby obstacles\n"
            + "WIND (cyan) = Slows your fall\n"
            + "SHADOW (purple) = Pass through obstacles\n"
            + "EARTH (green) = Shield absorbs 1 hit",

            "Collect 2 matching runes for COMBOS:\n\n"
            + "FIRE + WIND = Flame Trail\n   Burns obstacles + slows fall\n\n"
            + "SHADOW + WIND = Blink Dash\n   Teleport downward safely\n\n"
            + "EARTH + FIRE = Explosive Shield\n   Shield that explodes on hit!",

            "Every 12 seconds a choice appears:\n\n"
            + "GREEN (Safe) = Free power-up\n"
            + "RED (Risky) = Big reward but dangerous!\n\n"
            + "Tap left or right to choose.",

            "Top scorer each week wins $10 CAD!\n\n"
            + "Your depth is uploaded to the\nglobal leaderboard automatically.\n\n"
            + "New round starts every Monday.\nGood luck!"
        };

        public void Initialize()
        {
            CreateUI();
            _currentStep = 0;
            ShowStep(0);
            Time.timeScale = 0f;
        }

        private void Update()
        {
            bool tapped = false;
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) tapped = true;
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) tapped = true;
#endif
            if (!tapped) return;

            _currentStep++;
            if (_currentStep >= TOTAL_STEPS)
            {
                CompleteTutorial();
            }
            else
            {
                ShowStep(_currentStep);
            }
        }

        private void ShowStep(int step)
        {
            _stepCounterText.text = $"Step {step + 1} of {TOTAL_STEPS}";
            _stepText.text = _titles[step];
            _descText.text = _descriptions[step];
            _promptText.text = step < TOTAL_STEPS - 1 ? "TAP TO CONTINUE >>" : "TAP TO START PLAYING!";
        }

        private void CompleteTutorial()
        {
            Time.timeScale = 1f;

            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                save.Data.HasCompletedTutorial = true;
                save.Save();
            }

            EventBus.Publish(new TutorialCompletedEvent());
            Destroy(_panel);
            Destroy(gameObject);
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("TutorialCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();
            _panel = canvasGO;

            // Dark overlay
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasGO.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            // Step counter
            _stepCounterText = MakeText(canvasGO.transform, "StepCounter", new Vector2(0.5f, 0.92f),
                "", 22, new Color(0.5f, 0.5f, 0.6f));

            // Step title
            _stepText = MakeText(canvasGO.transform, "StepTitle", new Vector2(0.5f, 0.84f),
                "", 48, new Color(0.3f, 0.8f, 1f));

            UIHelper.MakeDivider(canvasGO.transform, "TutDiv", 0.79f);

            // Description (bigger area for more text)
            _descText = MakeText(canvasGO.transform, "Desc", new Vector2(0.5f, 0.52f),
                "", 26, Color.white);
            _descText.GetComponent<RectTransform>().sizeDelta = new Vector2(850, 500);

            // Tap prompt
            _promptText = MakeText(canvasGO.transform, "Prompt", new Vector2(0.5f, 0.2f),
                "TAP TO CONTINUE", 36, new Color(0.6f, 0.6f, 0.7f));
        }

        private Text MakeText(Transform parent, string name, Vector2 anchor, string text, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(800, 100);
            var txt = go.AddComponent<Text>();
            txt.text = text; txt.fontSize = size;
            txt.alignment = TextAnchor.MiddleCenter; txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("sans-serif", size);
            go.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            return txt;
        }
    }
}
