using UnityEngine;

namespace RuneDrop.Level
{
    /// <summary>
    /// Calculates and exposes the playable horizontal width
    /// based on camera orthographic size and aspect ratio.
    /// </summary>
    public class WorldBounds : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static WorldBounds Instance { get; private set; }

        // ── Properties ──────────────────────────────────────────────
        public float LeftBound { get; private set; }
        public float RightBound { get; private set; }
        public float Width => RightBound - LeftBound;

        private Camera _camera;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _camera = Camera.main;
            UpdateBounds();
        }

        private void Update()
        {
            UpdateBounds();
        }

        // ── Calculation ─────────────────────────────────────────────

        private void UpdateBounds()
        {
            if (_camera == null) return;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            LeftBound = -halfWidth;
            RightBound = halfWidth;
        }

        /// <summary>Clamp an X position to within bounds with optional padding.</summary>
        public float ClampX(float x, float padding = 0.5f)
        {
            return Mathf.Clamp(x, LeftBound + padding, RightBound - padding);
        }

        /// <summary>Get a random X position within bounds.</summary>
        public float RandomX(float padding = 0.5f)
        {
            return Random.Range(LeftBound + padding, RightBound - padding);
        }
    }
}
