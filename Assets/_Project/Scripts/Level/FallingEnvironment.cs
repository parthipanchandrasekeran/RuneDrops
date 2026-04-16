using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;

namespace RuneDrop.Level
{
    /// <summary>
    /// Rich visual environment — grid lines, side ruins, depth markers,
    /// floating dust, deep parallax layers, and zone color shifts.
    /// Creates atmosphere and sells the falling sensation.
    /// </summary>
    public class FallingEnvironment : MonoBehaviour
    {
        // Grid lines
        private SpriteRenderer[] _gridLines;
        private const int GRID_LINE_COUNT = 12;
        private const float GRID_SPACING = 3.5f;

        // Side ruins (2 columns per side)
        private SpriteRenderer[] _leftRuins, _rightRuins;
        private SpriteRenderer[] _leftRuinsInner, _rightRuinsInner;
        private const int RUIN_COUNT = 8;
        private const float RUIN_SPACING = 4f;

        // Depth markers
        private SpriteRenderer _depthMarker;
        private int _lastDepthMarker = -1;

        // Floating dust
        private Transform[] _dust;
        private SpriteRenderer[] _dustRenderers;
        private const int DUST_COUNT = 15;

        // Deep parallax background shapes
        private Transform[] _deepShapes;
        private const int DEEP_COUNT = 10;

        private Camera _camera;
        private float _halfWidth;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null) return;
            _halfWidth = _camera.orthographicSize * _camera.aspect;

            CreateGridLines();
            CreateSideRuins();
            CreateDepthMarker();
            CreateFloatingDust();
            CreateDeepParallax();
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            float camY = _camera.transform.position.y;

