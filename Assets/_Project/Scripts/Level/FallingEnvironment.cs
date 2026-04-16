using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;

namespace RuneDrop.Level
{
    /// <summary>
    /// Creates visual falling cues — scrolling grid lines, side ruins, and depth markers.
    /// Makes the player FEEL like they're falling, not just watching a meter.
    /// </summary>
    public class FallingEnvironment : MonoBehaviour
    {
        // ── Grid Lines (horizontal speed lines) ─────────────────────
        private SpriteRenderer[] _gridLines;
        private const int GRID_LINE_COUNT = 8;
        private const float GRID_SPACING = 4f;

        // ── Side Ruins (left and right decorative pillars) ──────────
        private SpriteRenderer[] _leftRuins;
        private SpriteRenderer[] _rightRuins;
        private const int RUIN_COUNT = 6;
        private const float RUIN_SPACING = 5f;

        // ── Depth Markers ───────────────────────────────────────────
        private SpriteRenderer _depthMarker;
        private SpriteRenderer _depthMarkerText;
        private int _lastDepthMarker = -1;

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
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            float camY = _camera.transform.position.y;

            UpdateGridLines(camY);
            UpdateSideRuins(camY);
            UpdateDepthMarker(camY);
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
                sr.color = new Color(0.15f, 0.1f, 0.25f, 0.15f);
                sr.sortingOrder = -8;
                go.transform.localScale = new Vector3(_halfWidth * 2.5f, 0.03f, 1f);
                _gridLines[i] = sr;
            }
        }

        private void UpdateGridLines(float camY)
        {
            for (int i = 0; i < GRID_LINE_COUNT; i++)
            {
                // Position lines relative to camera, wrapping
                float baseY = camY - (camY % GRID_SPACING) + (i - GRID_LINE_COUNT / 2) * GRID_SPACING;
                _gridLines[i].transform.position = new Vector3(0f, baseY, 0f);
            }
        }

        // ── Side Ruins ──────────────────────────────────────────────

        private void CreateSideRuins()
        {
            _leftRuins = new SpriteRenderer[RUIN_COUNT];
            _rightRuins = new SpriteRenderer[RUIN_COUNT];

            for (int i = 0; i < RUIN_COUNT; i++)
            {
                _leftRuins[i] = CreateRuinBlock(-_halfWidth - 0.3f, i);
                _rightRuins[i] = CreateRuinBlock(_halfWidth + 0.3f, i);
            }
        }

        private SpriteRenderer CreateRuinBlock(float xPos, int index)
        {
            var go = new GameObject($"Ruin_{(xPos < 0 ? "L" : "R")}_{index}");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteSquare;
            // Vary the color slightly for each block
            float shade = Random.Range(0.04f, 0.08f);
            sr.color = new Color(shade, shade * 0.7f, shade * 1.2f, 0.4f);
            sr.sortingOrder = -9;

            // Vary width for ruined look
            float width = Random.Range(0.4f, 1.2f);
            float height = Random.Range(1.5f, 4f);
            go.transform.localScale = new Vector3(width, height, 1f);
            go.transform.position = new Vector3(xPos, 0, 0);

            return sr;
        }

        private void UpdateSideRuins(float camY)
        {
            for (int i = 0; i < RUIN_COUNT; i++)
            {
                // Wrap vertically
                float baseY = camY - (camY % RUIN_SPACING) + (i - RUIN_COUNT / 2) * RUIN_SPACING;

                if (_leftRuins[i] != null)
                    _leftRuins[i].transform.position = new Vector3(
                        -_halfWidth - 0.3f, baseY, 0f);

                if (_rightRuins[i] != null)
                    _rightRuins[i].transform.position = new Vector3(
                        _halfWidth + 0.3f, baseY, 0f);
            }
        }

        // ── Depth Markers ───────────────────────────────────────────

        private void CreateDepthMarker()
        {
            // Horizontal marker line
            var go = new GameObject("DepthMarker");
            go.transform.SetParent(transform);
            _depthMarker = go.AddComponent<SpriteRenderer>();
            _depthMarker.sprite = SpriteHelper.WhiteSquare;
            _depthMarker.color = new Color(0.3f, 0.2f, 0.5f, 0.3f);
            _depthMarker.sortingOrder = -7;
            go.transform.localScale = new Vector3(_halfWidth * 2.5f, 0.06f, 1f);
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
                float markerY = player.transform.position.y - 3f; // Slightly below player

                _depthMarker.gameObject.SetActive(true);
                _depthMarker.transform.position = new Vector3(0f, markerY, 0f);

                // Flash brighter then fade
                _depthMarker.color = new Color(0.5f, 0.3f, 0.8f, 0.6f);
            }

            // Fade the marker
            if (_depthMarker.gameObject.activeSelf)
            {
                var c = _depthMarker.color;
                c.a -= Time.deltaTime * 0.3f;
                if (c.a <= 0f)
                {
                    _depthMarker.gameObject.SetActive(false);
                }
                else
                {
                    _depthMarker.color = c;
                }
            }
        }

        // ── Environment Color Shift ─────────────────────────────────

        private void UpdateEnvironmentColor()
        {
            var player = PlayerController.Instance;
            if (player == null || _camera == null) return;

            float depth = player.DepthTraveled;

            // Shift background color based on depth zone (every 100m)
            // Zone 0 (0-100m): dark purple
            // Zone 1 (100-200m): dark blue
            // Zone 2 (200-300m): dark teal
            // Zone 3 (300-400m): dark crimson
            // Zone 4 (400m+): deep black-red
            int zone = Mathf.FloorToInt(depth / 100f);
            float zoneProgress = (depth % 100f) / 100f;

            Color fromColor = GetZoneColor(zone);
            Color toColor = GetZoneColor(zone + 1);
            Color bgColor = Color.Lerp(fromColor, toColor, zoneProgress);

            _camera.backgroundColor = bgColor;

            // Shift grid line color to match zone
            Color gridColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.08f, bgColor.b + 0.15f, 0.2f);
            foreach (var line in _gridLines)
            {
                if (line != null) line.color = gridColor;
            }
        }

        private Color GetZoneColor(int zone)
        {
            return (zone % 5) switch
            {
                0 => new Color(0.05f, 0.02f, 0.10f), // Dark purple
                1 => new Color(0.02f, 0.05f, 0.12f), // Dark blue
                2 => new Color(0.02f, 0.08f, 0.08f), // Dark teal
                3 => new Color(0.10f, 0.03f, 0.05f), // Dark crimson
                4 => new Color(0.08f, 0.02f, 0.02f), // Deep black-red
                _ => new Color(0.05f, 0.02f, 0.10f)
            };
        }
    }
}
