using UnityEngine;

namespace RuneDrop.Player
{
    /// <summary>
    /// Visual feedback for player movement.
    /// Leans sprite into horizontal movement direction.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        // ── Configuration ───────────────────────────────────────────
        [Header("Lean")]
        [SerializeField] private float _maxLeanAngle = 15f;
        [SerializeField] private float _leanSpeed = 8f;

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        // ── State ───────────────────────────────────────────────────
        private float _previousX;
        private float _currentLean;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Start()
        {
            _previousX = transform.position.x;
        }

        private void Update()
        {
            float currentX = transform.position.x;
            float deltaX = currentX - _previousX;
            _previousX = currentX;

            // Calculate target lean angle based on horizontal velocity
            float targetLean = 0f;
            if (Mathf.Abs(deltaX) > 0.001f)
            {
                targetLean = -Mathf.Sign(deltaX) * _maxLeanAngle;
            }

            // Smooth lean
            _currentLean = Mathf.Lerp(_currentLean, targetLean, _leanSpeed * Time.deltaTime);

            // Apply rotation
            transform.rotation = Quaternion.Euler(0f, 0f, _currentLean);
        }
    }
}
