using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Overhauled leaderboard with stronger tab hierarchy and row readability.
    /// </summary>
    public class LeaderboardScreenUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text[] _entryTexts;
        private Text _tabLabel;
        private Text _statusText;
        private System.Action _onClose;
        private bool _isOpen;
        private bool _showingWeekly = true;

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

            float ny = tapPos.y / Screen.height;

            if (ny < 0.13f)
            {
                UIHelper.LightHaptic();
                _isOpen = false;
                _panel.SetActive(false);
                _onClose?.Invoke();
                return;
            }

            if (ny > 0.80f && ny < 0.89f)
            {
                UIHelper.LightHaptic();
                _showingWeekly = !_showingWeekly;
                RefreshEntries();
                return;
            }
        }

        private void RefreshEntries()
        {
            _tabLabel.text = _showingWeekly ? "WEEKLY  •  all-time" : "weekly  •  ALL-TIME";
            _tabLabel.color = _showingWeekly ? UIHelper.AccentGold : UIHelper.AccentCyan;

            var cloud = CloudLeaderboard.Instance;
            if (cloud == null || !cloud.IsLoaded)
            {
                _statusText.text = "Syncing cloud scores...";
                _statusText.gameObject.SetActive(true);
                for (int i = 0; i < 10; i++)
                    _entryTexts[i].text = "";
                return;
            }

            var entries = _showingWeekly ? cloud.TopThisWeek : cloud.TopAllTime;
            _statusText.gameObject.SetActive(entries.Count == 0);
            _statusText.text = entries.Count == 0 ?
                (_showingWeekly ? "No weekly scores yet — become #1." : "No all-time scores yet") : "";

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
                    _entryTexts[i].text = $"#{i + 1,-2}  {name,-12}   {e.Depth,4:F0}m   {e.RunesCollected,2} runes";
                    _entryTexts[i].color = i == 0 ? UIHelper.AccentGold : (i < 3 ? UIHelper.TextWhite : UIHelper.TextDim);
                }
                else if (localEntries != null && i < localEntries.Count)
                {
                    var e = localEntries[i];
                    _entryTexts[i].text = $"#{i + 1,-2}  LOCAL         {e.Depth,4:F0}m   {e.RunesCollected,2} runes";
                    _entryTexts[i].color = UIHelper.TextMuted;
                }
                else
                {
                    _entryTexts[i].text = $"#{i + 1,-2}  ---";
                    _entryTexts[i].color = UIHelper.TextMuted;
                }
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "LeaderboardCanvas", 400);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);
            UIHelper.MakeCard(ct, "BoardCard", new Vector2(0.03f, 0.16f), new Vector2(0.97f, 0.95f),
                new Color(0.07f, 0.1f, 0.17f, 0.96f), new Color(0.4f, 0.72f, 0.95f, 0.32f));

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.92f), "LEADERBOARD", 48, UIHelper.AccentCyan);
            UIHelper.MakeText(ct, "Prize", new Vector2(0.5f, 0.885f), "Weekly Crown Prize: $10 CAD", 22, UIHelper.AccentGold);
            _tabLabel = UIHelper.MakeText(ct, "Tabs", new Vector2(0.5f, 0.84f), "WEEKLY  •  all-time", 30, UIHelper.AccentGold);
            UIHelper.MakeDivider(ct, "Div", 0.81f);

            _statusText = UIHelper.MakeText(ct, "Status", new Vector2(0.5f, 0.55f), "Loading...", 28, UIHelper.TextDim);

            _entryTexts = new Text[10];
            for (int i = 0; i < 10; i++)
            {
                float y = 0.76f - (i * 0.058f);
                Color rowColor = i % 2 == 0 ? new Color(0.1f, 0.14f, 0.22f, 0.55f) : new Color(0.06f, 0.09f, 0.15f, 0.5f);
                UIHelper.MakePanel(ct, $"RowBG_{i}", new Vector2(0.06f, y - 0.022f), new Vector2(0.94f, y + 0.022f), rowColor);
                _entryTexts[i] = UIHelper.MakeText(ct, $"Entry_{i}", new Vector2(0.5f, y), "", 24, UIHelper.TextMuted,
                    TextAnchor.MiddleCenter, 960, 50);
            }

            UIHelper.MakeButton(ct, "Back", new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.12f),
                "BACK", 38, new Color(0.11f, 0.18f, 0.28f, 0.96f), UIHelper.AccentCyan);
            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }
    }
}
