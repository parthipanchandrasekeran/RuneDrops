using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;
using RuneDrop.Anchor;

namespace RuneDrop.UI
{
    /// <summary>
    /// Overhauled HUD with clearer grouping and stronger combat readability.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _depthText;
        private Text _depthLabel;
        private Image _runeSlotAImg;
        private Image _runeSlotBImg;
        private Text _runeSlotAText;
        private Text _runeSlotBText;
        private Image[] _anchorDots;
        private Text _comboText;
        private float _comboDisplayTimer;
        private float _pauseCooldown;

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

        private void CreateHUD()
        {
            _canvas = UIHelper.CreateCanvas(transform, "HUDCanvas", 100);
            var ct = UIHelper.GetSafeAreaRoot(_canvas);

            UIHelper.MakePanel(ct, "TopBarBG", new Vector2(0f, 0.91f), new Vector2(1f, 1f), new Color(0.02f, 0.05f, 0.12f, 0.82f));
            UIHelper.MakePanel(ct, "TopBarEdge", new Vector2(0f, 0.91f), new Vector2(1f, 0.914f), new Color(0.45f, 0.85f, 1f, 0.35f));

            _depthText = UIHelper.MakeGlowText(ct, "Depth", new Vector2(0.5f, 0.964f), "0", 54, UIHelper.TextWhite);
            _depthLabel = UIHelper.MakeText(ct, "DepthLabel", new Vector2(0.5f, 0.936f), "DEPTH", 18, UIHelper.TextDim);

            UIHelper.MakeText(ct, "SlotLabel", new Vector2(0.16f, 0.936f), "RUNES", 16, UIHelper.TextMuted, TextAnchor.MiddleCenter, 140, 30);
            _runeSlotAImg = CreateRuneSlot(ct, "SlotA", new Vector2(0.1f, 0.963f));
            _runeSlotBImg = CreateRuneSlot(ct, "SlotB", new Vector2(0.23f, 0.963f));
            _runeSlotAText = UIHelper.MakeText(ct, "SlotAText", new Vector2(0.1f, 0.963f), "--", 16, UIHelper.TextDim, TextAnchor.MiddleCenter, 110, 42);
            _runeSlotBText = UIHelper.MakeText(ct, "SlotBText", new Vector2(0.23f, 0.963f), "--", 16, UIHelper.TextDim, TextAnchor.MiddleCenter, 110, 42);

            _anchorDots = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                float x = 0.91f - (i * 0.05f);
                var dotGO = new GameObject($"AnchorDot_{i}");
                dotGO.transform.SetParent(ct, false);
                var rect = dotGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(x - 0.014f, 0.948f);
                rect.anchorMax = new Vector2(x + 0.014f, 0.972f);
                rect.sizeDelta = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                _anchorDots[i] = dotGO.AddComponent<Image>();
                _anchorDots[i].color = UIHelper.AccentCyan;
                dotGO.SetActive(false);
            }

            UIHelper.MakePanel(ct, "PauseBG", new Vector2(0.905f, 0.918f), new Vector2(0.995f, 0.997f), new Color(0.08f, 0.14f, 0.22f, 0.9f));
            UIHelper.MakeText(ct, "PauseBtn", new Vector2(0.95f, 0.957f), "II", 30, UIHelper.TextWhite, TextAnchor.MiddleCenter, 80, 80);

            _comboText = UIHelper.MakeGlowText(ct, "Combo", new Vector2(0.5f, 0.8f), "", 54, UIHelper.AccentGold);
        }

        private Image CreateRuneSlot(Transform parent, string name, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchor.x - 0.058f, anchor.y - 0.018f);
            rect.anchorMax = new Vector2(anchor.x + 0.058f, anchor.y + 0.018f);
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            return img;
        }

        private void UpdateDepth()
        {
            var player = PlayerController.Instance;
            if (player != null)
                _depthText.text = $"{player.DepthTraveled:F0}m";
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
                bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
                label.text = "EMPTY";
                label.fontSize = 13;
                label.color = UIHelper.TextMuted;
            }
            else
            {
                Color c = GetRuneColor(type);
                bg.color = new Color(c.r * 0.27f, c.g * 0.27f, c.b * 0.27f, 0.98f);
                label.text = type switch
                {
                    RuneType.Fire => "FIRE",
                    RuneType.Wind => "WIND",
                    RuneType.Shadow => "SHADOW",
                    RuneType.Earth => "EARTH",
                    _ => "?"
                };
                label.fontSize = 15;
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
                    _anchorDots[i].color = i < current ? UIHelper.AccentCyan : new Color(0.2f, 0.25f, 0.32f, 0.7f);
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

        private void CheckPauseButton()
        {
            if (_pauseCooldown > 0f)
            {
                _pauseCooldown -= Time.unscaledDeltaTime;
                return;
            }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            if (UIHelper.TapInRect(tapPos, 0.88f, 0.90f, 1f, 1f))
            {
                var gm = GameManager.Instance;
                if (gm != null && gm.CurrentState == GameState.Playing)
                {
                    _pauseCooldown = 0.5f;
                    gm.PauseGame();
                }
            }
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var combo = (ComboType)evt.Combo;
            _comboText.text = combo switch
            {
                ComboType.FlameTrail => "FLAME TRAIL",
                ComboType.BlinkDash => "BLINK DASH",
                ComboType.ExplosiveShield => "EXPLOSIVE SHIELD",
                _ => "COMBO!"
            };
            _comboText.color = UIHelper.AccentGold;
            _comboDisplayTimer = 3f;
        }

        private void OnComboExpired(ComboExpiredEvent evt)
        {
            _comboText.text = "";
        }

        private Color GetRuneColor(RuneType type)
        {
            return type switch
            {
                RuneType.Fire => new Color(1f, 0.45f, 0.25f),
                RuneType.Wind => new Color(0.45f, 1f, 0.85f),
                RuneType.Shadow => new Color(0.75f, 0.45f, 1f),
                RuneType.Earth => new Color(0.95f, 0.8f, 0.38f),
                _ => UIHelper.TextDim
            };
        }
    }
}
