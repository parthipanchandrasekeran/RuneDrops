using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Progression;

namespace RuneDrop.UI
{
    /// <summary>
    /// Premium run-end screen with clear summary hierarchy and next actions.
    /// </summary>
    public class DeathScreenUI : MonoBehaviour
    {
        private GameObject _panel;
        private UnityEngine.UI.Text _titleText;
        private UnityEngine.UI.Text _depthText;
        private UnityEngine.UI.Text _runesText;
        private UnityEngine.UI.Text _shardsText;
        private UnityEngine.UI.Text _bestText;
        private UnityEngine.UI.Text _goalText;

        private void Start()
        {
            CreateDeathScreen();
            _panel.SetActive(false);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            Invoke(nameof(ShowDeathScreen), 0.5f);
        }

        private void ShowDeathScreen()
        {
            var meta = MetaProgressionManager.Instance;
            var summary = meta?.LastRunSummary;
            SaveData save = null;
            if (ServiceLocator.TryGet<SaveSystem>(out var saveSystem))
                save = saveSystem.Data;

            _depthText.text = $"Depth: {(summary?.DepthReached ?? 0):F0}m";
            _runesText.text = $"Runes: {summary?.RunesCollected ?? 0}";
            _shardsText.text = $"+{summary?.SoulShardsEarned ?? 0} Soul Shards";
            _bestText.text = $"Best: {save?.BestDepth ?? 0:F0}m  ·  Total Shards: {save?.SoulShards ?? 0}";
            int currentDepth = Mathf.RoundToInt(summary?.DepthReached ?? 0);
            int goal = Mathf.Max(25, Mathf.CeilToInt((currentDepth + 1f) / 25f) * 25);
            int left = Mathf.Max(0, goal - currentDepth);
            _goalText.text = left <= 0 ? $"Milestone {goal}m cleared" : $"{left}m to next milestone ({goal}m)";

            if (summary != null && summary.IsNewBestDepth)
            {
                _titleText.text = "NEW BEST!";
                _titleText.color = UIHelper.AccentGold;
            }
            else
            {
                _titleText.text = "RUN ENDED";
                _titleText.color = UIHelper.AccentRed;
            }

            _panel.SetActive(true);
        }

        private void Update()
        {
            if (!_panel.activeSelf) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            if (nx > 0.05f && nx < 0.48f && ny > 0.05f && ny < 0.22f)
            {
                UIHelper.LightHaptic();
                _panel.SetActive(false);
                if (GameManager.Instance != null)
                    GameManager.Instance.StartRun();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            if (nx > 0.52f && nx < 0.95f && ny > 0.05f && ny < 0.22f)
            {
                UIHelper.LightHaptic();
                _panel.SetActive(false);
                var shop = FindFirstObjectByType<UpgradeShopUI>();
                if (shop != null) shop.Open();
                return;
            }
        }

        private void CreateDeathScreen()
        {
            var canvas = UIHelper.CreateCanvas(transform, "DeathCanvas", 200);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "Overlay", Vector2.zero, Vector2.one, new Color(0.01f, 0.02f, 0.05f, 0.93f));
            UIHelper.MakeCard(ct, "SummaryCard", new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.80f),
                new Color(0.07f, 0.1f, 0.17f, 0.96f), new Color(0.45f, 0.72f, 0.95f, 0.32f));

            _titleText = UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.73f), "RUN ENDED", 76, UIHelper.AccentRed);
            _depthText = UIHelper.MakeText(ct, "Depth", new Vector2(0.5f, 0.61f), "Depth: 0m", 44, UIHelper.TextWhite);
            _runesText = UIHelper.MakeText(ct, "Runes", new Vector2(0.5f, 0.54f), "Runes: 0", 42, UIHelper.TextWhite);
            _shardsText = UIHelper.MakeGlowText(ct, "Shards", new Vector2(0.5f, 0.46f), "+0 Soul Shards", 46, UIHelper.AccentPurple);
            _bestText = UIHelper.MakeText(ct, "Best", new Vector2(0.5f, 0.39f), "Best: 0m", 28, UIHelper.TextDim, TextAnchor.MiddleCenter, 900, 50);
            _goalText = UIHelper.MakeText(ct, "Goal", new Vector2(0.5f, 0.34f), "0m to next milestone", 24, UIHelper.AccentCyan, TextAnchor.MiddleCenter, 900, 50);

            UIHelper.MakeButton(ct, "Retry", new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.2f),
                "RETRY", 42, new Color(0.08f, 0.28f, 0.2f, 0.96f), UIHelper.AccentGreen);
            UIHelper.MakeButton(ct, "Upgrades", new Vector2(0.52f, 0.08f), new Vector2(0.95f, 0.2f),
                "UPGRADES", 36, new Color(0.16f, 0.1f, 0.26f, 0.96f), UIHelper.AccentPurple);
            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }
    }
}
