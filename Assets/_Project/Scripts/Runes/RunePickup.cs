using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Runes
{
    /// <summary>
    /// Precious ancient artifact — layered gem with sparkle motes,
    /// inner core highlight, outer attraction field, and gentle wobble.
    /// </summary>
    public class RunePickup : MonoBehaviour
    {
        private RuneType _runeType;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _glowBG;
        private SpriteRenderer _field;
        private SpriteRenderer _innerCore;
        private Transform[] _sparkles;
        private float _bobOffset;
        private Vector3 _basePosition;
        private bool _collected;
        private float _sparkleAngle;

        public void Initialize(RuneType type)
        {
            _runeType = type;
            _collected = false;
            _basePosition = transform.position;
            _bobOffset = Random.Range(0f, Mathf.PI * 2f);

            var color = GetColor(type);
            var brightColor = new Color(
                Mathf.Min(color.r + 0.3f, 1f),
                Mathf.Min(color.g + 0.3f, 1f),
                Mathf.Min(color.b + 0.3f, 1f), 0.65f);

            // Outer attraction field (very large, very faint)
            var fieldGO = new GameObject("RuneField");
            fieldGO.transform.SetParent(transform);
            fieldGO.transform.localPosition = new Vector3(0, 0, 0.2f);
            _field = fieldGO.AddComponent<SpriteRenderer>();
            _field.sprite = SpriteHelper.WhiteCircle;
            _field.color = new Color(color.r, color.g, color.b, 0.03f);
            _field.sortingOrder = 2;
            fieldGO.transform.localScale = Vector3.one * 3f;

            // Middle glow
            var glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(transform);
            glowGO.transform.localPosition = new Vector3(0, 0, 0.1f);
            _glowBG = glowGO.AddComponent<SpriteRenderer>();
            _glowBG.sprite = SpriteHelper.WhiteCircle;
            _glowBG.color = new Color(color.r, color.g, color.b, 0.12f);
            _glowBG.sortingOrder = 3;
            glowGO.transform.localScale = Vector3.one * 1.6f;

            // Diamond body (rotated square)
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = SpriteHelper.WhiteSquare;
            _spriteRenderer.color = color;
            _spriteRenderer.sortingOrder = 5;
            transform.localRotation = Quaternion.Euler(0, 0, 45f);

            // Inner gem highlight (brighter center)
            var coreGO = new GameObject("RuneCore");
            coreGO.transform.SetParent(transform);
            coreGO.transform.localPosition = Vector3.zero;
            _innerCore = coreGO.AddComponent<SpriteRenderer>();
            _innerCore.sprite = SpriteHelper.WhiteSquare;
            _innerCore.color = brightColor;
            _innerCore.sortingOrder = 6;
            coreGO.transform.localScale = Vector3.one * 0.4f;

            // Sparkle motes (2 orbiting dots)
            _sparkles = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var sGO = new GameObject($"Sparkle_{i}");
                sGO.transform.SetParent(transform);
                var sSR = sGO.AddComponent<SpriteRenderer>();
                sSR.sprite = SpriteHelper.WhiteCircle;
                sSR.color = new Color(color.r, color.g, color.b, 0.45f);
                sSR.sortingOrder = 6;
                sGO.transform.localScale = Vector3.one * 0.08f;
                _sparkles[i] = sGO.transform;
            }

            // Collider
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;

            transform.localScale = Vector3.one * 0.45f;
        }

        private void Update()
        {
            if (_collected) return;

            // Bob
            float bob = Mathf.Sin(Time.time * 2.5f + _bobOffset) * 0.18f;
            transform.position = _basePosition + new Vector3(0f, bob, 0f);

            // Scale pulse
            float pulse = 0.45f + Mathf.Sin(Time.time * 3f + _bobOffset) * 0.04f;
            transform.localScale = Vector3.one * pulse;

            // Gentle wobble rotation
            transform.localRotation = Quaternion.Euler(0, 0, 45f + Mathf.Sin(Time.time * 2f + _bobOffset) * 4f);

            // Glow pulse
            if (_glowBG != null)
            {
                float glowScale = 1.6f + Mathf.Sin(Time.time * 2f + _bobOffset) * 0.3f;
                _glowBG.transform.localScale = Vector3.one * glowScale;
            }

            // Field pulse
            if (_field != null)
            {
                float fieldScale = 3f + Mathf.Sin(Time.time * 1.5f + _bobOffset) * 0.4f;
                _field.transform.localScale = Vector3.one * fieldScale;
            }

            // Sparkle orbits
            _sparkleAngle += 90f * Time.deltaTime;
            for (int i = 0; i < 2 && _sparkles != null; i++)
            {
                if (_sparkles[i] == null) continue;
                float angle = (_sparkleAngle + i * 180f) * Mathf.Deg2Rad;
                _sparkles[i].localPosition = new Vector3(Mathf.Cos(angle) * 0.55f, Mathf.Sin(angle) * 0.55f, 0);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;
            var gm = RuneDrop.Core.GameManager.Instance;
            if (gm != null && gm.CurrentState == RuneDrop.Core.GameState.DecisionRoom) return;
            _collected = true;
            RuneInventory.Instance?.CollectRune(_runeType);
            Destroy(gameObject);
        }

        private Color GetColor(RuneType type) => type switch
        {
            RuneType.Fire => new Color(1f, 0.6f, 0f),
            RuneType.Wind => new Color(0.3f, 0.95f, 1f),
            RuneType.Shadow => new Color(0.8f, 0.3f, 1f),
            RuneType.Earth => new Color(0.3f, 1f, 0.3f),
            _ => Color.white
        };
    }
}
