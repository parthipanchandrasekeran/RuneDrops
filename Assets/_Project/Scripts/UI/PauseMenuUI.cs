using UnityEngine;
using UnityEngine.UI;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject _panel;
        private bool _isOpen;

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

        private float _openCooldown;

        private void Update()
        {
            if (!_isOpen) return;

            // Ignore input for 0.3s after opening to prevent instant-resume
            if (_openCooldown > 0f)
            {
                _openCooldown -= Time.unscaledDeltaTime;
                return;
            }

            Vector2 tapPos;
            if (!UIHelper.GetTap(out tapPos)) return;

            float ny = tapPos.y / Screen.height;

            if (ny > 0.45f)
            {
                Time.timeScale = 1f;
                var gm = GameManager.Instance;
                if (gm != null) gm.ResumeGame(); else Close();
            }
            else if (ny < 0.35f)
            {
                Time.timeScale = 1f;
                Close();
                if (GameManager.Instance != null)
                    GameManager.Instance.ReturnToMainMenu();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        private void CreateUI()
        {
            var canvas = UIHelper.CreateCanvas(transform, "PauseCanvas", 250);
            _panel = canvas.gameObject;
            var ct = canvas.transform;

            UIHelper.MakePanel(ct, "Overlay", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.8f));

            UIHelper.MakeText(ct, "Title", new Vector2(0.5f, 0.65f),
                "PAUSED", 72, UIHelper.AccentPurple);

            UIHelper.MakeButton(ct, "Resume",
                new Vector2(0.2f, 0.48f), new Vector2(0.8f, 0.57f),
                "RESUME", 44, new Color(0.1f, 0.3f, 0.1f, 0.9f), UIHelper.AccentGreen);

            UIHelper.MakeButton(ct, "Quit",
                new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.37f),
                "QUIT TO MENU", 38, new Color(0.3f, 0.1f, 0.1f, 0.9f), UIHelper.AccentRed);
        }
    }
}
