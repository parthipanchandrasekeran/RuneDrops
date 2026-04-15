namespace RuneDrop.Core
{
    /// <summary>
    /// Supabase connection config for cloud leaderboard.
    /// </summary>
    public static class SupabaseConfig
    {
        public const string ProjectUrl = "https://jntsrvgczelfmirqmtfh.supabase.co";
        public const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImpudHNydmdjemVsZm1pcnFtdGZoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM2MDI5MjEsImV4cCI6MjA4OTE3ODkyMX0.qI0JB8CD_bS4UUoUXRDVuPAckXYRPfeGYgO52YxuUI8";
        public const string ScoresTable = "runedrop_scores";
        public const string WinnersTable = "runedrop_weekly_winners";
    }
}
