using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    public class SettingsScreenUI : MonoBehaviour
    {
        private GameObject _panel;
        private Image _sfxFill;
        private Image _musicFill;
        private Text _sfxLabel;
        private Text _musicLabel;
        private System.Action _onClose;
        private bool _isOpen;

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
        }

        public void Open(System.Action onClose)
        {
            _onClose = onClose;
            _isOpen = true;
            _panel.SetActive(true);
            RefreshSliders();
        }

        public void Close()
        {
            _isOpen = false;
            _panel.SetActive(false);
            if (ServiceLocator.TryGet<SaveSystem>(out var save)) save.Save();
            _onClose?.Invoke();
        }

        private void Update()
        {
            if (!_isOpen) return;

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float nx = tapPos.x / Screen.width;
            float ny = tapPos.y / Screen.height;

            if (ny < 0.15f) { UIHelper.LightHaptic(); Close(); return; }

            if (ny > 0.50f && ny < 0.62f && nx > 0.1f && nx < 0.9f)
            {
                UIHelper.LightHaptic();
                AudioManager.Instance?.SetSFXVolume((nx - 0.1f) / 0.8f);
                RefreshSliders();
            }

            if (ny > 0.32f && ny < 0.44f && nx > 0.1f && nx < 0.9f)
            {
                UIHelper.LightHaptic();
                AudioManager.Instance?.SetMusicVolume((nx - 0.1f) / 0.8f);
                RefreshSliders();
            }
        }

        private void RefreshSliders()
        {
            float sfx = AudioManager.Instance?.SFXVolume ?? 1f;
            float music = AudioManager.Instance?.MusicVolume ?? 0.7f;
            _sfxFill.fillAmount = sfx;
            _musicFill.fillAmount = music;
            _sfxLabel.text = $"SFX: {Mathf.RoundToInt(sfx * 100)}%";
            _musicLabel.text = $"MUSIC: {Mathf.RoundToInt(music * 100)}%";
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "SettingsCanvas", 400);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);
            UIHelper.MakeCard(ct, "SettingsCard", new Vector2(0.05f, 0.16f), new Vector2(0.95f, 0.9f),
                new Color(0.07f, 0.1f, 0.18f, 0.96f), new Color(0.4f, 0.68f, 0.95f, 0.3f));

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.84f), "SETTINGS", 56, UIHelper.AccentCyan);
            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.80f), "Tune your ritual.", 20, UIHelper.TextDim);
            UIHelper.MakeDivider(ct, "Div1", 0.76f);

            _sfxLabel = UIHelper.MakeText(ct, "SFXLabel", new Vector2(0.5f, 0.65f), "SFX: 100%", 32, UIHelper.TextWhite);
            CreateSlider(ct, "SFX", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.60f), UIHelper.AccentCyan, out _sfxFill);

            _musicLabel = UIHelper.MakeText(ct, "MusicLabel", new Vector2(0.5f, 0.47f), "MUSIC: 70%", 32, UIHelper.TextWhite);
            CreateSlider(ct, "Music", new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.42f), UIHelper.AccentPurple, out _musicFill);

            UIHelper.MakeButton(ct, "Back", new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.13f),
                "BACK", 38, new Color(0.11f, 0.18f, 0.28f, 0.96f), UIHelper.AccentCyan);
            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }

        private void CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color fillColor, out Image fill)
        {
            var bg = UIHelper.MakePanel(parent, name + "BG", anchorMin, anchorMax, new Color(0.08f, 0.12f, 0.2f, 0.9f));
            UIHelper.MakePanel(parent, name + "Edge",
                new Vector2(anchorMin.x, anchorMax.y - 0.003f), new Vector2(anchorMax.x, anchorMax.y),
                new Color(fillColor.r, fillColor.g, fillColor.b, 0.45f));

            var fillGO = new GameObject(name + "Fill");
            fillGO.transform.SetParent(bg.transform, false);
            var rect = fillGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            fill = fillGO.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
        }
    }
}
