using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Ancient boulder — layered circle with shadow, highlight, and trailing crumbles.
    /// </summary>
    public class FallingRockObstacle : ObstacleBase
    {
        private float _fallSpeed;
        private float _rotateSpeed;
        private Transform[] _crumbles;

        protected override void SetupVisuals()
        {
            float size = Mathf.Lerp(0.5f, 0.9f, Difficulty);

            // Shadow (larger, darker, behind)
            var shadowGO = new GameObject("RockShadow");
            shadowGO.transform.SetParent(transform);
            shadowGO.transform.localPosition = new Vector3(0.05f, -0.05f, 0);
            var shadowSR = shadowGO.AddComponent<SpriteRenderer>();
            shadowSR.sprite = CreateCircleSprite();
            shadowSR.color = new Color(0.2f, 0.04f, 0.02f, 0.35f);
            shadowSR.sortingOrder = 1;
            shadowGO.transform.localScale = Vector3.one * 1.25f;

            // Main body
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0.55f, 0.15f, 0.1f);
            sr.sortingOrder = 2;
            transform.localScale = new Vector3(size, size * 0.85f, 1f);

            // Highlight (upper-left for 3D illusion)
            var hlGO = new GameObject("RockHighlight");
            hlGO.transform.SetParent(transform);
            hlGO.transform.localPosition = new Vector3(-0.12f, 0.12f, 0);
            var hlSR = hlGO.AddComponent<SpriteRenderer>();
            hlSR.sprite = CreateCircleSprite();
            hlSR.color = new Color(0.85f, 0.3f, 0.15f, 0.35f);
            hlSR.sortingOrder = 3;
            hlGO.transform.localScale = Vector3.one * 0.4f;

            // Trailing crumble particles
            _crumbles = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var cGO = new GameObject($"Crumble_{i}");
                cGO.transform.SetParent(transform);
                cGO.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.5f), 0);
                var cSR = cGO.AddComponent<SpriteRenderer>();
                cSR.sprite = CreateSquareSprite();
                cSR.color = new Color(0.45f, 0.1f, 0.07f, 0.25f);
                cSR.sortingOrder = 1;
                cGO.transform.localScale = Vector3.one * Random.Range(0.06f, 0.12f);
                cGO.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                _crumbles[i] = cGO.transform;
            }

            _fallSpeed = Mathf.Lerp(1f, 2f, Difficulty);
            _rotateSpeed = Random.Range(-45f, 45f);
        }

        protected override void SetupCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        private void Update()
        {
            transform.Translate(0f, -_fallSpeed * Time.deltaTime, 0f);
            transform.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);

            // Crumbles drift upward relative to rock
            if (_crumbles != null)
            {
                foreach (var c in _crumbles)
                {
                    if (c == null) continue;
                    c.localPosition += new Vector3(0, 0.4f * Time.deltaTime, 0);
                    if (c.localPosition.y > 0.8f)
                        c.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), -0.1f, 0);
                }
            }
        }
    }
}
