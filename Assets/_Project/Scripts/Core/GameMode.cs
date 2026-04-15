namespace RuneDrop.Core
{
    /// <summary>
    /// Available game modes. Each modifies gameplay parameters.
    /// </summary>
    public enum GameMode
    {
        Classic = 0,      // Standard endless descent
        Sprint = 1,       // Reach 200m as fast as possible
        RuneRush = 2,     // Runes 3x but obstacles 2x
        NoAnchor = 3,     // Zero anchor charges, pure skill
        DailyChallenge = 4 // Same seed for everyone, 1 attempt per day
    }

    /// <summary>
    /// Describes how a game mode modifies gameplay.
    /// </summary>
    public static class GameModeConfig
    {
        public static string GetName(GameMode mode) => mode switch
        {
            GameMode.Classic => "CLASSIC",
            GameMode.Sprint => "SPRINT",
            GameMode.RuneRush => "RUNE RUSH",
            GameMode.NoAnchor => "NO ANCHOR",
            GameMode.DailyChallenge => "DAILY CHALLENGE",
            _ => "CLASSIC"
        };

        public static string GetDescription(GameMode mode) => mode switch
        {
            GameMode.Classic => "Endless descent. How deep can you go?",
            GameMode.Sprint => "Reach 200m as fast as possible!",
            GameMode.RuneRush => "3x runes but 2x obstacles. High risk!",
            GameMode.NoAnchor => "No anchors. Pure skill only.",
            GameMode.DailyChallenge => "Same seed for all. 1 attempt per day!",
            _ => ""
        };

        public static UnityEngine.Color GetColor(GameMode mode) => mode switch
        {
            GameMode.Classic => UIHelper.AccentCyan,
            GameMode.Sprint => UIHelper.AccentGold,
            GameMode.RuneRush => UIHelper.AccentPurple,
            GameMode.NoAnchor => UIHelper.AccentRed,
            GameMode.DailyChallenge => UIHelper.AccentGreen,
            _ => UIHelper.AccentCyan
        };

        public static float GetObstacleMultiplier(GameMode mode) => mode switch
        {
            GameMode.RuneRush => 2f,
            _ => 1f
        };

        public static float GetRuneMultiplier(GameMode mode) => mode switch
        {
            GameMode.RuneRush => 3f,
            _ => 1f
        };

        public static int GetAnchorOverride(GameMode mode) => mode switch
        {
            GameMode.NoAnchor => 0, // Zero anchors
            _ => -1 // -1 = use normal value
        };

        public static float GetSprintTargetDepth(GameMode mode) => mode switch
        {
            GameMode.Sprint => 200f,
            _ => 0f // 0 = no target (endless)
        };

        public static int GetDailySeed()
        {
            // Same seed for everyone on the same day
            var today = System.DateTime.UtcNow.Date;
            return today.Year * 10000 + today.Month * 100 + today.Day;
        }
    }
}
