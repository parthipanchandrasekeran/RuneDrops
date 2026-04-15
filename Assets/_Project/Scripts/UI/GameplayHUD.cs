using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;
using RuneDrop.Anchor;

namespace RuneDrop.UI
{
    /// <summary>
    /// Clean gameplay HUD: depth counter, rune slots with colors,
    /// anchor charges as dots, combo banner, and pause button.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        // ── UI References ───────────────────────────────────────────
        private Canvas _canvas;
        private Text _depthText;
        private Text _depthLabel;
        private Image _runeSlotAImg;
        private Image _runeSlotBImg;
        private Text _runeSlotAText;
        private Text _runeSlotBText;
        private Image[] _anchorDots;
        private Text _comboText;
        private Text _pauseText;
        private float _comboDisplayTimer;

        private int _lastAnchorCharges = -1;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Start()
        {
            CreateHUD();
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<ComboExpiredEvent>(OnComboExpired);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<ComboExpiredEvent>(OnComboExpired);
        }

        private void Update()
        {
            UpdateDepth();
            UpdateRuneSlots();
            UpdateAnchorDots();
            UpdateCombo();
            CheckPauseButton();
        }

        // ── Create UI ───────────────────────────────────────────────

        private void CreateHUD()
        {
            _canvas = UIHelper.CreateCanvas(transform, "HUDCanvas", 100);
            var ct = _canvas.transform;

            // ── Top bar background ──────────────────────────────────
            UIHelper.MakePanel(ct, "TopBar",
                new Vector2(0f, 0.93f), new Vector2(1f, 1f),
                new Color(0f, 0f, 0f, 0.5f));

            // ── Depth (large, top center) ───────────────────────────
            _depthText = UIHelper.MakeText(ct, "Depth", new Vector2(0.5f, 0.965f),
                "0", 56, UIHelper.TextWhite);
            _depthLabel = UIHelper.MakeText(ct, "DepthLabel", new Vector2(0.5f, 0.938f),
                "meters", 22, UIHelper.TextDim);

            // ── Rune slots (left side, wider with full names) ──────
            UIHelper.MakeText(ct, "SlotLabel", new Vector2(0.12f, 0.935f),
                "RUNES:", 16, UIHelper.TextMuted, TextAnchor.MiddleCenter, 100, 30);
            _runeSlotAImg = CreateRuneSlot(ct, "SlotA", new Vector2(0.09f, 0.96f));
            _runeSlotBImg = CreateRuneSlot(ct, "SlotB", new Vector2(0.22f, 0.96f));
            _runeSlotAText = UIHelper.MakeText(ct, "SlotAText", new Vector2(0.09f, 0.96f),
                "--", 18, UIHelper.TextDim, TextAnchor.MiddleCenter, 100, 40);
            _runeSlotBText = UIHelper.MakeText(ct, "SlotBText", new Vector2(0.22f, 0.96f),
                "--", 18, UIHelper.TextDim, TextAnchor.MiddleCenter, 100, 40);

            // ── Anchor dots (right side) ────────────────────────────
            _anchorDots = new Image[5]; // Max possible anchors
            for (int i = 0; i < 5; i++)
            {
                float x = 0.92f - (i * 0.045f);
                var dotGO = new GameObject($"AnchorDot_{i}");
                dotGO.transform.SetParent(ct, false);
                var rect = dotGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(x - 0.015f, 0.948f);
                rect.anchorMax = new Vector2(x + 0.015f, 0.968f);
                rect.sizeDelta = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                _anchorDots[i] = dotGO.AddComponent<Image>();
                _anchorDots[i].color = UIHelper.AccentCyan;
                dotGO.SetActive(false);
            }

            // ── Pause button (top right corner) ─────────────────────
            UIHelper.MakePanel(ct, "PauseBG",
                new Vector2(0.90f, 0.93f), new Vector2(1f, 1f),
                new Color(0.2f, 0.1f, 0.3f, 0.6f));
            _pauseText = UIHelper.MakeText(ct, "PauseBtn", new Vector2(0.95f, 0.965f),
                "||", 32, UIHelper.TextDim, TextAnchor.MiddleCenter, 80, 80);

            // ── Combo banner (center, hidden by default) ────────────
            _comboText = UIHelper.MakeText(ct, "Combo", new Vector2(0.5f, 0.8f),
                "", 52, UIHelper.AccentGold);
        }

