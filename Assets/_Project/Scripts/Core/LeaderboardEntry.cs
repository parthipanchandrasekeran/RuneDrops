using System;
using System.Collections.Generic;

namespace RuneDrop.Core
{
    [Serializable]
    public class LeaderboardEntry
    {
        public float Depth;
        public int RunesCollected;
        public string Date;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> Entries = new();
    }
}
