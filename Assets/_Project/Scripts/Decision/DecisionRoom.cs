using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Player;
using RuneDrop.Core;

namespace RuneDrop.Decision
{
    /// <summary>
    /// Decision room — shows a compact banner at top of screen with two choices.
    /// NOT full-screen — player can still see the game world.
    /// Tap left or right button to choose. Fall speed slows but doesn't stop.
    /// </summary>
    public class DecisionRoom : MonoBehaviour
    {
        private DecisionRoomManager _manager;
        private DecisionChoice[] _choices;
        private bool _entered;
        private bool _decided;
        private float _enterCooldown;
        private GameObject _canvasGO;

        public void Initialize(DecisionRoomManager manager, DecisionChoice[] choices)
        {
            _manager = manager;
            _choices = choices;
            _entered = false;
            _decided = false;

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(12f, 3f);
            gameObject.layer = 9;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_entered || _decided) return;
            if (!other.CompareTag("Player")) return;
            _entered = true;
            _enterCooldown = 0.3f;
            EnterRoom();
        }

        private void EnterRoom()
        {
            var player = PlayerController.Instance;
            if (player != null)
                player.SetFallSpeedMultiplier(0.3f); // Slow but not stopped

            CreateBannerUI();
            Debug.Log("[Decision] Choose!");
        }

        private void Update()
        {
            if (!_entered || _decided) return;

            if (_enterCooldown > 0f) { _enterCooldown -= Time.deltaTime; return; }

            Vector2 tapPos;
            bool tapped = false;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            { tapped = true; tapPos = Input.GetTouch(0).position; }
            else if (Input.GetMouseButtonDown(0))
            { tapped = true; tapPos = Input.mousePosition; }
            else return;

            tapPos = ScreenSetup.FixTouchPos(tapPos);
            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // Only respond to taps in the banner area (top 30% of screen)
            if (ny < 0.65f) return;

            int choice = nx < 0.5f ? 0 : 1;
            MakeChoice(choice);
        }

        private void MakeChoice(int index)
        {
            if (_decided) return;
            _decided = true;

            var player = PlayerController.Instance;
            if (player != null) player.ResetFallSpeedMultiplier();

            if (_canvasGO != null) Destroy(_canvasGO);
            _manager.OnChoiceMade(_choices[index]);
        }

        private void OnDestroy()
        {
            if (_canvasGO != null) Destroy(_canvasGO);
        }

        private void CreateBannerUI()
        {
            _canvasGO = new GameObject("DecisionBanner");
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = _canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasGO.AddComponent<GraphicRaycaster>();
            var ct = _canvasGO.transform;

            // Semi-transparent top banner background
            MakePanel(ct, "BannerBG", new Vector2(0f, 0.68f), new Vector2(1f, 1f),
                new Color(0f, 0f, 0.03f, 0.9f));

            // "CHOOSE" title
            MakeText(ct, "Title", new Vector2(0.5f, 0.96f),
                "CHOOSE YOUR PATH", 36, new Color(1f, 0.8f, 0.3f));

            // Left choice (Safe) — green
            MakePanel(ct, "LeftBG", new Vector2(0.03f, 0.72f), new Vector2(0.48f, 0.93f),
                new Color(0.04f, 0.15f, 0.04f, 0.95f));
            MakeText(ct, "LeftType", new Vector2(0.255f, 0.91f),
                "SAFE", 24, new Color(0.3f, 1f, 0.4f));
            MakeText(ct, "LeftName", new Vector2(0.255f, 0.85f),
                _choices[0].Name, 32, Color.white);
            MakeText(ct, "LeftDesc", new Vector2(0.255f, 0.77f),
                _choices[0].Description, 20, new Color(0.7f, 0.7f, 0.7f));

            // Right choice (Risky) — red
            MakePanel(ct, "RightBG", new Vector2(0.52f, 0.72f), new Vector2(0.97f, 0.93f),
                new Color(0.15f, 0.04f, 0.04f, 0.95f));
            MakeText(ct, "RightType", new Vector2(0.745f, 0.91f),
                "RISKY", 24, new Color(1f, 0.3f, 0.3f));
            MakeText(ct, "RightName", new Vector2(0.745f, 0.85f),
                _choices[1].Name, 32, Color.white);
            MakeText(ct, "RightDesc", new Vector2(0.745f, 0.77f),
                _choices[1].Description, 20, new Color(0.7f, 0.7f, 0.7f));

            // Hint
            MakeText(ct, "Hint", new Vector2(0.5f, 0.695f),
                "TAP LEFT or RIGHT", 22, new Color(0.5f, 0.5f, 0.6f));
        }

        private GameObject MakePanel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.sizeDelta = Vector2.zero; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private Text MakeText(Transform parent, string name, Vector2 anchor, string text, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(500, 60);
            var txt = go.AddComponent<Text>();
            txt.text = text; txt.fontSize = size;
            txt.alignment = TextAnchor.MiddleCenter; txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            go.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            return txt;
        }
    }
}
