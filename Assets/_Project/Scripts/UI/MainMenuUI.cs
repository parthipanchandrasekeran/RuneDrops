using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// High-contrast main menu with cleaner hierarchy and dramatic visual rhythm.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _bestDepthText;
        private Text _shardsText;
        private Text _titleText;
        private Text _playText;
        private Text _playerNameText;
        private Text _streakText;
        private Text _dailyGoalText;
        private Text _weeklyLeaderText;
        private Text _weeklyCountdownText;
        private Text _weeklyPrizeText;
        private RectTransform _playBGRect;
        private Image _playGlowImage;
        private float _timer;
        private bool _bannerRefreshed;

        private SettingsScreenUI _settings;
        private LeaderboardScreenUI _leaderboard;
        private PowersReferenceUI _powers;

        private void Start()
        {
            CreateUI();
            RefreshEngagementMeta();
            // Don't find Settings/Leaderboard here — they may not exist yet
            // (created by MainMenuBootstrap AFTER MainMenuUI)
        }

        private void Update()
        {
            if (_canvas == null || !_canvas.gameObject.activeSelf) return;

            _timer += Time.deltaTime;

            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                _bestDepthText.text = $"{save.Data.BestDepth:F0}m";
                _shardsText.text = $"{save.Data.SoulShards}";
            }

            string pName = PlayerPrefs.GetString("PlayerName", "");
            if (_playerNameText != null)
                _playerNameText.text = string.IsNullOrWhiteSpace(pName) ? "Adventurer" : pName;

            float hue = 0.55f + Mathf.Sin(_timer * 0.3f) * 0.06f;
            _titleText.color = Color.HSVToRGB(hue, 0.55f, 1f);

            float s = 1f + Mathf.Sin(_timer * 2.2f) * 0.022f;
            if (_playBGRect != null) _playBGRect.localScale = new Vector3(s, s, 1f);

            if (_playGlowImage != null)
            {
                var c = _playGlowImage.color;
                c.a = 0.2f + Mathf.Sin(_timer * 1.9f) * 0.08f;
                _playGlowImage.color = c;
            }

            float t = (Mathf.Sin(_timer * 2.8f) + 1f) / 2f;
            _playText.color = Color.Lerp(UIHelper.AccentGreen, UIHelper.TextWhite, t);

            if (!_bannerRefreshed && CloudLeaderboard.Instance != null && CloudLeaderboard.Instance.IsLoaded)
            {
                RefreshWeeklyBanner();
                _bannerRefreshed = true;
            }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;
            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            if (ny > 0.30f && ny < 0.53f)
            {
                UIHelper.LightHaptic();
                if (GameManager.Instance != null) GameManager.Instance.StartRun();
                else UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            // Powers reference — center, y 0.13-0.20
            if (nx > 0.2f && nx < 0.8f && ny > 0.12f && ny < 0.22f)
            {
                if (_powers == null) _powers = FindFirstObjectByType<PowersReferenceUI>();
                if (_powers != null)
                {
                    UIHelper.LightHaptic();
                    _canvas.gameObject.SetActive(false);
                    _powers.Open(() => _canvas.gameObject.SetActive(true));
                }
                return;
            }

            // Settings button — left side, bottom strip
            if (nx < 0.47f && ny < 0.11f)
            {
                if (_settings == null) _settings = FindFirstObjectByType<SettingsScreenUI>();
                if (_settings != null)
                {
                    UIHelper.LightHaptic();
                    _canvas.gameObject.SetActive(false);
                    _settings.Open(() => _canvas.gameObject.SetActive(true));
                }
                return;
            }

            // Leaderboard button — right side, bottom strip
            if (nx > 0.53f && ny < 0.11f)
            {
                if (_leaderboard == null) _leaderboard = FindFirstObjectByType<LeaderboardScreenUI>();
                if (_leaderboard != null)
                {
                    UIHelper.LightHaptic();
                    _canvas.gameObject.SetActive(false);
                    _leaderboard.Open(() => _canvas.gameObject.SetActive(true));
                }
                return;
            }
        }

        private void RefreshWeeklyBanner()
        {
            var lb = CloudLeaderboard.Instance;
            if (lb == null) return;

            if (lb.TopThisWeek.Count > 0)
            {
                var top = lb.TopThisWeek[0];
                string[] parts = top.Date.Split('|');
                string name = parts.Length > 1 ? parts[0] : "???";
                _weeklyLeaderText.text = $"Current #1: {name} · {top.Depth:F0}m";
                _weeklyLeaderText.color = UIHelper.AccentGold;
            }
            else
            {
                _weeklyLeaderText.text = "No scores yet — claim the first crown.";
                _weeklyLeaderText.color = UIHelper.AccentGreen;
            }

            var now = System.DateTime.UtcNow;
            int days = ((int)System.DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (days == 0) days = 7;
            _weeklyCountdownText.text = $"Resets in {days} day{(days != 1 ? "s" : "")}";

            if (lb.CurrentWinner != null)
                _weeklyPrizeText.text = $"Last week winner: {lb.CurrentWinner.PlayerName} ($10 CAD)";
        }

        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas(transform, "MenuCanvas", 100);
            var ct = UIHelper.GetSafeAreaRoot(_canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);
            UIHelper.MakePanel(ct, "AuraTop", new Vector2(0f, 0.64f), new Vector2(1f, 1f), new Color(0.1f, 0.22f, 0.42f, 0.36f));
            UIHelper.MakePanel(ct, "AuraBottom", new Vector2(0f, 0f), new Vector2(1f, 0.28f), new Color(0.22f, 0.1f, 0.28f, 0.26f));

            UIHelper.MakeCard(ct, "Banner", new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.985f), new Color(0.09f, 0.12f, 0.2f, 0.95f), new Color(0.4f, 0.62f, 0.95f, 0.35f));
            _weeklyPrizeText = UIHelper.MakeGlowText(ct, "PrizeLabel", new Vector2(0.5f, 0.96f), "WEEKLY PRIZE · $10 CAD", 24, UIHelper.AccentGold);
            _weeklyLeaderText = UIHelper.MakeText(ct, "WeeklyLeader", new Vector2(0.5f, 0.925f), "Loading leaderboard...", 22, UIHelper.TextDim);
            _weeklyCountdownText = UIHelper.MakeText(ct, "Countdown", new Vector2(0.5f, 0.89f), "", 20, UIHelper.TextMuted);

            _titleText = UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.76f), "RUNE DROP", 90, UIHelper.AccentCyan);
            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.70f), "Arcane descent · Precision dodging · Rune mastery", 24, UIHelper.TextDim);
            UIHelper.MakeDivider(ct, "TitleDiv", 0.675f);

            UIHelper.MakeCard(ct, "DepthCard", new Vector2(0.06f, 0.56f), new Vector2(0.47f, 0.66f), new Color(0.07f, 0.1f, 0.18f, 0.95f), new Color(0.35f, 0.7f, 0.95f, 0.3f));
            UIHelper.MakeText(ct, "DepthLbl", new Vector2(0.265f, 0.637f), "BEST DEPTH", 20, UIHelper.TextMuted);
            _bestDepthText = UIHelper.MakeGlowText(ct, "DepthVal", new Vector2(0.265f, 0.597f), "0m", 46, UIHelper.AccentCyan);

            UIHelper.MakeCard(ct, "ShardCard", new Vector2(0.53f, 0.56f), new Vector2(0.94f, 0.66f), new Color(0.12f, 0.08f, 0.18f, 0.95f), new Color(0.62f, 0.38f, 0.95f, 0.3f));
            UIHelper.MakeText(ct, "ShardLbl", new Vector2(0.735f, 0.637f), "SOUL SHARDS", 20, UIHelper.TextMuted);
            _shardsText = UIHelper.MakeGlowText(ct, "ShardVal", new Vector2(0.735f, 0.597f), "0", 46, UIHelper.AccentPurple);

            var glowGO = UIHelper.MakePanel(ct, "PlayGlow", new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.54f), new Color(0.2f, 0.8f, 0.7f, 0.22f));
            _playGlowImage = glowGO.GetComponent<Image>();

            var playBG = UIHelper.MakePanel(ct, "PlayBG", new Vector2(0.12f, 0.35f), new Vector2(0.88f, 0.52f), new Color(0.08f, 0.28f, 0.28f, 0.95f));
            _playBGRect = playBG.GetComponent<RectTransform>();
            UIHelper.MakePanel(ct, "PlayEdge", new Vector2(0.12f, 0.515f), new Vector2(0.88f, 0.52f), new Color(0.65f, 1f, 0.95f, 0.45f));
            _playText = UIHelper.MakeGlowText(ct, "PlayText", new Vector2(0.5f, 0.435f), "BEGIN DESCENT", 54, UIHelper.TextWhite);

            _playerNameText = UIHelper.MakeText(ct, "PlayerName", new Vector2(0.5f, 0.28f), "Adventurer", 28, UIHelper.AccentCyan);
            _streakText = UIHelper.MakeText(ct, "Streak", new Vector2(0.5f, 0.255f), "", 19, UIHelper.AccentGold);
            _dailyGoalText = UIHelper.MakeText(ct, "DailyGoal", new Vector2(0.5f, 0.232f), "", 18, UIHelper.TextDim);
            UIHelper.MakeText(ct, "Controls", new Vector2(0.5f, 0.22f), "Drag: Move · Tap: Anchor · Chase the glow", 20, UIHelper.TextMuted);

            // Powers reference button (center, above bottom buttons)
            UIHelper.MakeButton(ct, "Powers", new Vector2(0.2f, 0.13f), new Vector2(0.8f, 0.20f),
                "VIEW ALL POWERS", 30, new Color(0.15f, 0.1f, 0.22f, 0.95f), UIHelper.AccentPurple);
            UIHelper.MakeText(ct, "Rule", new Vector2(0.5f, 0.178f), "Red hazards hurt · Rune colors power combos", 20, UIHelper.TextDim);

            UIHelper.MakeButton(ct, "Settings", new Vector2(0.05f, 0.035f), new Vector2(0.47f, 0.115f), "SETTINGS", 30, new Color(0.12f, 0.16f, 0.26f, 0.95f), UIHelper.AccentCyan);
            UIHelper.MakeButton(ct, "Leader", new Vector2(0.53f, 0.035f), new Vector2(0.95f, 0.115f), "LEADERBOARD", 28, new Color(0.2f, 0.14f, 0.1f, 0.95f), UIHelper.AccentGold);

            UIHelper.MakeText(ct, "Ver", new Vector2(0.5f, 0.012f), "v1.0", 16, new Color(0.35f, 0.42f, 0.52f));
            UIFXAnimator.Attach(_canvas.gameObject, 0.22f, 0.98f);
        }

        private void RefreshEngagementMeta()
        {
            var today = System.DateTime.UtcNow.Date;
            string todayStr = today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string lastStr = PlayerPrefs.GetString("LastActiveDate", "");
            int streak = PlayerPrefs.GetInt("LoginStreak", 0);

            if (lastStr != todayStr)
            {
                if (System.DateTime.TryParseExact(lastStr, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var lastDate) && (today - lastDate.Date).TotalDays == 1)
                    streak = Mathf.Clamp(streak + 1, 1, 365);
                else
                    streak = 1;

                PlayerPrefs.SetString("LastActiveDate", todayStr);
                PlayerPrefs.SetInt("LoginStreak", streak);
                PlayerPrefs.Save();
            }

            float bestDepth = 0f;
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
                bestDepth = save.Data.BestDepth;

            int goal = Mathf.Max(25, Mathf.CeilToInt((bestDepth + 1f) / 25f) * 25);
            if (_streakText != null) _streakText.text = $"Streak: {Mathf.Max(1, streak)} day{(streak == 1 ? "" : "s")}";
            if (_dailyGoalText != null) _dailyGoalText.text = $"Today's focus: reach {goal}m depth";
        }
    }
}
