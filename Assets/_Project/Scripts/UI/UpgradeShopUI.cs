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

            // Back/Retry button (bottom)
            if (ny < 0.13f)
            {
                Close();
                if (GameManager.Instance != null)
                    GameManager.Instance.StartRun();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
                return;
            }

            // Upgrade rows — each row is ~0.095 tall, starting from y=0.72 (row 0) down to y=0.34 (row 4)
            // Row centers: 0.72, 0.625, 0.53, 0.435, 0.34
            // Screen Y is bottom-up, so high ny = top rows
            for (int i = 0; i < 5; i++)
            {
                float rowCenter = 0.72f - (i * 0.095f);
                if (ny > rowCenter - 0.05f && ny < rowCenter + 0.05f)
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
                RefreshUI();
        }

        private void RefreshUI()
        {
            var meta = MetaProgressionManager.Instance;
            if (meta == null) return;

            SaveData save = null;
            if (ServiceLocator.TryGet<SaveSystem>(out var sys)) save = sys.Data;

            _shardsText.text = $"Soul Shards: {save?.SoulShards ?? 0}";

            for (int i = 0; i < MetaProgressionManager.Upgrades.Length; i++)
            {
                var def = MetaProgressionManager.Upgrades[i];
                int level = meta.GetUpgradeLevel(def.Id);
                bool maxed = meta.IsMaxLevel(def.Id);
                int cost = meta.GetUpgradeCost(def.Id);
                bool canAfford = save != null && save.SoulShards >= cost;

                _nameTexts[i].text = $"{def.Name}\n<size=18>{def.Description}</size>";
                string levelBar = "";
                for (int l = 0; l < def.MaxLevel; l++)
                    levelBar += l < level ? "[X]" : "[ ]";
                _levelTexts[i].text = maxed ? "MAX" : levelBar;
                _levelTexts[i].color = maxed ? UIHelper.AccentGold : UIHelper.TextWhite;

                if (maxed)
                {
                    _costTexts[i].text = "MAXED";
                    _costTexts[i].color = UIHelper.TextMuted;
                    _rowBGs[i].color = new Color(0.05f, 0.04f, 0.08f, 0.8f);
                }
                else
                {
                    _costTexts[i].text = $"{cost}";
                    _costTexts[i].color = canAfford ? UIHelper.AccentGreen : UIHelper.AccentRed;
                    _rowBGs[i].color = canAfford ?
                        new Color(0.08f, 0.15f, 0.08f, 0.8f) :
                        new Color(0.1f, 0.06f, 0.15f, 0.8f);
                }
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "ShopCanvas", 300);
            _panel = canvas.gameObject;
            var ct = canvas.transform;

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);

            UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.90f),
                "UPGRADES", 56, UIHelper.AccentPurple);

            _shardsText = UIHelper.MakeText(ct, "Shards", new Vector2(0.5f, 0.84f),
                "Soul Shards: 0", 36, new Color(0.8f, 0.6f, 1f));

            UIHelper.MakeDivider(ct, "Div1", 0.80f);

            // Column headers
            UIHelper.MakeText(ct, "ColName", new Vector2(0.25f, 0.77f),
                "Upgrade", 24, UIHelper.TextDim, TextAnchor.MiddleLeft, 300, 40);
            UIHelper.MakeText(ct, "ColLevel", new Vector2(0.6f, 0.77f),
                "Level", 24, UIHelper.TextDim, TextAnchor.MiddleCenter, 150, 40);
            UIHelper.MakeText(ct, "ColCost", new Vector2(0.85f, 0.77f),
                "Cost", 24, UIHelper.TextDim, TextAnchor.MiddleCenter, 150, 40);

            _nameTexts = new Text[5];
            _costTexts = new Text[5];
            _levelTexts = new Text[5];
            _rowBGs = new Image[5];

            for (int i = 0; i < 5; i++)
            {
                float y = 0.72f - (i * 0.095f);
                var def = MetaProgressionManager.Upgrades[i];

                // Row background
                var rowGO = UIHelper.MakePanel(ct, $"Row_{i}",
                    new Vector2(0.05f, y - 0.035f), new Vector2(0.95f, y + 0.035f),
                    UIHelper.BgPanel);
                _rowBGs[i] = rowGO.GetComponent<Image>();

                // Name only (description shown in refresh via _nameTexts)
                _nameTexts[i] = UIHelper.MakeText(ct, $"Name_{i}",
                    new Vector2(0.25f, y), def.Name, 26, UIHelper.TextWhite,
                    TextAnchor.MiddleLeft, 350, 60);

                // Level
                _levelTexts[i] = UIHelper.MakeText(ct, $"Level_{i}",
                    new Vector2(0.6f, y), "Lv 0", 28, UIHelper.TextWhite,
                    TextAnchor.MiddleCenter, 150, 40);

                // Cost
                _costTexts[i] = UIHelper.MakeText(ct, $"Cost_{i}",
                    new Vector2(0.85f, y), "0", 28, UIHelper.AccentGreen,
                    TextAnchor.MiddleCenter, 150, 40);
            }

            UIHelper.MakeDivider(ct, "Div2", 0.24f);

            UIHelper.MakeText(ct, "Hint", new Vector2(0.5f, 0.20f),
                "Tap an upgrade to buy it", 24, UIHelper.TextDim);

            UIHelper.MakeButton(ct, "Retry",
                new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.12f),
                "PLAY AGAIN", 40, new Color(0.1f, 0.3f, 0.1f, 0.9f), UIHelper.AccentGreen);
        }
    }
}
