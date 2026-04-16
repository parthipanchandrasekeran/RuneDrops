using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Runes;
using RuneDrop.Anchor;
using RuneDrop.Decision;
using RuneDrop.Utils;
using RuneDrop.Monetization;

namespace RuneDrop.UI
{
    /// <summary>
    /// Bootstrap for Gameplay scene. Creates all UI and manager GameObjects.
    /// Replaces the need for manually adding components via MCP.
    /// Attach to a single GameObject in the Gameplay scene.
    /// </summary>
    public class GameplayBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // ── Clear stale static state from previous scene ────────
            EventBus.Clear();

            // ── Core Services ───────────────────────────────────────
            if (!ServiceLocator.TryGet<SaveSystem>(out _))
            {
                var saveGO = new GameObject("SaveSystem");
                saveGO.AddComponent<SaveSystem>();
            }

            // Cloud leaderboard
            if (CloudLeaderboard.Instance == null)
            {
                var lbGO = new GameObject("CloudLeaderboard");
                lbGO.AddComponent<CloudLeaderboard>();
            }

            // ── Game Managers ───────────────────────────────────────
            var runeInvGO = new GameObject("RuneInventory");
            runeInvGO.AddComponent<RuneInventory>();

            var anchorGO = new GameObject("AnchorController");
            anchorGO.AddComponent<AnchorController>();

            var runePowerGO = new GameObject("RunePowerManager");
            runePowerGO.AddComponent<RunePowerManager>();

            var decisionGO = new GameObject("DecisionRoomManager");
            decisionGO.AddComponent<DecisionRoomManager>();

            if (!ServiceLocator.TryGet<RuneDrop.Progression.MetaProgressionManager>(out _))
            {
                var metaGO = new GameObject("MetaProgression");
                metaGO.AddComponent<RuneDrop.Progression.MetaProgressionManager>();
            }

            // ── UI ──────────────────────────────────────────────────
            var hudGO = new GameObject("HUD");
            hudGO.AddComponent<GameplayHUD>();

            var deathGO = new GameObject("DeathScreen");
            deathGO.AddComponent<DeathScreenUI>();

            var shopGO = new GameObject("UpgradeShop");
            shopGO.AddComponent<UpgradeShopUI>();

            var pauseGO = new GameObject("PauseMenu");
            pauseGO.AddComponent<PauseMenuUI>();

            var tutorialGO = new GameObject("TutorialTrigger");
            tutorialGO.AddComponent<TutorialTrigger>();

            // ── Ads ─────────────────────────────────────────────────
            if (AdManager.Instance == null)
            {
                var adGO = new GameObject("AdManager");
                adGO.AddComponent<AdManager>();
            }
            AdManager.Instance?.OnRunStarted();

            // ── Feedback ─────────────────────────────────────────────
            var feedbackGO = new GameObject("RuneCollectFeedback");
            feedbackGO.AddComponent<RuneCollectFeedback>();

            // ── Falling Environment (visual speed cues) ────────────
            var envGO = new GameObject("FallingEnvironment");
            envGO.AddComponent<RuneDrop.Level.FallingEnvironment>();
            envGO.AddComponent<RuneDrop.Level.SpeedVisualizer>();

            // ── Sprite Particles ─────────────────────────────────────
            var particlesGO = new GameObject("SpriteParticles");
            particlesGO.AddComponent<RuneDrop.Utils.SpriteParticles>();

            // ── Player Visuals (glow + trail) ───────────────────────
            var player = RuneDrop.Player.PlayerController.Instance;
            if (player != null && player.GetComponent<RuneDrop.Player.PlayerVisuals>() == null)
                player.gameObject.AddComponent<RuneDrop.Player.PlayerVisuals>();

            // ── Audio ────────────────────────────────────────────────
            var audioGO = new GameObject("ProceduralAudio");
            audioGO.AddComponent<RuneDrop.Core.ProceduralAudio>();
            audioGO.AddComponent<RuneDrop.Core.ProceduralMusic>();

            // ── Polish ──────────────────────────────────────────────
            var juiceGO = new GameObject("GameJuice");
            juiceGO.AddComponent<GameJuice>();

            var screenGO = new GameObject("ScreenSetup");
            screenGO.AddComponent<ScreenSetup>();

            // Back button
            var backGO = new GameObject("BackButtonHandler");
            backGO.AddComponent<BackButtonHandler>();

            // ScreenFlipper removed — orientation handled by OS
        }
    }
}
