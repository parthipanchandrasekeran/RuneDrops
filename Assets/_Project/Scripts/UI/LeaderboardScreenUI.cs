using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Cloud leaderboard with All-Time and Weekly tabs.
    /// Shows top 10 with player names, depth, and rune count.
    /// </summary>
    public class LeaderboardScreenUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text[] _entryTexts;
        private Text _tabLabel;
        private Text _statusText;
        private System.Action _onClose;
        private bool _isOpen;
        private bool _showingWeekly = true; // Default to weekly

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
        }

        public void Open(System.Action onClose)
        {
            _onClose = onClose;
            _isOpen = true;
            _showingWeekly = true;
            _panel.SetActive(true);
            RefreshEntries();
        }

        private void Update()
        {
            if (!_isOpen) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // Back button (bottom)
            if (ny < 0.13f)
            {
                _isOpen = false;
                _panel.SetActive(false);
                _onClose?.Invoke();
                return;
            }

            // Tab switch (top area, 0.82-0.88)
            if (ny > 0.80f && ny < 0.88f)
            {
                _showingWeekly = !_showingWeekly;
                RefreshEntries();
                return;
            }
        }

        private void RefreshEntries()
        {
            _tabLabel.text = _showingWeekly ? "THIS WEEK  |  all time" : "this week  |  ALL TIME";
            _tabLabel.color = UIHelper.AccentGold;

            var cloud = CloudLeaderboard.Instance;
            if (cloud == null || !cloud.IsLoaded)
            {
                _statusText.text = "Loading scores...";
                _statusText.gameObject.SetActive(true);
                for (int i = 0; i < 10; i++)
                    _entryTexts[i].text = "";
                return;
            }

            var entries = _showingWeekly ? cloud.TopThisWeek : cloud.TopAllTime;
            _statusText.gameObject.SetActive(entries.Count == 0);
            _statusText.text = entries.Count == 0 ?
                (_showingWeekly ? "No scores this week — play to be #1!" : "No scores yet") : "";

            // Also try local entries as fallback
            System.Collections.Generic.List<LeaderboardEntry> localEntries = null;
            if (entries.Count == 0 && ServiceLocator.TryGet<SaveSystem>(out var save))
                localEntries = save.GetLeaderboardEntries();

            for (int i = 0; i < 10; i++)
            {
                if (i < entries.Count)
                {
                    var e = entries[i];
                    string[] parts = e.Date.Split('|');
                    string name = parts.Length > 1 ? parts[0] : "???";
                    string date = parts.Length > 1 ? parts[1] : e.Date;

                    string medal = i == 0 ? ">> " : i == 1 ? "> " : i == 2 ? "> " : "  ";
                    _entryTexts[i].text = $"{medal}#{i + 1}  {name}  —  {e.Depth:F0}m  ({e.RunesCollected} runes)";
                    _entryTexts[i].color = i == 0 ? UIHelper.AccentGold :
                                           i < 3 ? UIHelper.TextWhite : UIHelper.TextDim;
                }
                else if (localEntries != null && i < localEntries.Count)
                {
                    var e = localEntries[i];
                    _entryTexts[i].text = $"  #{i + 1}  (local)  —  {e.Depth:F0}m  ({e.RunesCollected} runes)";
                    _entryTexts[i].color = UIHelper.TextMuted;
                }
                else
                {
                    _entryTexts[i].text = $"  #{i + 1}  ---";
                    _entryTexts[i].color = UIHelper.TextMuted;
                }
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "LeaderboardCanvas", 400);
            _panel = canvas.gameObject;
            var ct = canvas.transform;

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);

            UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.93f),
                "LEADERBOARD", 48, UIHelper.AccentGold);

            // Prize banner
            UIHelper.MakePanel(ct, "PrizeBG",
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.92f),
                new Color(0.2f, 0.15f, 0.02f, 0.8f));
            UIHelper.MakeText(ct, "Prize", new Vector2(0.5f, 0.90f),
                "Weekly Prize: $10 CAD for #1 each week!", 22, UIHelper.AccentGold);

            // Tab switcher
            _tabLabel = UIHelper.MakeText(ct, "Tabs", new Vector2(0.5f, 0.85f),
                "THIS WEEK  |  all time", 28, UIHelper.AccentGold);

            UIHelper.MakeDivider(ct, "Div", 0.82f);

            // Status text (shown when empty)
            _statusText = UIHelper.MakeText(ct, "Status", new Vector2(0.5f, 0.55f),
                "Loading...", 28, UIHelper.TextDim);

            // Entries
            _entryTexts = new Text[10];
            for (int i = 0; i < 10; i++)
            {
                float y = 0.78f - (i * 0.06f);

                if (i % 2 == 0)
                    UIHelper.MakePanel(ct, $"RowBG_{i}",
                        new Vector2(0.03f, y - 0.024f), new Vector2(0.97f, y + 0.024f),
                        new Color(0.05f, 0.03f, 0.08f, 0.5f));

                _entryTexts[i] = UIHelper.MakeText(ct, $"Entry_{i}",
                    new Vector2(0.5f, y), "", 24, UIHelper.TextMuted,
                    TextAnchor.MiddleCenter, 950, 50);
            }

            UIHelper.MakeButton(ct, "Back",
                new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.12f),
                "BACK", 38, UIHelper.BgButton, UIHelper.AccentCyan);
        }
    }
}
