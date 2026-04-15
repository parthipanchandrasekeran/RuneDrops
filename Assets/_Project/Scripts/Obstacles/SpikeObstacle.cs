using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Static spike obstacle. Red diamond shape — clearly dangerous.
    /// </summary>
    public class SpikeObstacle : ObstacleBase
    {
        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateDiamondSprite();
            sr.color = new Color(0.9f, 0.1f, 0.1f); // BRIGHT RED — danger
            sr.sortingOrder = 2;

            float size = Mathf.Lerp(0.9f, 1.4f, Difficulty);
            transform.localScale = new Vector3(size, size, 1f);
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        protected override void SetupCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
        }
    }
}
