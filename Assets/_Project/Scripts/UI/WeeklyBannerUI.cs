using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Weekly competition banner shown on main menu.
    /// Shows current week's top player and prize info.
    /// </summary>
    public class WeeklyBannerUI : MonoBehaviour
    {
        private Text _leaderText;
        private Text _prizeText;
        private Text _countdownText;
        private GameObject _panel;

        public void Initialize(Transform canvasTransform)
        {
            // Banner panel
            _panel = UIHelper.MakePanel(canvasTransform, "WeeklyBanner",
                new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.96f),
                new Color(0.15f, 0.1f, 0.02f, 0.95f));

            // Gold border
            UIHelper.MakePanel(canvasTransform, "BannerBorder",
                new Vector2(0.025f, 0.855f), new Vector2(0.975f, 0.965f),
                new Color(0.8f, 0.6f, 0.1f, 0.3f));

            // Prize label
            _prizeText = UIHelper.MakeText(canvasTransform, "PrizeLabel",
                new Vector2(0.5f, 0.945f),
                "WEEKLY PRIZE: $10 CAD", 24, UIHelper.AccentGold);

            // Current leader
            _leaderText = UIHelper.MakeText(canvasTransform, "WeeklyLeader",
                new Vector2(0.5f, 0.91f),
                "Loading...", 28, UIHelper.TextWhite);

            // Countdown
            _countdownText = UIHelper.MakeText(canvasTransform, "Countdown",
                new Vector2(0.5f, 0.875f),
                "", 20, UIHelper.TextDim);
        }

        public void Refresh()
        {
            var lb = CloudLeaderboard.Instance;
            if (lb == null || !lb.IsLoaded) return;

            // Show this week's #1
            if (lb.TopThisWeek.Count > 0)
            {
                var top = lb.TopThisWeek[0];
                string[] parts = top.Date.Split('|');
                string name = parts.Length > 1 ? parts[0] : "Unknown";
                _leaderText.text = $"Leader: {name} — {top.Depth:F0}m";
                _leaderText.color = UIHelper.AccentGold;
            }
            else
            {
                _leaderText.text = "No scores this week — be the first!";
                _leaderText.color = UIHelper.AccentGreen;
            }

            // Days until week ends (Monday reset)
            var now = System.DateTime.UtcNow;
            int daysUntilMonday = ((int)System.DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            _countdownText.text = $"Resets in {daysUntilMonday} day{(daysUntilMonday != 1 ? "s" : "")}";

            // Show last week's winner if exists
            if (lb.CurrentWinner != null)
            {
                _prizeText.text = $"Last Winner: {lb.CurrentWinner.PlayerName} ({lb.CurrentWinner.BestDepth:F0}m) — $10 CAD!";
            }
        }
    }
}
