using System;
using System.IO;
using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// JSON-based persistence to Application.persistentDataPath.
    /// Registers with ServiceLocator on Awake.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        // ── Constants ───────────────────────────────────────────────
        private const string SAVE_FILE = "runedrop_save.json";

        // ── Data ────────────────────────────────────────────────────
        public SaveData Data { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            ServiceLocator.Register(this);
            Load();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        // ── Save / Load ─────────────────────────────────────────────

        public void Save()
        {
            try
            {
                Data.LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e}");
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var json = File.ReadAllText(SavePath);
                    Data = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log($"[SaveSystem] Loaded save (runs: {Data.TotalRuns}, best: {Data.BestDepth:F0}m)");
                }
                else
                {
                    Data = SaveData.CreateNew();
                    Debug.Log("[SaveSystem] No save found, created new.");
                    Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e}");
                Data = SaveData.CreateNew();
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                Data = SaveData.CreateNew();
                Debug.Log("[SaveSystem] Save deleted.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Delete failed: {e}");
            }
        }

        // ── Leaderboard ─────────────────────────────────────────────

        public void AddLeaderboardEntry(float depth, int runes)
        {
            var entries = GetLeaderboardEntries();
            entries.Add(new LeaderboardEntry
            {
                Depth = depth,
                RunesCollected = runes,
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            });
            entries.Sort((a, b) => b.Depth.CompareTo(a.Depth));
            if (entries.Count > 10) entries.RemoveRange(10, entries.Count - 10);

            var wrapper = new LeaderboardData { Entries = entries };
            Data.LeaderboardJson = JsonUtility.ToJson(wrapper);
            Save();
        }

        public System.Collections.Generic.List<LeaderboardEntry> GetLeaderboardEntries()
        {
            if (string.IsNullOrEmpty(Data.LeaderboardJson))
                return new System.Collections.Generic.List<LeaderboardEntry>();

            try
            {
                var wrapper = JsonUtility.FromJson<LeaderboardData>(Data.LeaderboardJson);
                return wrapper?.Entries ?? new System.Collections.Generic.List<LeaderboardEntry>();
            }
            catch
            {
                return new System.Collections.Generic.List<LeaderboardEntry>();
            }
        }
    }

    // ── Save Data ───────────────────────────────────────────────────

    [Serializable]
    public class SaveData
    {
        // ── Timestamps ──────────────────────────────────────────────
        public long LastSaveTimestamp;

        // ── Run Stats ───────────────────────────────────────────────
        public int TotalRuns;
        public float BestDepth;
        public int TotalRunesCollected;

        // ── Currency ────────────────────────────────────────────────
        public int SoulShards;

        // ── Permanent Upgrades (0 = not purchased, 1+ = level) ──────
        public int UpgradeAnchorCharges;
        public int UpgradeSlowFall;
        public int UpgradeRuneSpawn;
        public int UpgradeStartShield;
        public int UpgradeComboExtend;

        // ── Settings ────────────────────────────────────────────────
        public bool AdsRemoved;
        public float SfxVolume;
        public float MusicVolume;

        // ── Tutorial ────────────────────────────────────────────────
        public bool HasCompletedTutorial;

        // ── Leaderboard ─────────────────────────────────────────────
        public string LeaderboardJson;

        // ── Factory ─────────────────────────────────────────────────
        public static SaveData CreateNew()
        {
            return new SaveData
            {
                LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TotalRuns = 0,
                BestDepth = 0f,
                TotalRunesCollected = 0,
                SoulShards = 0,
                UpgradeAnchorCharges = 0,
                UpgradeSlowFall = 0,
                UpgradeRuneSpawn = 0,
                UpgradeStartShield = 0,
                UpgradeComboExtend = 0,
                AdsRemoved = false,
                SfxVolume = 1f,
                MusicVolume = 0.7f
            };
        }
    }
}
