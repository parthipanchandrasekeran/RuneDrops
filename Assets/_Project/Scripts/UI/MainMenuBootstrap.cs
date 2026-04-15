using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.UI
{
    /// <summary>
    /// Bootstrap for MainMenu scene. Creates all managers and UI.
    /// Shows name input on first launch, then main menu with weekly banner.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // ── Core Services ───────────────────────────────────────
            if (!ServiceLocator.TryGet<SaveSystem>(out _))
            {
                var saveGO = new GameObject("SaveSystem");
                saveGO.AddComponent<SaveSystem>();
            }

            if (GameManager.Instance == null)
            {
                var gmGO = new GameObject("GameManager");
                gmGO.AddComponent<GameManager>();
            }

            // Cloud leaderboard
            if (CloudLeaderboard.Instance == null)
            {
                var lbGO = new GameObject("CloudLeaderboard");
                lbGO.AddComponent<CloudLeaderboard>();
            }
        }

        private void Start()
        {
            // Check if player has set a name
            string playerName = PlayerPrefs.GetString("PlayerName", "");

            if (string.IsNullOrWhiteSpace(playerName))
            {
                // First launch — show name input
                var nameGO = new GameObject("NameInput");
                var nameUI = nameGO.AddComponent<NameInputUI>();
                nameUI.Show(OnNameSet);
            }
            else
            {
                ShowMainMenu();
            }
        }

        private void OnNameSet(string name)
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            // Main menu UI
            var menuGO = new GameObject("MainMenuUI");
            menuGO.AddComponent<MainMenuUI>();

            // Settings (hidden by default)
            var settingsGO = new GameObject("SettingsScreenUI");
            settingsGO.AddComponent<SettingsScreenUI>();

            // Leaderboard (hidden by default)
            var leaderboardGO = new GameObject("LeaderboardScreenUI");
            leaderboardGO.AddComponent<LeaderboardScreenUI>();

            // Powers reference
            var powersGO = new GameObject("PowersReferenceUI");
            powersGO.AddComponent<PowersReferenceUI>();

            // Mode select
            var modeGO = new GameObject("ModeSelectUI");
            modeGO.AddComponent<ModeSelectUI>();

            // Back button handler
            var backGO = new GameObject("BackButtonHandler");
            backGO.AddComponent<BackButtonHandler>();

            // ScreenFlipper removed — orientation handled by OS
        }
    }
}
