using UnityEngine;

namespace RuneDrop.Data
{
    /// <summary>
    /// Central game configuration. Single source of truth for all tuning parameters.
    /// Create via Assets > Create > RuneDrop > Game Config.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "RuneDrop/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        // ── Player ──────────────────────────────────────────────────

        [Header("Player")]
        [Tooltip("Starting fall speed in units/second")]
        public float BaseFallSpeed = 4f;

        [Tooltip("Maximum fall speed cap")]
        public float MaxFallSpeed = 12f;

        [Tooltip("Fall speed increase per second")]
        public float FallAcceleration = 0.1f;

        [Tooltip("Horizontal movement speed")]
        public float HorizontalMoveSpeed = 8f;

        [Tooltip("Horizontal smoothing time")]
        public float HorizontalSmoothing = 0.1f;

        [Tooltip("Player collider radius")]
        public float PlayerColliderRadius = 0.3f;

        // ── Anchor ──────────────────────────────────────────────────

        [Header("Anchor")]
        [Tooltip("Starting anchor charges per run (before upgrades)")]
        public int BaseAnchorCharges = 2;

        [Tooltip("How long anchor slows fall (seconds)")]
        public float AnchorDuration = 1.5f;

        [Tooltip("Fall speed multiplier while anchored (0.2 = 20% speed)")]
        public float AnchorFallSpeedMultiplier = 0.2f;

        [Tooltip("Cooldown between anchor uses (seconds)")]
        public float AnchorCooldown = 0.5f;

        // ── Runes ───────────────────────────────────────────────────

        [Header("Runes")]
        [Tooltip("Base time between rune spawns (seconds)")]
        public float RuneSpawnInterval = 1.5f;

        [Tooltip("Base probability a spawn point will have a rune")]
        public float RuneBaseDropChance = 0.6f;

        [Tooltip("Duration of rune combo effects (seconds)")]
        public float ComboDuration = 8f;

        [Tooltip("Extra combo duration per upgrade level")]
        public float ComboExtensionPerUpgrade = 2f;

        [Tooltip("Duration of single rune powers (seconds)")]
        public float RunePowerDuration = 5f;

        // ── Level Generation ────────────────────────────────────────

        [Header("Level Generation")]
        [Tooltip("Height of each procedural chunk")]
        public float ChunkHeight = 20f;

        [Tooltip("How many chunks to keep loaded ahead of player")]
        public int ChunksAhead = 3;

        [Tooltip("How many chunks to keep loaded behind player")]
        public int ChunksBehind = 1;

        [Tooltip("Base obstacle density (0-1)")]
        public float ObstacleDensityBase = 0.3f;

        [Tooltip("Obstacle density increase per 100m depth")]
        public float ObstacleDensityScaling = 0.02f;

        [Tooltip("Seconds between decision rooms")]
        public float DecisionRoomInterval = 12f;

        // ── Camera ──────────────────────────────────────────────────

        [Header("Camera")]
        [Tooltip("Camera follow lerp speed")]
        public float CameraFollowSpeed = 5f;

        [Tooltip("Camera look-ahead distance below player")]
        public float CameraLookAhead = 2f;

        // ── Meta Progression ────────────────────────────────────────

        [Header("Meta Progression")]
        [Tooltip("Base soul shards awarded per run")]
        public int SoulShardsPerRun = 10;

        [Tooltip("Extra soul shards per 100m depth")]
        public float SoulShardDepthBonus = 0.5f;

        [Tooltip("Soul shards per rune collected")]
        public float SoulShardPerRune = 0.5f;

        [Tooltip("Max rewarded ad revives per run")]
        public int ReviveAdMaxPerRun = 1;

        // ── Ads ─────────────────────────────────────────────────────

        [Header("Ads")]
        [Tooltip("Minimum time between rewarded ads")]
        public float RewardedAdCooldown = 30f;

        // ── Upgrade Scaling ─────────────────────────────────────────

        [Header("Upgrade Scaling")]
        [Tooltip("Fall speed reduction per SlowFall upgrade level (multiplied)")]
        public float SlowFallReductionPerLevel = 0.1f;

        [Tooltip("Rune spawn rate bonus per RuneSpawn upgrade level")]
        public float RuneSpawnBonusPerLevel = 0.15f;
    }
}
