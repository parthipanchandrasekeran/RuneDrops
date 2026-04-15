using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneDrop.Core
{
    /// <summary>
    /// Handles Android hardware back button across all screens.
    /// </summary>
    public class BackButtonHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBack();
            }
        }

        private void HandleBack()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                // No GameManager — quit app
                Application.Quit();
                return;
            }

            switch (gm.CurrentState)
            {
                case GameState.Playing:
                    // Pause the game
                    gm.PauseGame();
                    break;

                case GameState.Paused:
                    // Resume
                    gm.ResumeGame();
                    break;

                case GameState.Dead:
                    // Return to main menu
                    gm.ReturnToMainMenu();
                    break;

                case GameState.MainMenu:
                    // Quit app
                    Application.Quit();
                    break;

                default:
                    gm.ReturnToMainMenu();
                    break;
            }
        }
    }
}
