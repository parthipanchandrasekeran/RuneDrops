using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Falling rock obstacle. Dark red-brown circle.
    /// </summary>
    public class FallingRockObstacle : ObstacleBase
    {
        private float _fallSpeed;
        private float _rotateSpeed;

        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0.7f, 0.15f, 0.1f); // RED-BROWN
            sr.sortingOrder = 2;

            float size = Mathf.Lerp(1.0f, 1.5f, Difficulty);
            transform.localScale = new Vector3(size, size, 1f);

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
        }
    }
}
