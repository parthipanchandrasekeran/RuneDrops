using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;

namespace RuneDrop.Progression
{
    /// <summary>
    /// Manages soul shard rewards and permanent upgrade purchasing.
    /// Subscribes to PlayerDiedEvent to award run rewards.
    /// </summary>
    public class MetaProgressionManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static MetaProgressionManager Instance { get; private set; }

        // ── Config ──────────────────────────────────────────────────
        private GameConfigSO _config;

        // ── Last Run Summary ────────────────────────────────────────
        public RunSummary LastRunSummary { get; private set; }

        // ── Upgrade Definitions ─────────────────────────────────────
        public static readonly UpgradeDefinition[] Upgrades = new[]
        {
            new UpgradeDefinition
            {
                Id = "anchor_charges",
                Name = "+1 Anchor Charge",
                Description = "Extra anchor charge per run",
                MaxLevel = 3,
                Costs = new[] { 50, 150, 400 }
            },
            new UpgradeDefinition
            {
                Id = "slow_fall",
                Name = "Slower Fall",
                Description = "-10% fall speed per level",
                MaxLevel = 5,
                Costs = new[] { 30, 80, 200, 500, 1000 }
            },
            new UpgradeDefinition
            {
                Id = "rune_spawn",
                Name = "Rune Magnet",
                Description = "+15% rune spawn rate",
                MaxLevel = 5,
                Costs = new[] { 40, 100, 250, 600, 1200 }
            },
            new UpgradeDefinition
            {
                Id = "start_shield",
                Name = "Start Shield",
                Description = "Begin runs with Earth shield",
                MaxLevel = 1,
                Costs = new[] { 200 }
            },
            new UpgradeDefinition
            {
                Id = "combo_extend",
                Name = "Combo Duration",
                Description = "+2s combo duration",
                MaxLevel = 3,
                Costs = new[] { 60, 180, 500 }
            }
        };

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _config = Resources.Load<GameConfigSO>("Configs/GameConfig");
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            if (Instance == this)
            {
                ServiceLocator.Unregister<MetaProgressionManager>();
                Instance = null;
            }
        }

        // ── Run Reward ──────────────────────────────────────────────

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            AwardRunReward(evt.DepthReached, evt.RunesCollected, evt.RunDuration);
        }

        public RunSummary AwardRunReward(float depth, int runesCollected, float duration)
        {
            int shards = CalculateRunReward(depth, runesCollected);

            if (!ServiceLocator.TryGet<SaveSystem>(out var save)) return null;

            bool isNewBest = depth > save.Data.BestDepth;

            int oldShards = save.Data.SoulShards;
            save.Data.SoulShards += shards;
            save.Save();

            EventBus.Publish(new SoulShardsChangedEvent
            {
                OldAmount = oldShards,
                NewAmount = save.Data.SoulShards,
                Delta = shards
            });

            LastRunSummary = new RunSummary
            {
                DepthReached = depth,
                RunesCollected = runesCollected,
                RunDuration = duration,
                SoulShardsEarned = shards,
                IsNewBestDepth = isNewBest,
                ComboActivations = GameManager.Instance?.CurrentRunCombos ?? 0,
                DecisionRoomsSurvived = GameManager.Instance?.CurrentRunDecisionRooms ?? 0
            };

            // Record to leaderboard
            save.AddLeaderboardEntry(depth, runesCollected);

            // Submit to cloud leaderboard
            string playerName = UnityEngine.PlayerPrefs.GetString("PlayerName", "Anonymous");
            var cloud = CloudLeaderboard.Instance;
            if (cloud != null)
            {
                cloud.SubmitScore(playerName, depth, runesCollected,
                    GameManager.Instance?.CurrentRunCombos ?? 0, duration);
            }

            Debug.Log($"[Meta] Run reward: {shards} soul shards (depth: {depth:F0}m, runes: {runesCollected})");
            return LastRunSummary;
        }

        public int CalculateRunReward(float depth, int runesCollected)
        {
            float shards = _config.SoulShardsPerRun;
            shards += (depth / 100f) * _config.SoulShardDepthBonus * 100f;
            shards += runesCollected * _config.SoulShardPerRune;
            return Mathf.RoundToInt(shards);
        }

        // ── Upgrades ────────────────────────────────────────────────

        public int GetUpgradeLevel(string upgradeId)
        {
            if (!ServiceLocator.TryGet<SaveSystem>(out var save)) return 0;

            return upgradeId switch
            {
                "anchor_charges" => save.Data.UpgradeAnchorCharges,
                "slow_fall" => save.Data.UpgradeSlowFall,
                "rune_spawn" => save.Data.UpgradeRuneSpawn,
                "start_shield" => save.Data.UpgradeStartShield,
                "combo_extend" => save.Data.UpgradeComboExtend,
                _ => 0
            };
        }

        public bool TryPurchaseUpgrade(string upgradeId)
        {
            if (!ServiceLocator.TryGet<SaveSystem>(out var save)) return false;

            var def = GetUpgradeDefinition(upgradeId);
            if (def == null) return false;

            int currentLevel = GetUpgradeLevel(upgradeId);
            if (currentLevel >= def.MaxLevel) return false;

            int cost = def.Costs[currentLevel];
            if (save.Data.SoulShards < cost) return false;

            // Deduct and apply
            int oldShards = save.Data.SoulShards;
            save.Data.SoulShards -= cost;

            SetUpgradeLevel(save, upgradeId, currentLevel + 1);
            save.Save();

            EventBus.Publish(new SoulShardsChangedEvent
            {
                OldAmount = oldShards,
                NewAmount = save.Data.SoulShards,
                Delta = -cost
            });

            EventBus.Publish(new UpgradePurchasedEvent
            {
                UpgradeId = upgradeId,
                NewLevel = currentLevel + 1
            });

            Debug.Log($"[Meta] Purchased {upgradeId} level {currentLevel + 1} for {cost} shards");
            return true;
        }

        public int GetUpgradeCost(string upgradeId)
        {
            var def = GetUpgradeDefinition(upgradeId);
            if (def == null) return 0;
            int level = GetUpgradeLevel(upgradeId);
            if (level >= def.MaxLevel) return 0;
            return def.Costs[level];
        }

        public bool IsMaxLevel(string upgradeId)
        {
            var def = GetUpgradeDefinition(upgradeId);
            if (def == null) return true;
            return GetUpgradeLevel(upgradeId) >= def.MaxLevel;
        }

        // ── Helpers ─────────────────────────────────────────────────

        private UpgradeDefinition GetUpgradeDefinition(string id)
        {
            foreach (var def in Upgrades)
            {
                if (def.Id == id) return def;
            }
            return null;
        }

        private void SetUpgradeLevel(SaveSystem save, string upgradeId, int level)
        {
            switch (upgradeId)
            {
                case "anchor_charges": save.Data.UpgradeAnchorCharges = level; break;
                case "slow_fall": save.Data.UpgradeSlowFall = level; break;
                case "rune_spawn": save.Data.UpgradeRuneSpawn = level; break;
                case "start_shield": save.Data.UpgradeStartShield = level; break;
                case "combo_extend": save.Data.UpgradeComboExtend = level; break;
            }
        }
    }

    // ── Upgrade Definition ──────────────────────────────────────────

    public class UpgradeDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public int MaxLevel;
        public int[] Costs;
    }
}
