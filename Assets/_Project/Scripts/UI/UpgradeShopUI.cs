using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;
using RuneDrop.Progression;

namespace RuneDrop.UI
{
    public class UpgradeShopUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _shardsText;
        private Text[] _nameTexts;
        private Text[] _costTexts;
        private Text[] _levelTexts;
        private Image[] _rowBGs;
        private bool _isOpen;

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
        }

        public void Open()
        {
            _isOpen = true;
            _panel.SetActive(true);
            RefreshUI();
        }

        public void Close()
        {
            _isOpen = false;
            _panel.SetActive(false);
        }

        private void Update()
        {
            if (!_isOpen) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            if (ny < 0.14f)
            {
                UIHelper.LightHaptic();
                Close();
                if (GameManager.Instance != null)
                    GameManager.Instance.StartRun();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                float rowCenter = 0.70f - (i * 0.10f);
                if (nx > 0.06f && nx < 0.94f && ny > rowCenter - 0.045f && ny < rowCenter + 0.045f)
                {
                    TryBuyUpgrade(i);
                    return;
                }
            }
        }

        private void TryBuyUpgrade(int index)
        {
            var meta = MetaProgressionManager.Instance;
            if (meta == null) return;
            if (index < 0 || index >= MetaProgressionManager.Upgrades.Length) return;

            var def = MetaProgressionManager.Upgrades[index];
            if (meta.TryPurchaseUpgrade(def.Id))
            {
                UIHelper.LightHaptic();
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            var meta = MetaProgressionManager.Instance;
            if (meta == null) return;

            SaveData save = null;
            if (ServiceLocator.TryGet<SaveSystem>(out var sys)) save = sys.Data;
            int shards = save?.SoulShards ?? 0;
            _shardsText.text = $"Soul Shards: {shards}";

            for (int i = 0; i < MetaProgressionManager.Upgrades.Length; i++)
            {
                var def = MetaProgressionManager.Upgrades[i];
                int level = meta.GetUpgradeLevel(def.Id);
                bool maxed = meta.IsMaxLevel(def.Id);
                int cost = meta.GetUpgradeCost(def.Id);
                bool canAfford = shards >= cost;

                _nameTexts[i].text = $"{def.Name}\n<size=18>{def.Description}</size>";

                string levelBar = "";
                for (int l = 0; l < def.MaxLevel; l++)
                    levelBar += l < level ? "●" : "○";
                _levelTexts[i].text = maxed ? "MAX" : levelBar;
                _levelTexts[i].color = maxed ? UIHelper.AccentGold : UIHelper.TextWhite;

                if (maxed)
                {
                    _costTexts[i].text = "MAX";
                    _costTexts[i].color = UIHelper.TextMuted;
                    _rowBGs[i].color = new Color(0.09f, 0.13f, 0.2f, 0.88f);
                }
                else
                {
                    _costTexts[i].text = cost.ToString();
                    _costTexts[i].color = canAfford ? UIHelper.AccentGreen : UIHelper.AccentRed;
                    _rowBGs[i].color = canAfford ? new Color(0.08f, 0.2f, 0.14f, 0.9f) : new Color(0.19f, 0.1f, 0.15f, 0.9f);
                }
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "ShopCanvas", 300);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);
            UIHelper.MakeCard(ct, "ShopCard", new Vector2(0.03f, 0.16f), new Vector2(0.97f, 0.95f),
                new Color(0.07f, 0.1f, 0.17f, 0.96f), new Color(0.42f, 0.7f, 0.95f, 0.3f));

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.91f), "UPGRADES", 58, UIHelper.AccentPurple);
            _shardsText = UIHelper.MakeGlowText(ct, "Shards", new Vector2(0.5f, 0.85f), "Soul Shards: 0", 36, UIHelper.AccentCyan);
            UIHelper.MakeDivider(ct, "Div1", 0.81f);

            UIHelper.MakeText(ct, "ColName", new Vector2(0.24f, 0.78f), "Upgrade", 22, UIHelper.TextMuted, TextAnchor.MiddleLeft, 300, 40);
            UIHelper.MakeText(ct, "ColLevel", new Vector2(0.63f, 0.78f), "Level", 22, UIHelper.TextMuted, TextAnchor.MiddleCenter, 170, 40);
            UIHelper.MakeText(ct, "ColCost", new Vector2(0.84f, 0.78f), "Cost", 22, UIHelper.TextMuted, TextAnchor.MiddleCenter, 140, 40);

            _nameTexts = new Text[5];
            _costTexts = new Text[5];
            _levelTexts = new Text[5];
            _rowBGs = new Image[5];

            for (int i = 0; i < 5; i++)
            {
                float y = 0.70f - (i * 0.10f);
                var rowGO = UIHelper.MakePanel(ct, $"Row_{i}", new Vector2(0.06f, y - 0.04f), new Vector2(0.94f, y + 0.04f), new Color(0.1f, 0.14f, 0.22f, 0.85f));
                _rowBGs[i] = rowGO.GetComponent<Image>();

                _nameTexts[i] = UIHelper.MakeText(ct, $"Name_{i}", new Vector2(0.24f, y), "", 27, UIHelper.TextWhite, TextAnchor.MiddleLeft, 460, 74);
                _levelTexts[i] = UIHelper.MakeText(ct, $"Level_{i}", new Vector2(0.63f, y), "", 26, UIHelper.TextWhite, TextAnchor.MiddleCenter, 180, 50);
                _costTexts[i] = UIHelper.MakeText(ct, $"Cost_{i}", new Vector2(0.84f, y), "", 30, UIHelper.AccentGreen, TextAnchor.MiddleCenter, 140, 50);
            }

            UIHelper.MakeDivider(ct, "Div2", 0.24f);
            UIHelper.MakeText(ct, "Hint", new Vector2(0.5f, 0.20f), "Tap a row to purchase · Tap PLAY AGAIN to continue", 20, UIHelper.TextDim);

            UIHelper.MakeButton(ct, "Retry", new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.12f),
                "PLAY AGAIN", 40, new Color(0.08f, 0.28f, 0.2f, 0.96f), UIHelper.AccentGreen);
            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }
    }
}
