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
    /// With detailed logging to debug Android issues.
    /// </summary>
    public class CloudLeaderboard : MonoBehaviour
    {
        public static CloudLeaderboard Instance { get; private set; }

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
            if (Instance == this) { ServiceLocator.Unregister<CloudLeaderboard>(); Instance = null; }
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

            string safeName = playerName?.Replace("\"", "").Replace("\\", "") ?? "Anonymous";
            string json = $"{{\"player_name\":\"{safeName}\",\"depth\":{depth:F1},\"runes_collected\":{runes},\"combos_activated\":{combos},\"run_duration\":{duration:F1},\"device_id\":\"{SystemInfo.deviceUniqueIdentifier}\",\"week_number\":{week},\"year\":{year}}}";

            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}";
            Debug.Log($"[Cloud] Submitting score to: {url}");

            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");
            request.SetRequestHeader("Prefer", "return=minimal");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log($"[Cloud] Score submitted: {safeName} {depth:F0}m");
            else
                Debug.LogWarning($"[Cloud] Submit failed: {request.result} - {request.error} - {request.downloadHandler?.text}");

            request.Dispose();
            StartCoroutine(FetchAll());
        }

        // ── Fetch ───────────────────────────────────────────────────

        public IEnumerator FetchAll()
        {
            Debug.Log("[Cloud] Fetching leaderboard data...");
            yield return FetchTopAllTime();
            yield return FetchTopThisWeek();
            IsLoaded = true;
            Debug.Log($"[Cloud] Loaded: {TopAllTime.Count} all-time, {TopThisWeek.Count} weekly");
        }

        private IEnumerator FetchTopAllTime()
        {
            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}?select=player_name,depth,runes_collected,created_at&order=depth.desc&limit=20";
            yield return FetchEntries(url, (entries) => TopAllTime = entries, "AllTime");
        }

        private IEnumerator FetchTopThisWeek()
        {
            var now = DateTime.UtcNow;
            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = now.Year;

            var url = $"{SupabaseConfig.ProjectUrl}/rest/v1/{SupabaseConfig.ScoresTable}?select=player_name,depth,runes_collected,created_at&week_number=eq.{week}&year=eq.{year}&order=depth.desc&limit=20";
            yield return FetchEntries(url, (entries) => TopThisWeek = entries, "Weekly");
        }

        private IEnumerator FetchEntries(string url, System.Action<List<LeaderboardEntry>> onResult, string label)
        {
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", SupabaseConfig.AnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {SupabaseConfig.AnonKey}");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[Cloud] {label} response ({responseText.Length} chars): {responseText.Substring(0, Mathf.Min(200, responseText.Length))}");
                var entries = ParseEntries(responseText);
                onResult(entries);
                Debug.Log($"[Cloud] {label}: parsed {entries.Count} entries");
            }
            else
            {
                Debug.LogWarning($"[Cloud] {label} fetch failed: {request.result} - {request.error}");
            }
            request.Dispose();
        }

        // ── Parsing ─────────────────────────────────────────────────

        private List<LeaderboardEntry> ParseEntries(string json)
        {
            var list = new List<LeaderboardEntry>();
            if (string.IsNullOrEmpty(json) || json == "[]") return list;

            try
            {
                // JsonUtility can't parse arrays directly, wrap in object
                string wrapped = "{\"items\":" + json + "}";
                var wrapper = JsonUtility.FromJson<ScoreArrayWrapper>(wrapped);
                if (wrapper?.items != null)
                {
                    foreach (var item in wrapper.items)
                    {
                        string date = "";
                        if (!string.IsNullOrEmpty(item.created_at) && item.created_at.Length >= 10)
                            date = item.created_at.Substring(0, 10);

                        string name = string.IsNullOrEmpty(item.player_name) ? "???" : item.player_name;

                        list.Add(new LeaderboardEntry
                        {
                            Depth = item.depth,
                            RunesCollected = item.runes_collected,
                            Date = name + "|" + date
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Cloud] Parse error: {e.Message}");

                // Fallback: manual parsing for simple cases
                list = ManualParseEntries(json);
            }
            return list;
        }

        /// <summary>Simple manual JSON parser as fallback if JsonUtility fails.</summary>
        private List<LeaderboardEntry> ManualParseEntries(string json)
        {
            var list = new List<LeaderboardEntry>();
            try
            {
                // Find all "player_name":"X" patterns
                int pos = 0;
                while (pos < json.Length)
                {
                    int nameStart = json.IndexOf("\"player_name\":\"", pos);
                    if (nameStart < 0) break;
                    nameStart += 15;
                    int nameEnd = json.IndexOf("\"", nameStart);
                    string name = json.Substring(nameStart, nameEnd - nameStart);

                    int depthStart = json.IndexOf("\"depth\":", pos);
                    if (depthStart < 0) break;
                    depthStart += 8;
                    int depthEnd = depthStart;
                    while (depthEnd < json.Length && (char.IsDigit(json[depthEnd]) || json[depthEnd] == '.'))
                        depthEnd++;
                    float.TryParse(json.Substring(depthStart, depthEnd - depthStart),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out float depth);

                    int runesStart = json.IndexOf("\"runes_collected\":", pos);
                    int runesVal = 0;
                    if (runesStart >= 0)
                    {
                        runesStart += 18;
                        int runesEnd = runesStart;
                        while (runesEnd < json.Length && char.IsDigit(json[runesEnd]))
                            runesEnd++;
                        int.TryParse(json.Substring(runesStart, runesEnd - runesStart), out runesVal);
                    }

                    list.Add(new LeaderboardEntry
                    {
                        Depth = depth,
                        RunesCollected = runesVal,
                        Date = name + "|"
                    });

                    pos = nameEnd + 1;
                }
                Debug.Log($"[Cloud] Manual parse found {list.Count} entries");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Cloud] Manual parse failed: {e.Message}");
            }
            return list;
        }

        // ── Data Classes ────────────────────────────────────────────

        [Serializable] private class ScoreItem
        {
            public string player_name;
            public float depth;
            public int runes_collected;
            public string created_at;
        }

        [Serializable] private class ScoreArrayWrapper { public ScoreItem[] items; }
    }

    public class WeeklyWinnerData
    {
        public string PlayerName;
        public float BestDepth;
        public int WeekNumber;
        public int Year;
    }
}
