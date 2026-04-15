namespace RuneDrop.Player
{
    /// <summary>
    /// All possible player states during a run.
    /// </summary>
    public enum PlayerState
    {
        Falling,
        Anchored,
        Phasing,    // Shadow rune: pass through obstacles
        Shielded,   // Earth rune: absorb one hit
        Dead
    }
}
