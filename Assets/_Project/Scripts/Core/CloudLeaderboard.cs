using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace RuneDrop.Core
{
    /// <summary>
    /// Cloud leaderboard via Supabase REST API.
    /// Submits scores, fetches top 20 all-time and weekly leaderboard,
    /// and fetches current weekly winner.
    /// </summary>
    public class CloudLeaderboard : MonoBehaviour
    {
        public static CloudLeaderboard Instance { get; private set; }

        // ── Cached Data ─────────────────────────────────────────────
        public List<LeaderboardEntry> TopAllTime { get; private set; } = new();
        public List<LeaderboardEntry> TopThisWeek { get; private set; } = new();
        public WeeklyWinnerData CurrentWinner { get; private set; }
        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            StartCoroutine(FetchAll());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<CloudLeaderboard>();
                Instance = null;
            }
        }

        // ── Submit Score ────────────────────────────────────────────

        public void SubmitScore(string playerName, float depth, int runes, int combos, float duration)
        {
            StartCoroutine(PostScore(playerName, depth, runes, combos, duration));
        }

        private IEnumerator PostScore(string playerName, float depth, int runes, int combos, float duration)
        {
            var now = DateTime.UtcNow;
            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = now.Year;

            string json = $"{{\"player_name\":\"{EscapeJson(playerName)}\",\"depth\":{depth:F1},\"runes_collected\":{runes},\"combos_activated\":{combos},\"run_duration\":{duration:F1},\"device_id\":\"{SystemInfo.deviceUniqueIdentifier}\",\"week_number\":{week},\"year\":{year}}}";

            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}";
            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log($"[CloudLeaderboard] Score submitted: {playerName} {depth:F0}m");
            else
                Debug.LogWarning($"[CloudLeaderboard] Submit failed: {request.error}");

            request.Dispose();

            // Refresh after submit
            StartCoroutine(FetchAll());
        }

        // ── Fetch Leaderboards ──────────────────────────────────────

        public IEnumerator FetchAll()
        {
            yield return FetchTopAllTime();
            yield return FetchTopThisWeek();
            yield return FetchWeeklyWinner();
            IsLoaded = true;
        }

        private IEnumerator FetchTopAllTime()
        {
            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}?select=player_name,depth,runes_collected,created_at&order=depth.desc&limit=20";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TopAllTime = ParseEntries(request.downloadHandler.text);
            }
            request.Dispose();
        }

        private IEnumerator FetchTopThisWeek()
        {
            var now = DateTime.UtcNow;
            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = now.Year;

            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}?select=player_name,depth,runes_collected,created_at&week_number=eq.{week}&year=eq.{year}&order=depth.desc&limit=20";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TopThisWeek = ParseEntries(request.downloadHandler.text);
            }
            request.Dispose();
        }

        private IEnumerator FetchWeeklyWinner()
        {
            // Get last week's winner
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                lastWeek, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = lastWeek.Year;

            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.WinnersTable}?week_number=eq.{week}&year=eq.{year}&limit=1";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var text = request.downloadHandler.text;
                if (text.Length > 5) // Not empty array
                {
                    CurrentWinner = ParseWinner(text);
                }
            }
            request.Dispose();
        }

        // ── Parsing ─────────────────────────────────────────────────

        private List<LeaderboardEntry> ParseEntries(string json)
        {
            var list = new List<LeaderboardEntry>();
            // Simple JSON array parsing without external library
            try
            {
                var wrapper = JsonUtility.FromJson<ScoreArrayWrapper>("{\"items\":" + json + "}");
                if (wrapper?.items != null)
                {
                    foreach (var item in wrapper.items)
                    {
                        list.Add(new LeaderboardEntry
                        {
                            Depth = item.depth,
                            RunesCollected = item.runes_collected,
                            Date = item.created_at?.Substring(0, 10) ?? ""
                        });
                        // Store player name (LeaderboardEntry doesn't have it, use Date field hack)
                        if (list.Count > 0)
                        {
                            var entry = list[list.Count - 1];
                            entry.Date = item.player_name + "|" + entry.Date;
                            list[list.Count - 1] = entry;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CloudLeaderboard] Parse error: {e.Message}");
            }
            return list;
        }

        private WeeklyWinnerData ParseWinner(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<WinnerArrayWrapper>("{\"items\":" + json + "}");
                if (wrapper?.items != null && wrapper.items.Length > 0)
                {
                    var w = wrapper.items[0];
                    return new WeeklyWinnerData
                    {
                        PlayerName = w.player_name,
                        BestDepth = w.best_depth,
                        WeekNumber = w.week_number,
                        Year = w.year
                    };
                }
            }
            catch { }
            return null;
        }

        private string EscapeJson(string s) => s?.Replace("\"", "\\\"").Replace("\n", "") ?? "";

        // ── Data Classes ────────────────────────────────────────────

        [Serializable] private class ScoreItem
        {
            public string player_name;
            public float depth;
            public int runes_collected;
            public string created_at;
        }

        [Serializable] private class ScoreArrayWrapper { public ScoreItem[] items; }

        [Serializable] private class WinnerItem
        {
            public string player_name;
            public float best_depth;
            public int week_number;
            public int year;
        }

        [Serializable] private class WinnerArrayWrapper { public WinnerItem[] items; }
    }

    public class WeeklyWinnerData
    {
        public string PlayerName;
        public float BestDepth;
        public int WeekNumber;
        public int Year;
    }
}
