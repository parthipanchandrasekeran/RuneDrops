using UnityEngine;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Sweeping energy blade — thin beam with edge glow and tip markers.
    /// </summary>
    public class MovingBladeObstacle : ObstacleBase
    {
        private float _startX, _amplitude, _speed, _phase;

        protected override void SetupVisuals()
        {
            var sr = EnsureSpriteRenderer();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.9f, 0.12f, 0.12f, 0.9f);
            sr.sortingOrder = 2;

            float width = Mathf.Lerp(1.2f, 2f, Difficulty);
            transform.localScale = new Vector3(width, 0.12f, 1f);

            // Edge glow (top + bottom)
            CreateEdge("BladeEdgeTop", new Vector3(0, 0.3f, 0), width, new Color(1f, 0.3f, 0.2f, 0.25f));
            CreateEdge("BladeEdgeBot", new Vector3(0, -0.3f, 0), width, new Color(1f, 0.3f, 0.2f, 0.25f));

            // Danger zone (wider faint area)
            var zoneGO = new GameObject("BladeZone");
            zoneGO.transform.SetParent(transform);
            zoneGO.transform.localPosition = Vector3.zero;
            var zoneSR = zoneGO.AddComponent<SpriteRenderer>();
            zoneSR.sprite = CreateSquareSprite();
            zoneSR.color = new Color(1f, 0.1f, 0.1f, 0.03f);
            zoneSR.sortingOrder = 1;
            zoneGO.transform.localScale = new Vector3(1f, 5f, 1f);

            // Tip markers
            CreateTip("TipL", new Vector3(-0.5f, 0, 0));
            CreateTip("TipR", new Vector3(0.5f, 0, 0));

            _startX = transform.position.x;
            _amplitude = Mathf.Lerp(1.5f, 3f, Difficulty);
            _speed = Mathf.Lerp(1.5f, 3f, Difficulty);
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void CreateEdge(string name, Vector3 localPos, float width, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = color;
            sr.sortingOrder = 3;
            go.transform.localScale = new Vector3(1f, 0.3f, 1f);
        }

        private void CreateTip(string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 0.4f, 0.2f, 0.45f);
            sr.sortingOrder = 3;
            go.transform.localScale = new Vector3(1.2f, 8f, 1f);
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
