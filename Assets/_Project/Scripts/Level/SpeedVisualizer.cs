using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;

namespace RuneDrop.Level
{
    /// <summary>
    /// Speed-reactive visual effects — side streaks, edge vignette,
    /// and camera zoom that intensify with fall speed.
    /// </summary>
    public class SpeedVisualizer : MonoBehaviour
    {
        private SpriteRenderer[] _streaks;
        private SpriteRenderer[] _vignette;
        private Camera _camera;
        private float _baseOrthoSize;
        private const int STREAK_COUNT = 6;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null) return;
            _baseOrthoSize = _camera.orthographicSize;

            CreateStreaks();
            CreateVignette();
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            var player = PlayerController.Instance;
            float speed = player != null && player.IsAlive ? player.CurrentFallSpeed : 0f;
            float intensity = Mathf.InverseLerp(5f, 12f, speed);

            UpdateStreaks(intensity);
            UpdateVignette(intensity);
            UpdateCameraZoom(intensity);
        }

        private void CreateStreaks()
        {
            _streaks = new SpriteRenderer[STREAK_COUNT];
            float halfW = _camera.orthographicSize * _camera.aspect;

            for (int i = 0; i < STREAK_COUNT; i++)
            {
                var go = new GameObject($"SpeedStreak_{i}");
                go.transform.SetParent(transform);
                _streaks[i] = go.AddComponent<SpriteRenderer>();
                _streaks[i].sprite = SpriteHelper.WhiteSquare;
                _streaks[i].color = new Color(0.3f, 0.5f, 0.8f, 0f);
                _streaks[i].sortingOrder = -3;

                float xSide = (i % 2 == 0) ? -1f : 1f;
                float xOffset = halfW * Random.Range(0.5f, 0.95f) * xSide;
                go.transform.localPosition = new Vector3(xOffset, Random.Range(-5f, 5f), 0);
                go.transform.localScale = new Vector3(0.012f, Random.Range(1f, 2.5f), 1f);
            }
        }

        private void CreateVignette()
        {
            _vignette = new SpriteRenderer[4]; // L, R, T, B
            float halfW = _camera.orthographicSize * _camera.aspect;
            float halfH = _camera.orthographicSize;
            Color vigColor = new Color(0.01f, 0.005f, 0.04f, 0f);

            _vignette[0] = CreateVignetteEdge("VigL", new Vector3(-halfW - 0.8f, 0, 0), new Vector3(2f, halfH * 3f, 1f), vigColor);
            _vignette[1] = CreateVignetteEdge("VigR", new Vector3(halfW + 0.8f, 0, 0), new Vector3(2f, halfH * 3f, 1f), vigColor);
            _vignette[2] = CreateVignetteEdge("VigT", new Vector3(0, halfH + 0.5f, 0), new Vector3(halfW * 3f, 1.2f, 1f), vigColor);
            _vignette[3] = CreateVignetteEdge("VigB", new Vector3(0, -halfH - 0.5f, 0), new Vector3(halfW * 3f, 1.2f, 1f), vigColor);
        }

        private SpriteRenderer CreateVignetteEdge(string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_camera.transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteSquare;
            sr.color = color;
            sr.sortingOrder = 6;
            return sr;
        }

        private void UpdateStreaks(float intensity)
        {
            if (_streaks == null) return;
            float camY = _camera.transform.position.y;

            for (int i = 0; i < STREAK_COUNT; i++)
            {
                if (_streaks[i] == null) continue;

                // Fade alpha based on speed
                var c = _streaks[i].color;
                c.a = Mathf.Lerp(c.a, intensity * 0.12f, Time.deltaTime * 3f);
                _streaks[i].color = c;

                // Scroll upward (relative to camera)
                var pos = _streaks[i].transform.position;
                pos.y += intensity * 15f * Time.deltaTime;
                if (pos.y > camY + 10f)
                    pos.y = camY - 10f;
                _streaks[i].transform.position = pos;
            }
        }

        private void UpdateVignette(float intensity)
        {
            if (_vignette == null) return;
            float vigAlpha = Mathf.Lerp(0f, 0.35f, Mathf.InverseLerp(0.5f, 1f, intensity));

            foreach (var vig in _vignette)
            {
                if (vig == null) continue;
                var c = vig.color;
                c.a = Mathf.Lerp(c.a, vigAlpha, Time.deltaTime * 2f);
                vig.color = c;
            }
        }

        private void UpdateCameraZoom(float intensity)
        {
            // Slight zoom out at high speed for dramatic feel
            float targetSize = _baseOrthoSize + intensity * 0.4f;
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetSize, Time.deltaTime * 2f);
        }
    }
}
