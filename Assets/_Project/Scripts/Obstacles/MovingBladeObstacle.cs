using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Oscillating blade obstacle. Dark red thin rectangle.
    /// </summary>
    public class MovingBladeObstacle : ObstacleBase
    {
        private float _startX;
        private float _amplitude;
        private float _speed;
        private float _phase;

        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.8f, 0.15f, 0.15f); // DARK RED
            sr.sortingOrder = 2;

            float width = Mathf.Lerp(1.2f, 2f, Difficulty);
            transform.localScale = new Vector3(width, 0.15f, 1f);

            _startX = transform.position.x;
            _amplitude = Mathf.Lerp(1.5f, 3f, Difficulty);
            _speed = Mathf.Lerp(1.5f, 3f, Difficulty);
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        protected override void SetupCollider()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            float x = _startX + Mathf.Sin(Time.time * _speed + _phase) * _amplitude;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }
    }
}
