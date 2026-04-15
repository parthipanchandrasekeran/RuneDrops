namespace RuneDrop.Runes
{
    /// <summary>
    /// Static utility for order-independent rune combo detection.
    /// </summary>
    public static class ComboDetector
    {
        public static ComboType Detect(RuneType a, RuneType b)
        {
            if (a == RuneType.None || b == RuneType.None) return ComboType.None;

            // Order-independent matching
            int set = ((int)a * 10) + (int)b;

            return set switch
            {
                12 or 21 => ComboType.FlameTrail,       // Fire + Wind
                23 or 32 => ComboType.BlinkDash,         // Shadow + Wind
                41 or 14 => ComboType.ExplosiveShield,   // Earth + Fire
                _ => ComboType.None
            };
        }
    }
}
