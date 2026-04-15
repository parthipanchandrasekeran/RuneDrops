using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Player
{
    /// <summary>
    /// Gives the player visual identity — glowing orb with trailing afterimages.
    /// Changes color based on active rune power state.
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        private SpriteRenderer _mainSprite;
        private SpriteRenderer _glowSprite;
        private Transform[] _trail;
        private SpriteRenderer[] _trailRenderers;
        private Vector3[] _trailPositions;
        private const int TRAIL_LENGTH = 6;

        private Color _baseColor = new Color(0.4f, 0.8f, 1f); // Cyan
        private Color _targetColor;
        private float _pulseTimer;

        private void Start()
        {
            _mainSprite = GetComponent<SpriteRenderer>();
            if (_mainSprite == null) return;

            // Make main sprite look like an orb, not a square
            _mainSprite.color = _baseColor;

            // Add inner glow (larger, semi-transparent)
            var glowGO = new GameObject("PlayerGlow");
            glowGO.transform.SetParent(transform);
            glowGO.transform.localPosition = Vector3.zero;
            _glowSprite = glowGO.AddComponent<SpriteRenderer>();
            _glowSprite.sprite = SpriteHelper.WhiteCircle;
            _glowSprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.2f);
            _glowSprite.sortingOrder = 9;
            glowGO.transform.localScale = Vector3.one * 2.5f;

            // Main sprite on top
            _mainSprite.sortingOrder = 10;

            // Create trail afterimages
            _trail = new Transform[TRAIL_LENGTH];
            _trailRenderers = new SpriteRenderer[TRAIL_LENGTH];
            _trailPositions = new Vector3[TRAIL_LENGTH];

            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                var trailGO = new GameObject($"Trail_{i}");
                trailGO.transform.position = transform.position;
                _trail[i] = trailGO.transform;
                _trailRenderers[i] = trailGO.AddComponent<SpriteRenderer>();
                _trailRenderers[i].sprite = SpriteHelper.WhiteCircle;
                float alpha = (1f - (float)i / TRAIL_LENGTH) * 0.15f;
                float scale = (1f - (float)i / TRAIL_LENGTH) * 0.7f;
                _trailRenderers[i].color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
                _trailRenderers[i].sortingOrder = 8;
                trailGO.transform.localScale = Vector3.one * scale;
                _trailPositions[i] = transform.position;
            }

            _targetColor = _baseColor;

            // Listen for power state changes
            EventBus.Subscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Subscribe<RunePowerExpiredEvent>(OnPowerExpired);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<ComboExpiredEvent>(OnComboExpired);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Unsubscribe<RunePowerExpiredEvent>(OnPowerExpired);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<ComboExpiredEvent>(OnComboExpired);

            // Clean up trail objects
            if (_trail != null)
            {
                foreach (var t in _trail)
                    if (t != null) Destroy(t.gameObject);
            }
        }

        private void Update()
        {
            if (_mainSprite == null) return;

            _pulseTimer += Time.deltaTime;

            // Pulse glow
            if (_glowSprite != null)
            {
                float pulse = 2.5f + Mathf.Sin(_pulseTimer * 3f) * 0.3f;
                _glowSprite.transform.localScale = Vector3.one * pulse;
                var gc = _glowSprite.color;
                gc.a = 0.15f + Mathf.Sin(_pulseTimer * 2f) * 0.05f;
                _glowSprite.color = gc;
            }

            // Lerp color toward target
            Color currentColor = Color.Lerp(_mainSprite.color, _targetColor, Time.deltaTime * 5f);
            _mainSprite.color = currentColor;

            // Update trail
            UpdateTrail(currentColor);
        }

        private void UpdateTrail(Color color)
        {
            if (_trail == null) return;

            // Shift trail positions (newest = current player pos)
            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
            {
                _trailPositions[i] = _trailPositions[i - 1];
            }
            _trailPositions[0] = transform.position;

            // Apply positions and colors
            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                if (_trail[i] == null) continue;
                _trail[i].position = _trailPositions[i];
                float alpha = (1f - (float)i / TRAIL_LENGTH) * 0.12f;
                _trailRenderers[i].color = new Color(color.r, color.g, color.b, alpha);
            }
        }

        // ── Power Color Changes ─────────────────────────────────────

        private void OnPowerActivated(RunePowerActivatedEvent evt)
        {
            _targetColor = evt.Type switch
            {
                1 => new Color(1f, 0.5f, 0f),   // Fire
                2 => new Color(0.3f, 1f, 1f),    // Wind
                3 => new Color(0.7f, 0.2f, 1f),  // Shadow
                4 => new Color(0.2f, 1f, 0.3f),  // Earth
                _ => _baseColor
            };
        }

        private void OnPowerExpired(RunePowerExpiredEvent evt)
        {
            _targetColor = _baseColor;
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            _targetColor = new Color(1f, 0.8f, 0.2f); // Gold for combo
        }

        private void OnComboExpired(ComboExpiredEvent evt)
        {
            _targetColor = _baseColor;
        }
    }
}
