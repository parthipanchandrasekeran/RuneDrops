using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Full-screen reference card showing all rune powers and combos.
    /// Accessible from Main Menu and during gameplay (pause).
    /// </summary>
    public class PowersReferenceUI : MonoBehaviour
    {
        private GameObject _panel;
        private System.Action _onClose;
        private bool _isOpen;

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
        }

        public void Open(System.Action onClose = null)
        {
            _onClose = onClose;
            _isOpen = true;
            _panel.SetActive(true);
        }

        private void Update()
        {
            if (!_isOpen) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float ny = tapPos.y / Screen.height;
            if (ny < 0.12f)
            {
                UIHelper.LightHaptic();
                _isOpen = false;
                _panel.SetActive(false);
                _onClose?.Invoke();
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "PowersCanvas", 450);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.93f),
                "RUNE POWERS", 48, UIHelper.AccentCyan);
            UIHelper.MakeDivider(ct, "Div1", 0.89f);

            // ── Single Runes ────────────────────────────────────────
            UIHelper.MakeText(ct, "SingleLabel", new Vector2(0.5f, 0.86f),
                "SINGLE RUNES (collect one)", 26, UIHelper.AccentGold);

            float y = 0.80f;
            MakePowerRow(ct, y, "FIRE", "Destroys red blocks near you for 5 sec",
                new Color(1f, 0.5f, 0f), "orange");
            y -= 0.065f;
            MakePowerRow(ct, y, "WIND", "You fall slower for 5 sec",
                UIHelper.AccentCyan, "cyan");
            y -= 0.065f;
            MakePowerRow(ct, y, "SHADOW", "Pass through red blocks for 5 sec",
                UIHelper.AccentPurple, "purple");
            y -= 0.065f;
            MakePowerRow(ct, y, "EARTH", "Survive the next hit (shield)",
                UIHelper.AccentGreen, "green");

            UIHelper.MakeDivider(ct, "Div2", y - 0.03f);

            // ── Combos ──────────────────────────────────────────────
            y -= 0.06f;
            UIHelper.MakeText(ct, "ComboLabel", new Vector2(0.5f, y),
                "COMBOS (collect two matching)", 26, UIHelper.AccentGold);

            y -= 0.07f;
            MakeComboRow(ct, y, "FIRE + WIND", "FLAME TRAIL",
                "Burns blocks near you + slows fall for 8 sec",
                new Color(1f, 0.6f, 0.2f));
            y -= 0.08f;
            MakeComboRow(ct, y, "SHADOW + WIND", "BLINK DASH",
                "Teleport down 5 meters through everything",
                new Color(0.5f, 0.6f, 1f));
            y -= 0.08f;
            MakeComboRow(ct, y, "EARTH + FIRE", "EXPLOSIVE SHIELD",
                "Next hit won't kill you + explodes all blocks nearby",
                new Color(0.5f, 1f, 0.5f));

            UIHelper.MakeDivider(ct, "Div3", y - 0.04f);

            // ── Anchor ──────────────────────────────────────────────
            y -= 0.07f;
            UIHelper.MakeText(ct, "AnchorLabel", new Vector2(0.5f, y),
                "ANCHOR (tap quickly)", 26, UIHelper.AccentCyan);
            y -= 0.05f;
            UIHelper.MakeText(ct, "AnchorDesc", new Vector2(0.5f, y),
                "Slows your fall for 1.5 sec. Limited charges per run.", 22, UIHelper.TextDim);

            // Back button
            UIHelper.MakeButton(ct, "Back", new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.10f),
                "BACK", 38, new Color(0.11f, 0.18f, 0.28f, 0.96f), UIHelper.AccentCyan);
        }

        private void MakePowerRow(Transform ct, float y, string name, string desc, Color color, string colorName)
        {
            UIHelper.MakePanel(ct, $"Row_{name}", new Vector2(0.05f, y - 0.025f),
                new Vector2(0.95f, y + 0.025f), new Color(color.r * 0.15f, color.g * 0.15f, color.b * 0.15f, 0.8f));
            UIHelper.MakeText(ct, $"Name_{name}", new Vector2(0.18f, y),
                name, 28, color, TextAnchor.MiddleLeft, 200, 50);
            UIHelper.MakeText(ct, $"Desc_{name}", new Vector2(0.62f, y),
                desc, 20, UIHelper.TextDim, TextAnchor.MiddleLeft, 500, 50);
        }

        private void MakeComboRow(Transform ct, float y, string recipe, string name, string desc, Color color)
        {
            UIHelper.MakePanel(ct, $"CRow_{name}", new Vector2(0.05f, y - 0.032f),
                new Vector2(0.95f, y + 0.032f), new Color(0.1f, 0.12f, 0.2f, 0.85f));
            UIHelper.MakeText(ct, $"CRecipe_{name}", new Vector2(0.5f, y + 0.015f),
                $"{recipe}  =  {name}", 26, color);
            UIHelper.MakeText(ct, $"CDesc_{name}", new Vector2(0.5f, y - 0.015f),
                desc, 20, UIHelper.TextDim);
        }
    }
}
