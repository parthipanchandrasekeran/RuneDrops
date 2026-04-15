using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Player;
using RuneDrop.Core;
using RuneDrop.UI;

namespace RuneDrop.Decision
{
    /// <summary>
    /// Premium-styled decision overlay with clear contrast and large tap targets.
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
            if (_entered || _decided || !other.CompareTag("Player")) return;
            _entered = true;
            _enterCooldown = 0.3f;
            EnterRoom();
        }

        private void EnterRoom()
        {
            var player = PlayerController.Instance;
            if (player != null)
                player.SetFallSpeedMultiplier(0.3f);

            var hits = Physics2D.OverlapCircleAll(transform.position, 8f);
            foreach (var hit in hits)
            {
                if (hit.gameObject.layer == 7)
                    Destroy(hit.gameObject);
            }

            var feedbackCanvas = GameObject.Find("FeedbackCanvas");
            if (feedbackCanvas != null)
            {
                for (int i = feedbackCanvas.transform.childCount - 1; i >= 0; i--)
                    Destroy(feedbackCanvas.transform.GetChild(i).gameObject);
            }

            CreateBannerUI();
        }

        private void Update()
        {
            if (!_entered || _decided) return;
            if (_enterCooldown > 0f) { _enterCooldown -= Time.deltaTime; return; }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;
            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            // Wide, generous touch area for decision cards
            if (ny < 0.60f) return;

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
            UIHelper.LightHaptic();
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
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "TopOverlay", new Vector2(0f, 0.58f), new Vector2(1f, 1f), new Color(0.02f, 0.04f, 0.09f, 0.88f));
            UIHelper.MakePanel(ct, "TopEdge", new Vector2(0f, 0.58f), new Vector2(1f, 0.584f), new Color(0.5f, 0.82f, 1f, 0.35f));

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.955f), "CHOOSE YOUR PATH", 44, UIHelper.AccentGold, 900, 80);
            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.915f), "Safe reward or risky payoff", 24, UIHelper.TextDim, TextAnchor.MiddleCenter, 900, 60);

            // Left: safe
            UIHelper.MakeCard(ct, "SafeCard", new Vector2(0.04f, 0.66f), new Vector2(0.48f, 0.89f),
                new Color(0.08f, 0.19f, 0.14f, 0.96f), new Color(0.35f, 0.95f, 0.65f, 0.35f));
            UIHelper.MakeText(ct, "LeftType", new Vector2(0.26f, 0.865f), "SAFE", 24, UIHelper.AccentGreen, TextAnchor.MiddleCenter, 350, 50);
            UIHelper.MakeGlowText(ct, "LeftName", new Vector2(0.26f, 0.805f), _choices[0].Name, 42, UIHelper.TextWhite, 420, 70);
            UIHelper.MakeText(ct, "LeftDesc", new Vector2(0.26f, 0.735f), _choices[0].Description, 24, UIHelper.TextDim, TextAnchor.MiddleCenter, 420, 70);
            UIHelper.MakeText(ct, "LeftTap", new Vector2(0.26f, 0.685f), "TAP LEFT", 20, UIHelper.AccentGreen, TextAnchor.MiddleCenter, 300, 50);

            // Right: risky
            UIHelper.MakeCard(ct, "RiskCard", new Vector2(0.52f, 0.66f), new Vector2(0.96f, 0.89f),
                new Color(0.22f, 0.09f, 0.1f, 0.96f), new Color(1f, 0.42f, 0.45f, 0.35f));
            UIHelper.MakeText(ct, "RightType", new Vector2(0.74f, 0.865f), "RISKY", 24, UIHelper.AccentRed, TextAnchor.MiddleCenter, 350, 50);
            UIHelper.MakeGlowText(ct, "RightName", new Vector2(0.74f, 0.805f), _choices[1].Name, 42, UIHelper.TextWhite, 420, 70);
            UIHelper.MakeText(ct, "RightDesc", new Vector2(0.74f, 0.735f), _choices[1].Description, 24, UIHelper.TextDim, TextAnchor.MiddleCenter, 420, 70);
            UIHelper.MakeText(ct, "RightTap", new Vector2(0.74f, 0.685f), "TAP RIGHT", 20, UIHelper.AccentRed, TextAnchor.MiddleCenter, 300, 50);

            UIHelper.MakeText(ct, "Hint", new Vector2(0.5f, 0.615f), "Time slows while you choose", 20, UIHelper.TextMuted, TextAnchor.MiddleCenter, 900, 50);
            UIFXAnimator.Attach(_canvasGO, 0.16f, 0.985f);
        }
    }
}
