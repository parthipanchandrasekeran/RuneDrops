using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Crystalline danger spike — layered diamond with glow, inner highlight, and pulse.
    /// </summary>
    public class SpikeObstacle : ObstacleBase
    {
        private SpriteRenderer _glowSR;
        private SpriteRenderer _innerSR;
        private float _phase;

        protected override void SetupVisuals()
        {
            _phase = Random.Range(0f, Mathf.PI * 2f);
            float size = Mathf.Lerp(0.5f, 0.8f, Difficulty);

            // Warning glow (large, faint)
            var glowGO = new GameObject("SpikeGlow");
            glowGO.transform.SetParent(transform);
            glowGO.transform.localPosition = Vector3.zero;
            _glowSR = glowGO.AddComponent<SpriteRenderer>();
            _glowSR.sprite = CreateCircleSprite();
            _glowSR.color = new Color(1f, 0.15f, 0.1f, 0.07f);
            _glowSR.sortingOrder = 1;
            glowGO.transform.localScale = Vector3.one * size * 2.2f;

            // Main crystal body
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateDiamondSprite();
            sr.color = new Color(1f, 0.2f, 0.15f);
            sr.sortingOrder = 2;
            transform.localScale = new Vector3(size, size, 1f);
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            // Inner highlight (brighter center)
            var innerGO = new GameObject("SpikeInner");
            innerGO.transform.SetParent(transform);
            innerGO.transform.localPosition = Vector3.zero;
            innerGO.transform.localRotation = Quaternion.identity;
            _innerSR = innerGO.AddComponent<SpriteRenderer>();
            _innerSR.sprite = CreateSquareSprite();
            _innerSR.color = new Color(1f, 0.5f, 0.3f, 0.5f);
            _innerSR.sortingOrder = 3;
            innerGO.transform.localScale = Vector3.one * 0.45f;
        }

        protected override void SetupCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
        }

        private void Update()
        {
            // Slow rotate
            transform.Rotate(0, 0, 20f * Time.deltaTime);
            // Pulse glow
            if (_glowSR != null)
            {
                float pulse = 2.2f + Mathf.Sin(Time.time * 4f + _phase) * 0.3f;
                _glowSR.transform.localScale = Vector3.one * pulse;
            }
        }
    }
}
