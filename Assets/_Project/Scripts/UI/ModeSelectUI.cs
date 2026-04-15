using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Game mode selection screen. Shows all 5 modes with descriptions.
    /// Tap a mode to start that run.
    /// </summary>
    public class ModeSelectUI : MonoBehaviour
    {
        private GameObject _panel;
        private System.Action _onClose;
        private bool _isOpen;

        private static readonly GameMode[] MODES = {
            GameMode.Classic, GameMode.Sprint, GameMode.RuneRush,
            GameMode.NoAnchor, GameMode.DailyChallenge
        };

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
        }

        public void Open(System.Action onClose = null)
        {
            _onClose = onClose;
            _isOpen = true;
            _panel.SetActive(true);

            // Check daily challenge availability
            CheckDailyAvailability();
        }

        private void Update()
        {
            if (!_isOpen) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // Back button (bottom)
            if (ny < 0.10f)
            {
                UIHelper.LightHaptic();
                _isOpen = false;
                _panel.SetActive(false);
                _onClose?.Invoke();
                return;
            }

            // Mode rows
            for (int i = 0; i < MODES.Length; i++)
            {
                float rowY = 0.73f - (i * 0.12f);
                if (ny > rowY - 0.05f && ny < rowY + 0.05f)
                {
                    var mode = MODES[i];

                    // Check daily challenge limit
                    if (mode == GameMode.DailyChallenge && HasPlayedDailyToday())
                    {
                        return; // Already played today
                    }

                    UIHelper.LightHaptic();
                    _isOpen = false;
                    _panel.SetActive(false);

                    if (mode == GameMode.DailyChallenge)
                    {
                        PlayerPrefs.SetString("LastDailyChallenge",
                            System.DateTime.UtcNow.ToString("yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture));
                        PlayerPrefs.Save();
                    }

                    if (GameManager.Instance != null)
                        GameManager.Instance.StartRunWithMode(mode);
                    return;
                }
            }
        }

        private bool HasPlayedDailyToday()
        {
            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture);
            return PlayerPrefs.GetString("LastDailyChallenge", "") == today;
        }

        private void CheckDailyAvailability()
        {
            // Visual indicator would go here
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "ModeSelectCanvas", 350);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.92f),
                "SELECT MODE", 52, UIHelper.AccentCyan);
            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.87f),
                "Each mode has its own leaderboard", 22, UIHelper.TextDim);
            UIHelper.MakeDivider(ct, "Div", 0.84f);

            for (int i = 0; i < MODES.Length; i++)
            {
                var mode = MODES[i];
                float y = 0.73f - (i * 0.12f);
                Color modeColor = GameModeConfig.GetColor(mode);

                UIHelper.MakeCard(ct, $"Mode_{i}",
                    new Vector2(0.05f, y - 0.045f), new Vector2(0.95f, y + 0.045f),
                    new Color(modeColor.r * 0.15f, modeColor.g * 0.15f, modeColor.b * 0.15f, 0.95f),
                    new Color(modeColor.r, modeColor.g, modeColor.b, 0.3f));

                UIHelper.MakeText(ct, $"ModeName_{i}", new Vector2(0.3f, y + 0.012f),
                    GameModeConfig.GetName(mode), 32, modeColor,
                    TextAnchor.MiddleLeft, 400, 40);

                UIHelper.MakeText(ct, $"ModeDesc_{i}", new Vector2(0.3f, y - 0.015f),
                    GameModeConfig.GetDescription(mode), 18, UIHelper.TextDim,
                    TextAnchor.MiddleLeft, 500, 30);
            }

            UIHelper.MakeButton(ct, "Back", new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.10f),
                "BACK", 36, new Color(0.11f, 0.18f, 0.28f, 0.96f), UIHelper.AccentCyan);

            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }
    }
}