            UpdateGridLines(camY);
            UpdateSideRuins(camY);
            UpdateDepthMarker(camY);
            UpdateFloatingDust(camY);
            UpdateDeepParallax(camY);
            UpdateEnvironmentColor();
        }

        // ── Grid Lines ──────────────────────────────────────────────

        private void CreateGridLines()
        {
            _gridLines = new SpriteRenderer[GRID_LINE_COUNT];
            for (int i = 0; i < GRID_LINE_COUNT; i++)
            {
                var go = new GameObject($"GridLine_{i}");
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteHelper.WhiteSquare;
                // Every 3rd line is "major" — brighter and thicker
                bool major = (i % 3 == 0);
                sr.color = new Color(0.15f, 0.1f, 0.25f, major ? 0.2f : 0.08f);
                sr.sortingOrder = -8;
                go.transform.localScale = new Vector3(_halfWidth * 2.5f, major ? 0.04f : 0.02f, 1f);
                _gridLines[i] = sr;
            }
        }

        private void UpdateGridLines(float camY)
        {
            for (int i = 0; i < GRID_LINE_COUNT; i++)
            {
                float baseY = camY - (camY % GRID_SPACING) + (i - GRID_LINE_COUNT / 2) * GRID_SPACING;
                _gridLines[i].transform.position = new Vector3(0f, baseY, 0f);
            }
        }

        // ── Side Ruins ──────────────────────────────────────────────

        private void CreateSideRuins()
        {
            _leftRuins = new SpriteRenderer[RUIN_COUNT];
            _rightRuins = new SpriteRenderer[RUIN_COUNT];
            _leftRuinsInner = new SpriteRenderer[RUIN_COUNT];
            _rightRuinsInner = new SpriteRenderer[RUIN_COUNT];

            for (int i = 0; i < RUIN_COUNT; i++)
            {
                // Outer column
                _leftRuins[i] = CreateRuinBlock(-_halfWidth - 0.2f, i, -9, 0.35f);
                _rightRuins[i] = CreateRuinBlock(_halfWidth + 0.2f, i, -9, 0.35f);
                // Inner column (more transparent, creates depth)
                _leftRuinsInner[i] = CreateRuinBlock(-_halfWidth + 0.3f, i + 100, -10, 0.15f);
                _rightRuinsInner[i] = CreateRuinBlock(_halfWidth - 0.3f, i + 100, -10, 0.15f);
            }
        }

        private SpriteRenderer CreateRuinBlock(float xPos, int index, int sortOrder, float maxAlpha)
        {
            var go = new GameObject($"Ruin_{(xPos < 0 ? "L" : "R")}_{index}");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteSquare;
            float shade = Random.Range(0.03f, 0.07f);
            float r = shade + Random.Range(-0.01f, 0.02f);
            float b = shade + Random.Range(0f, 0.04f);
            sr.color = new Color(r, shade * 0.7f, b, Random.Range(maxAlpha * 0.5f, maxAlpha));
            sr.sortingOrder = sortOrder;
            float width = Random.Range(0.3f, 0.9f);
            float height = Random.Range(1.5f, 4f);
            go.transform.localScale = new Vector3(width, height, 1f);
            go.transform.position = new Vector3(xPos, 0, 0);
            return sr;
        }

        private void UpdateSideRuins(float camY)
        {
            UpdateRuinArray(_leftRuins, camY, -_halfWidth - 0.2f);
            UpdateRuinArray(_rightRuins, camY, _halfWidth + 0.2f);
            UpdateRuinArray(_leftRuinsInner, camY, -_halfWidth + 0.3f);
            UpdateRuinArray(_rightRuinsInner, camY, _halfWidth - 0.3f);
        }

        private void UpdateRuinArray(SpriteRenderer[] ruins, float camY, float xPos)
        {
            if (ruins == null) return;
            for (int i = 0; i < ruins.Length; i++)
            {
                if (ruins[i] == null) continue;
                float baseY = camY - (camY % RUIN_SPACING) + (i - ruins.Length / 2) * RUIN_SPACING;
                ruins[i].transform.position = new Vector3(xPos, baseY, 0f);
            }
        }

        // ── Depth Markers ───────────────────────────────────────────

        private void CreateDepthMarker()
        {
            var go = new GameObject("DepthMarker");
            go.transform.SetParent(transform);
            _depthMarker = go.AddComponent<SpriteRenderer>();
            _depthMarker.sprite = SpriteHelper.WhiteSquare;
            _depthMarker.color = new Color(0.3f, 0.2f, 0.5f, 0.3f);
            _depthMarker.sortingOrder = -7;
            go.transform.localScale = new Vector3(_halfWidth * 2.5f, 0.05f, 1f);
            go.SetActive(false);
        }

        private void UpdateDepthMarker(float camY)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            int currentMarker = Mathf.FloorToInt(player.DepthTraveled / 50f);
            if (currentMarker > _lastDepthMarker && currentMarker > 0)
            {
                _lastDepthMarker = currentMarker;
                _depthMarker.gameObject.SetActive(true);
                _depthMarker.transform.position = new Vector3(0f, player.transform.position.y - 3f, 0f);
                _depthMarker.color = new Color(0.5f, 0.3f, 0.8f, 0.5f);
            }

            if (_depthMarker.gameObject.activeSelf)
            {
                var c = _depthMarker.color;
                c.a -= Time.deltaTime * 0.25f;
                if (c.a <= 0f) _depthMarker.gameObject.SetActive(false);
                else _depthMarker.color = c;
            }
        }

        // ── Floating Dust ───────────────────────────────────────────

        private void CreateFloatingDust()
        {
            _dust = new Transform[DUST_COUNT];
            _dustRenderers = new SpriteRenderer[DUST_COUNT];

            for (int i = 0; i < DUST_COUNT; i++)
            {
                var go = new GameObject($"Dust_{i}");
                go.transform.SetParent(transform);
                _dust[i] = go.transform;
                _dustRenderers[i] = go.AddComponent<SpriteRenderer>();
                _dustRenderers[i].sprite = SpriteHelper.WhiteCircle;
                _dustRenderers[i].color = new Color(0.3f, 0.25f, 0.5f, Random.Range(0.03f, 0.07f));
                _dustRenderers[i].sortingOrder = -5;
                go.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
                go.transform.position = new Vector3(
                    Random.Range(-_halfWidth, _halfWidth),
                    Random.Range(-10f, 10f), 0);
            }
        }

        private void UpdateFloatingDust(float camY)
        {
            if (_dust == null) return;
            for (int i = 0; i < DUST_COUNT; i++)
            {
                if (_dust[i] == null) continue;
                var pos = _dust[i].position;
                // Drift upward relative to camera + gentle sway
                pos.y += 0.25f * Time.deltaTime;
                pos.x += Mathf.Sin(Time.time * 0.5f + i) * 0.1f * Time.deltaTime;

                // Wrap when out of view
                if (pos.y > camY + 10f)
                {
                    pos.y = camY - 10f;
                    pos.x = Random.Range(-_halfWidth, _halfWidth);
                }
                _dust[i].position = pos;
            }
        }

        // ── Deep Parallax Background ────────────────────────────────

        private void CreateDeepParallax()
        {
            _deepShapes = new Transform[DEEP_COUNT];
            for (int i = 0; i < DEEP_COUNT; i++)
            {
                var go = new GameObject($"DeepShape_{i}");
                go.transform.SetParent(transform);
                _deepShapes[i] = go.transform;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = (i % 3 == 0) ? SpriteHelper.WhiteCircle : SpriteHelper.WhiteSquare;
                sr.color = new Color(
                    Random.Range(0.03f, 0.08f),
                    Random.Range(0.02f, 0.06f),
                    Random.Range(0.05f, 0.12f),
                    Random.Range(0.03f, 0.06f));
                sr.sortingOrder = -12;
                float scale = Random.Range(0.5f, 2.5f);
                go.transform.localScale = new Vector3(scale, Random.Range(scale * 0.5f, scale * 2f), 1f);
                go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                go.transform.position = new Vector3(
                    Random.Range(-_halfWidth * 1.5f, _halfWidth * 1.5f),
                    Random.Range(-15f, 15f), 0);
            }
        }

        private void UpdateDeepParallax(float camY)
        {
            if (_deepShapes == null) return;
            float parallax = 0.6f; // Moves slower = appears farther
            for (int i = 0; i < DEEP_COUNT; i++)
            {
                if (_deepShapes[i] == null) continue;
                var pos = _deepShapes[i].position;
                float targetY = camY * parallax + (i - DEEP_COUNT / 2) * 3f;
                pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime);
                // Wrap
                if (Mathf.Abs(pos.y - camY) > 15f)
                    pos.y = camY + (pos.y > camY ? -14f : 14f);
                _deepShapes[i].position = pos;
            }
        }

        // ── Environment Color Shift ─────────────────────────────────

        private void UpdateEnvironmentColor()
        {
            var player = PlayerController.Instance;
            if (player == null || _camera == null) return;

            float depth = player.DepthTraveled;
            int zone = Mathf.FloorToInt(depth / 100f);
            float zoneProgress = (depth % 100f) / 100f;

            Color fromColor = GetZoneColor(zone);
            Color toColor = GetZoneColor(zone + 1);
            Color bgColor = Color.Lerp(fromColor, toColor, zoneProgress);
            _camera.backgroundColor = bgColor;

            // Tint grid lines to match zone
            Color gridColor = new Color(bgColor.r + 0.08f, bgColor.g + 0.06f, bgColor.b + 0.12f, 0.12f);
            for (int i = 0; i < _gridLines.Length; i++)
            {
                if (_gridLines[i] == null) continue;
                bool major = (i % 3 == 0);
                _gridLines[i].color = new Color(gridColor.r, gridColor.g, gridColor.b, major ? 0.18f : gridColor.a);
            }
        }

        private Color GetZoneColor(int zone)
        {
            return (zone % 5) switch
            {
                0 => new Color(0.04f, 0.02f, 0.08f),  // Dark purple
                1 => new Color(0.02f, 0.04f, 0.10f),  // Dark blue
                2 => new Color(0.02f, 0.06f, 0.06f),  // Dark teal
                3 => new Color(0.08f, 0.02f, 0.04f),  // Dark crimson
                4 => new Color(0.06f, 0.01f, 0.01f),  // Deep black-red
                _ => new Color(0.04f, 0.02f, 0.08f)
            };
        }
    }
}
