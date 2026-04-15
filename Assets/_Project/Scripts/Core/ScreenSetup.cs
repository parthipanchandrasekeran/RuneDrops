using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Basic screen setup. No orientation hacks.
    /// </summary>
    public class ScreenSetup : MonoBehaviour
    {
        public static bool IsFlipped => false;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public static Vector2 FixTouchPos(Vector2 pos) => pos;
    }
}
