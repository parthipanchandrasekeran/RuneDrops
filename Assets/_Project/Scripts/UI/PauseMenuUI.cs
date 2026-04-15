using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject _panel;
        private bool _isOpen;
        private float _openCooldown;

        private void Start()
        {
            CreateUI();
            _panel.SetActive(false);
            EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
        }

        private void OnStateChanged(GameStateChangedEvent evt)
        {
            if (evt.NewState == GameState.Paused) Open();
            else if (evt.OldState == GameState.Paused) Close();
        }

        public void Open() { _isOpen = true; _openCooldown = 0.4f; _panel.SetActive(true); }
        public void Close() { _isOpen = false; _panel.SetActive(false); }

        private void Update()
        {
            if (!_isOpen) return;
            if (_openCooldown > 0f)
            {
                _openCooldown -= Time.unscaledDeltaTime;
                return;
            }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;
            float ny = tapPos.y / Screen.height;

            if (ny > 0.44f)
            {
                UIHelper.LightHaptic();
                Time.timeScale = 1f;
                var gm = GameManager.Instance;
                if (gm != null) gm.ResumeGame(); else Close();
            }
            else if (ny < 0.34f)
            {
                UIHelper.LightHaptic();
                Time.timeScale = 1f;
                Close();
                if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
                else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "PauseCanvas", 250);
            _panel = canvas.gameObject;
            var ct = UIHelper.GetSafeAreaRoot(canvas);

            UIHelper.MakePanel(ct, "Overlay", Vector2.zero, Vector2.one, new Color(0.01f, 0.02f, 0.06f, 0.88f));
            UIHelper.MakeCard(ct, "PauseCard", new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.76f),
                new Color(0.07f, 0.1f, 0.17f, 0.95f), new Color(0.45f, 0.75f, 1f, 0.35f));

            UIHelper.MakeGlowText(ct, "Title", new Vector2(0.5f, 0.66f), "PAUSED", 70, UIHelper.AccentCyan);
            UIHelper.MakeText(ct, "Sub", new Vector2(0.5f, 0.61f), "Take a breath. Descend when ready.", 22, UIHelper.TextDim);

            UIHelper.MakeButton(ct, "Resume", new Vector2(0.2f, 0.46f), new Vector2(0.8f, 0.56f),
                "RESUME", 44, new Color(0.08f, 0.24f, 0.2f, 0.95f), UIHelper.AccentGreen);

            UIHelper.MakeButton(ct, "Quit", new Vector2(0.2f, 0.26f), new Vector2(0.8f, 0.36f),
                "QUIT TO MENU", 36, new Color(0.24f, 0.1f, 0.14f, 0.95f), UIHelper.AccentRed);
            UIFXAnimator.Attach(_panel, 0.2f, 0.985f);
        }
    }
}
