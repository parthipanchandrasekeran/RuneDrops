using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Main menu with animated title, weekly banner, stats cards, and clear navigation.
    /// Dark fantasy theme with purple/gold accents.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _bestDepthText;
        private Text _shardsText;
        private Text _titleText;
        private Text _playText;
        private Text _playerNameText;
        private Text _weeklyLeaderText;
        private Text _weeklyCountdownText;
        private Text _weeklyPrizeText;
        private RectTransform _playBGRect;
        private Image _playBGImage;
        private Image _playGlowImage;
        private float _timer;
        private bool _bannerRefreshed;

        private SettingsScreenUI _settings;
        private LeaderboardScreenUI _leaderboard;

        private void Start()
        {
            CreateUI();
            _settings = FindFirstObjectByType<SettingsScreenUI>();
            _leaderboard = FindFirstObjectByType<LeaderboardScreenUI>();
        }

        private void Update()
        {
            if (_canvas == null || !_canvas.gameObject.activeSelf) return;

            _timer += Time.deltaTime;

            // ── Update stats ────────────────────────────────────────
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                _bestDepthText.text = $"{save.Data.BestDepth:F0}m";
                _shardsText.text = $"{save.Data.SoulShards}";
            }

            string pName = PlayerPrefs.GetString("PlayerName", "");
            if (_playerNameText != null && !string.IsNullOrEmpty(pName))
                _playerNameText.text = pName;

            // ── Animations ──────────────────────────────────────────
            // Title shimmer
            float hue = 0.75f + Mathf.Sin(_timer * 0.5f) * 0.05f;
            _titleText.color = Color.HSVToRGB(hue, 0.45f, 1f);

            // Play button pulse
            float s = 1f + Mathf.Sin(_timer * 2.5f) * 0.03f;
            if (_playBGRect != null) _playBGRect.localScale = new Vector3(s, s, 1f);

            // Play glow breathe
            if (_playGlowImage != null)
            {
                var c = _playGlowImage.color;
                c.a = 0.15f + Mathf.Sin(_timer * 1.5f) * 0.1f;
                _playGlowImage.color = c;
            }

            // Play text color
            float t = (Mathf.Sin(_timer * 3f) + 1f) / 2f;
            _playText.color = Color.Lerp(new Color(0.8f, 1f, 0.8f), Color.white, t);

            // ── Refresh weekly banner ───────────────────────────────
            if (!_bannerRefreshed && CloudLeaderboard.Instance != null && CloudLeaderboard.Instance.IsLoaded)
            {
                RefreshWeeklyBanner();
                _bannerRefreshed = true;
            }

            // ── Handle taps ─────────────────────────────────────────
            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;
            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // PLAY button (0.33-0.50)
            if (ny > 0.30f && ny < 0.52f)
            {
                if (GameManager.Instance != null) GameManager.Instance.StartRun();
                else UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            // Settings (bottom-left)
            if (nx < 0.45f && ny < 0.13f && _settings != null)
            {
                _canvas.gameObject.SetActive(false);
                _settings.Open(() => _canvas.gameObject.SetActive(true));
                return;
            }

            // Leaderboard (bottom-right)
            if (nx > 0.55f && ny < 0.13f && _leaderboard != null)
            {
                _canvas.gameObject.SetActive(false);
                _leaderboard.Open(() => _canvas.gameObject.SetActive(true));
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
                _weeklyLeaderText.text = $"Current #1: {name} — {top.Depth:F0}m";
                _weeklyLeaderText.color = UIHelper.AccentGold;
            }
            else
            {
                _weeklyLeaderText.text = "No scores yet — be the first!";
                _weeklyLeaderText.color = UIHelper.AccentGreen;
            }

            var now = System.DateTime.UtcNow;
            int days = ((int)System.DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (days == 0) days = 7;
            _weeklyCountdownText.text = $"Resets in {days} day{(days != 1 ? "s" : "")}";

            if (lb.CurrentWinner != null)
                _weeklyPrizeText.text = $"Last week: {lb.CurrentWinner.PlayerName} won $10 CAD!";
        }

        // ── Create UI ───────────────────────────────────────────────

        private void CreateUI()
        {
            _canvas = UIHelper.CreateCanvas(transform, "MenuCanvas", 100);
            var ct = _canvas.transform;

            // ══════════════════════════════════════════════════════════
            // BACKGROUND LAYERS (creates depth)
            // ══════════════════════════════════════════════════════════
            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one,
                new Color(0.01f, 0.005f, 0.03f));

            // Top purple glow
            UIHelper.MakePanel(ct, "TopGlow",
                new Vector2(0f, 0.7f), new Vector2(1f, 1f),
                new Color(0.12f, 0.04f, 0.2f, 0.5f));

            // Bottom subtle glow
            UIHelper.MakePanel(ct, "BotGlow",
                new Vector2(0f, 0f), new Vector2(1f, 0.25f),
                new Color(0.03f, 0.02f, 0.08f, 0.8f));

            // ══════════════════════════════════════════════════════════
            // WEEKLY PRIZE BANNER (top)
            // ══════════════════════════════════════════════════════════
            UIHelper.MakePanel(ct, "BannerBG",
                new Vector2(0f, 0.87f), new Vector2(1f, 1f),
                new Color(0.12f, 0.08f, 0.02f, 0.95f));
            // Gold line
            UIHelper.MakePanel(ct, "BannerLine",
                new Vector2(0f, 0.868f), new Vector2(1f, 0.872f),
                new Color(0.8f, 0.6f, 0.15f, 0.6f));

            _weeklyPrizeText = UIHelper.MakeText(ct, "PrizeLabel", new Vector2(0.5f, 0.96f),
                "WEEKLY PRIZE: $10 CAD", 26, UIHelper.AccentGold);
            _weeklyLeaderText = UIHelper.MakeText(ct, "WeeklyLeader", new Vector2(0.5f, 0.925f),
                "Loading leaderboard...", 24, UIHelper.TextDim);
            _weeklyCountdownText = UIHelper.MakeText(ct, "Countdown", new Vector2(0.5f, 0.89f),
                "", 20, UIHelper.TextMuted);

            // ══════════════════════════════════════════════════════════
            // TITLE
            // ══════════════════════════════════════════════════════════
            _titleText = UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.78f),
                "RUNE DROP", 84, UIHelper.AccentPurple);

            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.71f),
                "Descend  .  Collect  .  Survive", 24, UIHelper.TextDim);

            // Accent line under title
            UIHelper.MakePanel(ct, "TitleLine",
                new Vector2(0.2f, 0.685f), new Vector2(0.8f, 0.688f),
                new Color(0.5f, 0.25f, 0.7f, 0.5f));

            // ══════════════════════════════════════════════════════════
            // STATS CARDS
            // ══════════════════════════════════════════════════════════
            // Best depth card
            UIHelper.MakePanel(ct, "DepthCardBorder",
                new Vector2(0.06f, 0.565f), new Vector2(0.47f, 0.67f),
                new Color(0.2f, 0.4f, 0.5f, 0.2f));
            UIHelper.MakePanel(ct, "DepthCard",
                new Vector2(0.065f, 0.57f), new Vector2(0.465f, 0.665f),
                new Color(0.04f, 0.06f, 0.1f, 0.9f));
            UIHelper.MakeText(ct, "DepthLbl", new Vector2(0.265f, 0.645f),
                "BEST DEPTH", 20, UIHelper.TextMuted);
            _bestDepthText = UIHelper.MakeText(ct, "DepthVal", new Vector2(0.265f, 0.6f),
                "0m", 44, UIHelper.AccentCyan);

            // Soul shards card
            UIHelper.MakePanel(ct, "ShardCardBorder",
                new Vector2(0.53f, 0.565f), new Vector2(0.94f, 0.67f),
                new Color(0.4f, 0.2f, 0.5f, 0.2f));
            UIHelper.MakePanel(ct, "ShardCard",
                new Vector2(0.535f, 0.57f), new Vector2(0.935f, 0.665f),
                new Color(0.06f, 0.03f, 0.1f, 0.9f));
            UIHelper.MakeText(ct, "ShardLbl", new Vector2(0.735f, 0.645f),
                "SOUL SHARDS", 20, UIHelper.TextMuted);
            _shardsText = UIHelper.MakeText(ct, "ShardVal", new Vector2(0.735f, 0.6f),
                "0", 44, UIHelper.AccentPurple);

            // ══════════════════════════════════════════════════════════
            // PLAY BUTTON (big, centered, glowing)
            // ══════════════════════════════════════════════════════════
            // Outer glow
            var glowGO = UIHelper.MakePanel(ct, "PlayGlow",
                new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.53f),
                new Color(0.05f, 0.2f, 0.05f, 0.25f));
            _playGlowImage = glowGO.GetComponent<Image>();

            // Button background
            var playBG = UIHelper.MakePanel(ct, "PlayBG",
                new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.51f),
                new Color(0.06f, 0.22f, 0.06f, 0.95f));
            _playBGRect = playBG.GetComponent<RectTransform>();
            _playBGImage = playBG.GetComponent<Image>();

            // Top edge highlight
            UIHelper.MakePanel(ct, "PlayEdge",
                new Vector2(0.1f, 0.507f), new Vector2(0.9f, 0.513f),
                new Color(0.3f, 0.9f, 0.3f, 0.3f));

            _playText = UIHelper.MakeText(ct, "PlayText", new Vector2(0.5f, 0.435f),
                "P L A Y", 62, Color.white);

            // ══════════════════════════════════════════════════════════
            // PLAYER INFO + HINTS
            // ══════════════════════════════════════════════════════════
            _playerNameText = UIHelper.MakeText(ct, "PlayerName", new Vector2(0.5f, 0.30f),
                "", 28, UIHelper.AccentCyan);

            UIHelper.MakeText(ct, "Controls", new Vector2(0.5f, 0.255f),
                "Drag = Move   |   Tap = Anchor   |   Glow = Collect", 20, UIHelper.TextMuted);

            UIHelper.MakePanel(ct, "RuleLine",
                new Vector2(0.15f, 0.235f), new Vector2(0.85f, 0.237f),
                UIHelper.Divider);

            UIHelper.MakeText(ct, "Rule", new Vector2(0.5f, 0.215f),
                "RED = Danger      GLOW = Collect", 22, UIHelper.TextDim);

            // ══════════════════════════════════════════════════════════
            // BOTTOM BUTTONS
            // ══════════════════════════════════════════════════════════
            UIHelper.MakeButton(ct, "Settings",
                new Vector2(0.05f, 0.04f), new Vector2(0.47f, 0.12f),
                "SETTINGS", 30, new Color(0.06f, 0.04f, 0.1f, 0.9f), UIHelper.TextDim);

            UIHelper.MakeButton(ct, "Leader",
                new Vector2(0.53f, 0.04f), new Vector2(0.95f, 0.12f),
                "LEADERBOARD", 28, new Color(0.06f, 0.04f, 0.1f, 0.9f), UIHelper.AccentGold);

            // Version
            UIHelper.MakeText(ct, "Ver", new Vector2(0.5f, 0.015f),
                "v1.0", 16, new Color(0.2f, 0.15f, 0.25f));
        }
    }
}
