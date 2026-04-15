using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Name input using native mobile keyboard (TouchScreenKeyboard).
    /// Clean, simple design that actually works on Android.
    /// </summary>
    public class NameInputUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _nameDisplay;
        private Text _errorText;
        private Text _tapPrompt;
        private System.Action<string> _onComplete;
        private TouchScreenKeyboard _keyboard;
        private string _currentName = "";
        private bool _keyboardOpen;
        private float _cursorBlink;

        public void Show(System.Action<string> onComplete)
        {
            _onComplete = onComplete;
            CreateUI();
            _panel.SetActive(true);
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            // Handle keyboard
            if (_keyboard != null && _keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                _currentName = _keyboard.text;
                _keyboard = null;
                _keyboardOpen = false;
            }
            else if (_keyboard != null && _keyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                _keyboard = null;
                _keyboardOpen = false;
            }
            else if (_keyboard != null && _keyboard.active)
            {
                _currentName = _keyboard.text;
            }

            // Blinking cursor effect
            _cursorBlink += Time.deltaTime;
            string cursor = Mathf.PingPong(_cursorBlink * 2f, 1f) > 0.5f ? "|" : "";

            if (string.IsNullOrEmpty(_currentName))
                _nameDisplay.text = cursor;
            else
                _nameDisplay.text = _currentName + cursor;

            // Handle editor input (for testing)
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Return) && _currentName.Length >= 2)
            {
                ConfirmName();
                return;
            }
            foreach (char c in Input.inputString)
            {
                if (c == '\b' && _currentName.Length > 0)
                    _currentName = _currentName.Substring(0, _currentName.Length - 1);
                else if (c >= ' ' && _currentName.Length < 15)
                    _currentName += c;
            }
#endif

            // Handle taps
            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;
            float ny = tapPos.y / Screen.height;

            // Tap on name area (0.45-0.58) opens keyboard
            if (ny > 0.42f && ny < 0.60f && !_keyboardOpen)
            {
                OpenKeyboard();
                return;
            }

            // Confirm button (0.28-0.38)
            if (ny > 0.25f && ny < 0.40f)
            {
                ConfirmName();
            }
        }

        private void OpenKeyboard()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _keyboard = TouchScreenKeyboard.Open(_currentName, TouchScreenKeyboardType.Default, false, false, false, false, "Enter your name", 15);
            _keyboardOpen = true;
#endif
        }

        private void ConfirmName()
        {
            string name = _currentName.Trim();
            if (name.Length < 2)
            {
                _errorText.text = "Name must be at least 2 characters!";
                _errorText.color = UIHelper.AccentRed;
                return;
            }

            PlayerPrefs.SetString("PlayerName", name);
            PlayerPrefs.Save();
            Debug.Log($"[NameInput] Player name set: {name}");

            _panel.SetActive(false);
            Destroy(_panel);
            _onComplete?.Invoke(name);
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "NameCanvas", 600);
            _panel = canvas.gameObject;
            var ct = canvas.transform;

            // ── Background with gradient effect ─────────────────────
            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one,
                new Color(0.01f, 0.005f, 0.03f));
            UIHelper.MakePanel(ct, "TopGlow",
                new Vector2(0f, 0.6f), new Vector2(1f, 1f),
                new Color(0.15f, 0.05f, 0.25f, 0.4f));
            UIHelper.MakePanel(ct, "BotGlow",
                new Vector2(0f, 0f), new Vector2(1f, 0.3f),
                new Color(0.05f, 0.15f, 0.02f, 0.3f));

            // ── Welcome text ────────────────────────────────────────
            UIHelper.MakeText(ct, "Welcome", new Vector2(0.5f, 0.85f),
                "WELCOME TO", 32, UIHelper.TextDim);
            UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.78f),
                "RUNE DROP", 72, UIHelper.AccentPurple);

            UIHelper.MakeDivider(ct, "Div1", 0.72f);

            UIHelper.MakeText(ct, "EnterLabel", new Vector2(0.5f, 0.65f),
                "What should we call you?", 30, UIHelper.TextWhite);

            // ── Name input area ─────────────────────────────────────
            // Outer glow
            UIHelper.MakePanel(ct, "InputGlow",
                new Vector2(0.08f, 0.46f), new Vector2(0.92f, 0.59f),
                new Color(0.4f, 0.2f, 0.6f, 0.2f));
            // Input background
            UIHelper.MakePanel(ct, "InputBG",
                new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.58f),
                new Color(0.08f, 0.05f, 0.14f));

            // Name display text
            _nameDisplay = UIHelper.MakeText(ct, "NameDisplay", new Vector2(0.5f, 0.525f),
                "|", 44, Color.white);

            // Tap to type hint
            _tapPrompt = UIHelper.MakeText(ct, "TapHint", new Vector2(0.5f, 0.43f),
                "TAP ABOVE TO TYPE YOUR NAME", 22, UIHelper.TextMuted);

            // Error text
            _errorText = UIHelper.MakeText(ct, "Error", new Vector2(0.5f, 0.39f),
                "", 24, UIHelper.AccentRed);

            // ── Confirm button ──────────────────────────────────────
            UIHelper.MakePanel(ct, "ConfirmGlow",
                new Vector2(0.14f, 0.27f), new Vector2(0.86f, 0.38f),
                new Color(0.1f, 0.3f, 0.1f, 0.3f));
            UIHelper.MakeButton(ct, "Confirm",
                new Vector2(0.15f, 0.28f), new Vector2(0.85f, 0.37f),
                "LET'S GO!", 44, new Color(0.08f, 0.25f, 0.08f, 0.95f), UIHelper.AccentGreen);

            // ── Prize banner at bottom ──────────────────────────────
            UIHelper.MakePanel(ct, "PrizeBG",
                new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.2f),
                new Color(0.15f, 0.1f, 0.02f, 0.9f));
            UIHelper.MakePanel(ct, "PrizeBorder",
                new Vector2(0.04f, 0.055f), new Vector2(0.96f, 0.205f),
                new Color(0.6f, 0.4f, 0.1f, 0.25f));
            UIHelper.MakeText(ct, "PrizeIcon", new Vector2(0.5f, 0.175f),
                "WEEKLY COMPETITION", 26, UIHelper.AccentGold);
            UIHelper.MakeText(ct, "PrizeAmount", new Vector2(0.5f, 0.135f),
                "$10 CAD Prize Every Week!", 32, new Color(1f, 0.85f, 0.3f));
            UIHelper.MakeText(ct, "PrizeHow", new Vector2(0.5f, 0.09f),
                "Top score wins. New round every Monday.", 22, UIHelper.TextDim);
        }
    }
}
