using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;
using RuneDrop.Progression;

namespace RuneDrop.UI
{
    /// <summary>
    /// Death screen showing run stats with Retry and Upgrades buttons.
    /// </summary>
    public class DeathScreenUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _titleText;
        private Text _depthText;
        private Text _runesText;
        private Text _shardsText;
        private Text _bestText;

        // Button rects (screen-space normalized)
        private Rect _retryRect;
        private Rect _upgradesRect;

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
            _bestText.text = $"Best: {save?.BestDepth ?? 0:F0}m  |  Total: {save?.SoulShards ?? 0}";

            if (summary != null && summary.IsNewBestDepth)
            {
                _titleText.text = "NEW BEST!";
                _titleText.color = new Color(1f, 0.8f, 0f);
            }
            else
            {
                _titleText.text = "YOU FELL";
                _titleText.color = new Color(1f, 0.3f, 0.3f);
            }

            _panel.SetActive(true);
        }

        private void Update()
        {
            if (!_panel.activeSelf) return;

            Vector2 tapPos;
            if (!GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // RETRY button (left side, bottom area: x 0.05-0.48, y 0.08-0.18)
            if (nx > 0.05f && nx < 0.48f && ny > 0.05f && ny < 0.22f)
            {
                _panel.SetActive(false);
                if (GameManager.Instance != null)
                    GameManager.Instance.StartRun();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            // UPGRADES button (right side, bottom area: x 0.52-0.95, y 0.08-0.18)
            if (nx > 0.52f && nx < 0.95f && ny > 0.05f && ny < 0.22f)
            {
                _panel.SetActive(false);
                var shop = FindFirstObjectByType<UpgradeShopUI>();
                if (shop != null) shop.Open();
                return;
            }
        }

        private bool GetTap(out Vector2 pos)
        {
            pos = Vector2.zero;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                pos = Input.GetTouch(0).position;
                return true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                pos = Input.mousePosition;
                return true;
            }
            return false;
        }

        private void CreateDeathScreen()
        {
            var canvasGO = new GameObject("DeathCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();
            _panel = canvasGO;

            // SOLID dark overlay — fully opaque to hide the game world behind
            MakePanel(canvasGO.transform, "Overlay", Vector2.zero, Vector2.one, new Color(0.02f, 0.01f, 0.05f, 1f));

            // Title
            _titleText = MakeText(canvasGO.transform, "Title", new Vector2(0.5f, 0.72f),
                "YOU FELL", 72, Color.red);

            // Stats
            _depthText = MakeText(canvasGO.transform, "Depth", new Vector2(0.5f, 0.58f),
                "Depth: 0m", 44, Color.white);
            _runesText = MakeText(canvasGO.transform, "Runes", new Vector2(0.5f, 0.50f),
                "Runes: 0", 44, Color.white);
            _shardsText = MakeText(canvasGO.transform, "Shards", new Vector2(0.5f, 0.42f),
                "+0 Soul Shards", 48, new Color(0.8f, 0.6f, 1f));
            _bestText = MakeText(canvasGO.transform, "Best", new Vector2(0.5f, 0.34f),
                "Best: 0m", 32, new Color(0.7f, 0.7f, 0.7f));

            // RETRY button (left)
            MakePanel(canvasGO.transform, "RetryBG",
                new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.2f),
                new Color(0.1f, 0.4f, 0.1f, 0.9f));
            MakeText(canvasGO.transform, "RetryText", new Vector2(0.265f, 0.14f),
                "RETRY", 42, new Color(0.3f, 1f, 0.3f));

            // UPGRADES button (right)
            MakePanel(canvasGO.transform, "UpgradesBG",
                new Vector2(0.52f, 0.08f), new Vector2(0.95f, 0.2f),
                new Color(0.3f, 0.15f, 0.4f, 0.9f));
            MakeText(canvasGO.transform, "UpgradesText", new Vector2(0.735f, 0.14f),
                "UPGRADES", 38, new Color(0.8f, 0.6f, 1f));
        }

        private GameObject MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

        private Text MakeText(Transform parent, string name, Vector2 anchor, string text, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(500, 80);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = size;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            go.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
            return txt;
        }
    }
}
