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

            if (ny < 0.15f) { Close(); return; }

            // SFX slider (y: 0.52-0.60)
            if (ny > 0.50f && ny < 0.62f && nx > 0.1f && nx < 0.9f)
            {
                AudioManager.Instance?.SetSFXVolume((nx - 0.1f) / 0.8f);
                RefreshSliders();
            }

            // Music slider (y: 0.34-0.42)
            if (ny > 0.32f && ny < 0.44f && nx > 0.1f && nx < 0.9f)
            {
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
            _sfxLabel.text = $"SFX Volume: {Mathf.RoundToInt(sfx * 100)}%";
            _musicLabel.text = $"Music Volume: {Mathf.RoundToInt(music * 100)}%";
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "SettingsCanvas", 400);
            _panel = canvas.gameObject;
            var ct = canvas.transform;

            UIHelper.MakePanel(ct, "BG", Vector2.zero, Vector2.one, UIHelper.BgDark);

            UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.85f),
                "SETTINGS", 56, UIHelper.AccentPurple);

            UIHelper.MakeDivider(ct, "Div1", 0.80f);

            // SFX
            _sfxLabel = UIHelper.MakeText(ct, "SFXLabel", new Vector2(0.5f, 0.65f),
                "SFX Volume: 100%", 32, UIHelper.TextWhite);
            CreateSlider(ct, "SFX", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.59f), out _sfxFill);

            // Music
            _musicLabel = UIHelper.MakeText(ct, "MusicLabel", new Vector2(0.5f, 0.47f),
                "Music Volume: 70%", 32, UIHelper.TextWhite);
            CreateSlider(ct, "Music", new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.41f), out _musicFill);

            UIHelper.MakeDivider(ct, "Div2", 0.25f);

            UIHelper.MakeButton(ct, "Back",
                new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.14f),
                "BACK", 38, UIHelper.BgButton, UIHelper.AccentCyan);
        }

        private void CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, out Image fill)
        {
            UIHelper.MakePanel(parent, name + "BG", anchorMin, anchorMax,
                new Color(0.15f, 0.12f, 0.2f));

            var fillGO = new GameObject(name + "Fill");
            fillGO.transform.SetParent(parent.GetChild(parent.childCount - 1), false);
            var rect = fillGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            fill = fillGO.AddComponent<Image>();
            fill.color = UIHelper.AccentPurple;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
        }
    }
}
