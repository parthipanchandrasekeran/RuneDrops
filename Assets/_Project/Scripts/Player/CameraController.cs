using UnityEngine;
using RuneDrop.Data;

namespace RuneDrop.Player
{
    /// <summary>
    /// Smooth camera follow on Y axis. Keeps player centered horizontally,
    /// follows descent with look-ahead offset. Includes screen shake.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ── Configuration ───────────────────────────────────────────
        [Header("Config")]
        [SerializeField] private GameConfigSO _config;

        // ── State ───────────────────────────────────────────────────
        private Transform _target;

        // ── Shake ───────────────────────────────────────────────────
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;

        // ── Public API ──────────────────────────────────────────────

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null && _config != null)
            {
                var pos = transform.position;
                pos.y = _target.position.y - _config.CameraLookAhead;
                transform.position = pos;
            }
        }

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeTimer = duration;
            _shakeDuration = duration;
        }

        // ── Lifecycle ───────────────────────────────────────────────

        private void Start()
        {
            if (_config == null)
                _config = Resources.Load<GameConfigSO>("Configs/GameConfig");

            // Zoom in for mobile — makes everything bigger and more visible
            var cam = GetComponent<Camera>();
            if (cam != null) cam.orthographicSize = 7f;

            if (_target == null)
            {
                var player = PlayerController.Instance;
                if (player != null)
                    SetTarget(player.transform);
            }
        }

        private void LateUpdate()
        {
            if (_target == null || _config == null) return;

            var pos = transform.position;

            // Smooth follow on Y axis
            float targetY = _target.position.y - _config.CameraLookAhead;
            pos.y = Mathf.Lerp(pos.y, targetY, _config.CameraFollowSpeed * Time.deltaTime);
            pos.x = 0f;

            // Apply shake AFTER follow so it's visible
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float decay = Mathf.Clamp01(_shakeTimer / Mathf.Max(_shakeDuration, 0.01f));
                Vector2 offset = Random.insideUnitCircle * _shakeIntensity * decay;
                pos.x += offset.x;
                pos.y += offset.y;
            }

            transform.position = pos;
        }
    }
}