        private Image CreateRuneSlot(Transform parent, string name, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchor.x - 0.055f, anchor.y - 0.015f);
            rect.anchorMax = new Vector2(anchor.x + 0.055f, anchor.y + 0.015f);
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.1f, 0.2f, 0.8f);
            return img;
        }

        // ── Updates ─────────────────────────────────────────────────

        private void UpdateDepth()
        {
            var player = PlayerController.Instance;
            if (player != null)
                _depthText.text = $"{player.DepthTraveled:F0}";
        }

        private void UpdateRuneSlots()
        {
            var inv = RuneInventory.Instance;
            if (inv == null) return;

            UpdateSlot(_runeSlotAImg, _runeSlotAText, inv.SlotA);
            UpdateSlot(_runeSlotBImg, _runeSlotBText, inv.SlotB);
        }

        private void UpdateSlot(Image bg, Text label, RuneType type)
        {
            if (type == RuneType.None)
            {
                bg.color = new Color(0.15f, 0.1f, 0.2f, 0.8f);
                label.text = "empty";
                label.fontSize = 14;
                label.color = new Color(0.3f, 0.25f, 0.35f);
            }
            else
            {
                Color c = GetRuneColor(type);
                bg.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 0.9f);
                label.text = type switch
                {
                    RuneType.Fire => "FIRE",
                    RuneType.Wind => "WIND",
                    RuneType.Shadow => "SHADOW",
                    RuneType.Earth => "EARTH",
                    _ => "?"
                };
                label.fontSize = 16;
                label.color = c;
            }
        }

        private void UpdateAnchorDots()
        {
            var anchor = AnchorController.Instance;
            if (anchor == null) return;

            int max = anchor.MaxCharges;
            int current = anchor.CurrentCharges;

            for (int i = 0; i < _anchorDots.Length; i++)
            {
                if (i < max)
                {
                    _anchorDots[i].gameObject.SetActive(true);
                    _anchorDots[i].color = i < current ?
                        UIHelper.AccentCyan :
                        new Color(0.15f, 0.15f, 0.2f, 0.6f);
                }
                else
                {
                    _anchorDots[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateCombo()
        {
            if (_comboDisplayTimer > 0f)
            {
                _comboDisplayTimer -= Time.deltaTime;
                // Fade out in last second
                if (_comboDisplayTimer < 1f)
                {
                    var c = _comboText.color;
                    c.a = _comboDisplayTimer;
                    _comboText.color = c;
                }
                if (_comboDisplayTimer <= 0f)
                    _comboText.text = "";
            }
        }

        private float _pauseCooldown;

        private void CheckPauseButton()
        {
            if (_pauseCooldown > 0f)
            {
                _pauseCooldown -= Time.unscaledDeltaTime;
                return;
            }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            if (UIHelper.TapInRect(tapPos, 0.85f, 0.90f, 1f, 1f))
            {
                var gm = GameManager.Instance;
                if (gm != null && gm.CurrentState == GameState.Playing)
                {
                    _pauseCooldown = 0.5f;
                    gm.PauseGame();
                }
            }
        }

        // ── Events ──────────────────────────────────────────────────

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var combo = (ComboType)evt.Combo;
            string name = combo switch
            {
                ComboType.FlameTrail => "FLAME TRAIL",
                ComboType.BlinkDash => "BLINK DASH",
                ComboType.ExplosiveShield => "EXPLOSIVE SHIELD",
                _ => "COMBO!"
            };
            _comboText.text = name;
            _comboText.color = UIHelper.AccentGold;
            _comboDisplayTimer = 3f;
        }

        private void OnComboExpired(ComboExpiredEvent evt)
        {
            _comboText.text = "";
        }

        // ── Helpers ─────────────────────────────────────────────────

        private Color GetRuneColor(RuneType type)
        {
            return type switch
            {
                RuneType.Fire => new Color(1f, 0.4f, 0f),
                RuneType.Wind => UIHelper.AccentCyan,
                RuneType.Shadow => UIHelper.AccentPurple,
                RuneType.Earth => UIHelper.AccentGreen,
                _ => UIHelper.TextDim
            };
        }
    }
}
