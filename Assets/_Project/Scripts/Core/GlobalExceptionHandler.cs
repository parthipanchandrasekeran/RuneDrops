using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Catches unhandled exceptions to prevent silent crashes on Android.
    /// Logs errors with full stack traces.
    /// </summary>
    public class GlobalExceptionHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                Debug.LogError($"[GlobalExceptionHandler] Unhandled exception:\n{logString}\n{stackTrace}");

                // In production, could report to analytics here
            }
        }
    }
}
