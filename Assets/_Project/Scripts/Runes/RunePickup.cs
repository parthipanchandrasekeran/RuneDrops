using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Runes
{
    /// <summary>
    /// Collectible rune. Bright colored diamond (rotated square) with glow.
    /// Uses SpriteHelper built-in sprites for device compatibility.
    /// </summary>
    public class RunePickup : MonoBehaviour
    {
        private RuneType _runeType;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _glowBG;
        private float _bobOffset;
        private Vector3 _basePosition;
        private bool _collected;

        public void Initialize(RuneType type)
        {
            _runeType = type;
            _collected = false;
            _basePosition = transform.position;
            _bobOffset = Random.Range(0f, Mathf.PI * 2f);

            var color = GetColor(type);
            var sprite = SpriteHelper.WhiteSquare;

            // Glow circle behind (bigger, semi-transparent)
            var glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(transform);
            glowGO.transform.localPosition = new Vector3(0, 0, 0.1f);
            _glowBG = glowGO.AddComponent<SpriteRenderer>();
            if (SpriteHelper.WhiteCircle != null)
                _glowBG.sprite = SpriteHelper.WhiteCircle;
            _glowBG.color = new Color(1f, 1f, 1f, 0.25f);
            _glowBG.sortingOrder = 3;
            glowGO.transform.localScale = Vector3.one * 3f;

            // The rune itself (rotated 45 to look like diamond)
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (sprite != null)
                _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = color;
            _spriteRenderer.sortingOrder = 5;

            // Rotate 45 degrees for diamond shape
            transform.localRotation = Quaternion.Euler(0, 0, 45f);

            // Collider
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;

            transform.localScale = Vector3.one * 0.8f;
        }

        private void Update()
        {
            if (_collected) return;

            float bob = Mathf.Sin(Time.time * 2.5f + _bobOffset) * 0.2f;
            transform.position = _basePosition + new Vector3(0f, bob, 0f);

            float pulse = 0.8f + Mathf.Sin(Time.time * 3f + _bobOffset) * 0.08f;
            transform.localScale = Vector3.one * pulse;

            if (_glowBG != null)
            {
                float glowScale = 2f + Mathf.Sin(Time.time * 2f + _bobOffset) * 0.3f;
                _glowBG.transform.localScale = Vector3.one * glowScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;
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
