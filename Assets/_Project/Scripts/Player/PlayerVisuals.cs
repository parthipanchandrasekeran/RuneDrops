using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Player
{
    /// <summary>
    /// Living Arcane Orb — multi-layered glowing player with orbiting motes,
    /// inner core, speed lines, and power-reactive colors.
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        private SpriteRenderer _mainSprite;
        private SpriteRenderer _core;
        private SpriteRenderer _innerRing;
        private SpriteRenderer _glow;
        private SpriteRenderer _aura;
        private Transform[] _motes;
        private SpriteRenderer[] _moteRenderers;
        private Transform[] _trail;
        private SpriteRenderer[] _trailRenderers;
        private Transform[] _trailGlow;
        private SpriteRenderer[] _trailGlowRenderers;
        private Vector3[] _trailPositions;
        private SpriteRenderer[] _speedLines;
        private SpriteRenderer _shieldRing;

        private const int TRAIL_LENGTH = 10;
        private const int MOTE_COUNT = 2;
        private const int SPEED_LINE_COUNT = 4;

        private Color _baseColor = new Color(0.4f, 0.85f, 1f);
        private Color _targetColor;
        private float _moteAngle;
        private float _pulseTimer;
        private float _powerFlashTimer;
        private bool _hasPower;

        private void Start()
        {
            _mainSprite = GetComponent<SpriteRenderer>();
            if (_mainSprite == null) return;

            _mainSprite.color = _baseColor;
            _mainSprite.sortingOrder = 10;
            _targetColor = _baseColor;

            CreateCore();
            CreateInnerRing();
            CreateGlow();
            CreateAura();
            CreateMotes();
            CreateTrail();
            CreateSpeedLines();

            EventBus.Subscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Subscribe<RunePowerExpiredEvent>(OnPowerExpired);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<ComboExpiredEvent>(OnComboExpired);
            EventBus.Subscribe<PlayerStateChangedEvent>(OnStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Unsubscribe<RunePowerExpiredEvent>(OnPowerExpired);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<ComboExpiredEvent>(OnComboExpired);
            EventBus.Unsubscribe<PlayerStateChangedEvent>(OnStateChanged);
            CleanupTrail();
        }

        private void Update()
        {
            if (_mainSprite == null) return;
            _pulseTimer += Time.deltaTime;

            Color current = Color.Lerp(_mainSprite.color, _targetColor, Time.deltaTime * 5f);
            _mainSprite.color = current;

            UpdateCore();
            UpdateInnerRing();
            UpdateGlow(current);
            UpdateAura(current);
            UpdateMotes(current);
            UpdateTrail(current);
            UpdateSpeedLines();
            UpdatePowerFlash();
            UpdateShield();
        }

        // ── Create Layers ───────────────────────────────────────────

        private void CreateCore()
        {
            var go = new GameObject("PlayerCore");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _core = go.AddComponent<SpriteRenderer>();
            _core.sprite = SpriteHelper.WhiteCircle;
            _core.color = new Color(0.95f, 1f, 1f, 0.9f);
            _core.sortingOrder = 11;
            go.transform.localScale = Vector3.one * 0.35f;
        }

        private void CreateInnerRing()
        {
            var go = new GameObject("PlayerInnerRing");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _innerRing = go.AddComponent<SpriteRenderer>();
            _innerRing.sprite = SpriteHelper.WhiteCircle;
            _innerRing.color = new Color(0.2f, 0.5f, 0.8f, 0.2f);
            _innerRing.sortingOrder = 9;
            go.transform.localScale = Vector3.one * 1.15f;
        }

        private void CreateGlow()
        {
            var go = new GameObject("PlayerGlow");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _glow = go.AddComponent<SpriteRenderer>();
            _glow.sprite = SpriteHelper.WhiteCircle;
            _glow.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.1f);
            _glow.sortingOrder = 8;
            go.transform.localScale = Vector3.one * 2f;
        }

        private void CreateAura()
        {
            var go = new GameObject("PlayerAura");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _aura = go.AddComponent<SpriteRenderer>();
            _aura.sprite = SpriteHelper.WhiteCircle;
            _aura.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.04f);
            _aura.sortingOrder = 7;
            go.transform.localScale = Vector3.one * 3.5f;
        }

        private void CreateMotes()
        {
            _motes = new Transform[MOTE_COUNT];
            _moteRenderers = new SpriteRenderer[MOTE_COUNT];
            for (int i = 0; i < MOTE_COUNT; i++)
            {
                var go = new GameObject($"Mote_{i}");
                go.transform.SetParent(transform);
                _motes[i] = go.transform;
                _moteRenderers[i] = go.AddComponent<SpriteRenderer>();
                _moteRenderers[i].sprite = SpriteHelper.WhiteCircle;
                _moteRenderers[i].color = new Color(0.7f, 0.95f, 1f, 0.4f);
                _moteRenderers[i].sortingOrder = 11;
                go.transform.localScale = Vector3.one * 0.1f;
            }
        }

        private void CreateTrail()
        {
            _trail = new Transform[TRAIL_LENGTH];
            _trailRenderers = new SpriteRenderer[TRAIL_LENGTH];
            _trailGlow = new Transform[TRAIL_LENGTH];
            _trailGlowRenderers = new SpriteRenderer[TRAIL_LENGTH];
            _trailPositions = new Vector3[TRAIL_LENGTH];

            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                // Main trail segment
                var go = new GameObject($"Trail_{i}");
                go.transform.position = transform.position;
                _trail[i] = go.transform;
                _trailRenderers[i] = go.AddComponent<SpriteRenderer>();
                _trailRenderers[i].sprite = SpriteHelper.WhiteCircle;
                _trailRenderers[i].sortingOrder = 8;

                float t = (float)i / TRAIL_LENGTH;
                float alpha = (1f - t) * 0.18f;
                float scale = (1f - t) * 0.55f;
                _trailRenderers[i].color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
                go.transform.localScale = Vector3.one * scale;

                // Trail glow (larger, fainter behind each segment)
                var glowGO = new GameObject($"TrailGlow_{i}");
                glowGO.transform.position = transform.position;
                _trailGlow[i] = glowGO.transform;
                _trailGlowRenderers[i] = glowGO.AddComponent<SpriteRenderer>();
                _trailGlowRenderers[i].sprite = SpriteHelper.WhiteCircle;
                _trailGlowRenderers[i].sortingOrder = 6;
                _trailGlowRenderers[i].color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha * 0.4f);
                glowGO.transform.localScale = Vector3.one * scale * 1.8f;

                _trailPositions[i] = transform.position;
            }
        }

        private void CreateSpeedLines()
        {
            _speedLines = new SpriteRenderer[SPEED_LINE_COUNT];
            float[] xOffsets = { -0.5f, -0.2f, 0.25f, 0.45f };
            float[] heights = { 0.5f, 0.7f, 0.4f, 0.6f };

            for (int i = 0; i < SPEED_LINE_COUNT; i++)
            {
                var go = new GameObject($"SpeedLine_{i}");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(xOffsets[i], heights[i], 0);
                _speedLines[i] = go.AddComponent<SpriteRenderer>();
                _speedLines[i].sprite = SpriteHelper.WhiteSquare;
                _speedLines[i].color = new Color(0.4f, 0.7f, 1f, 0f);
                _speedLines[i].sortingOrder = 7;
                go.transform.localScale = new Vector3(0.015f, 0.5f, 1f);
            }
        }

        // ── Update Layers ───────────────────────────────────────────

        private void UpdateCore()
        {
            if (_core == null) return;
            float pulse = 0.33f + Mathf.Sin(_pulseTimer * 4f) * 0.04f;
            _core.transform.localScale = Vector3.one * pulse;
        }

        private void UpdateInnerRing()
        {
            if (_innerRing == null) return;
            _innerRing.transform.Rotate(0, 0, -30f * Time.deltaTime);
        }

        private void UpdateGlow(Color c)
        {
            if (_glow == null) return;
            float scale = 2f + Mathf.Sin(_pulseTimer * 3f) * 0.25f;
            _glow.transform.localScale = Vector3.one * scale;
            _glow.color = new Color(c.r, c.g, c.b, 0.1f + Mathf.Sin(_pulseTimer * 2f) * 0.03f);
        }

        private void UpdateAura(Color c)
        {
            if (_aura == null) return;
            float scale = 3.5f + Mathf.Sin(_pulseTimer * 1.5f) * 0.4f;
            _aura.transform.localScale = Vector3.one * scale;
            _aura.color = new Color(c.r, c.g, c.b, _hasPower ? 0.06f : 0.03f);
        }

        private void UpdateMotes(Color c)
        {
            if (_motes == null) return;
            float speed = _hasPower ? 200f : 120f;
            _moteAngle += speed * Time.deltaTime;

            for (int i = 0; i < MOTE_COUNT; i++)
            {
                if (_motes[i] == null) continue;
                float angle = (_moteAngle + i * 180f) * Mathf.Deg2Rad;
                float radius = 0.45f;
                _motes[i].localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                _moteRenderers[i].color = new Color(c.r, c.g, c.b, 0.4f);
            }
        }

        private void UpdateTrail(Color c)
        {
            if (_trail == null) return;

            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
                _trailPositions[i] = Vector3.Lerp(_trailPositions[i], _trailPositions[i - 1], 0.35f);
            _trailPositions[0] = transform.position;

            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                if (_trail[i] == null) continue;
                _trail[i].position = _trailPositions[i];
                float t = (float)i / TRAIL_LENGTH;
                float alpha = (1f - t) * 0.15f;
                _trailRenderers[i].color = new Color(c.r, c.g, c.b, alpha);

                if (_trailGlow[i] != null)
                {
                    _trailGlow[i].position = _trailPositions[i];
                    _trailGlowRenderers[i].color = new Color(c.r, c.g, c.b, alpha * 0.35f);
                }
            }
        }

        private void UpdateSpeedLines()
        {
            if (_speedLines == null) return;
            var pc = PlayerController.Instance;
            float speed = pc != null ? pc.CurrentFallSpeed : 0f;
            float intensity = Mathf.InverseLerp(6f, 12f, speed);

            for (int i = 0; i < SPEED_LINE_COUNT; i++)
            {
                if (_speedLines[i] == null) continue;
                var c = _speedLines[i].color;
                c.a = Mathf.Lerp(c.a, intensity * 0.25f, Time.deltaTime * 3f);
                _speedLines[i].color = c;
            }
        }

        private void UpdatePowerFlash()
        {
            if (_powerFlashTimer > 0f)
            {
                _powerFlashTimer -= Time.deltaTime;
                if (_core != null)
                    _core.color = Color.Lerp(new Color(0.95f, 1f, 1f, 0.9f), Color.white, _powerFlashTimer / 0.2f);
            }
        }

        private void UpdateShield()
        {
            if (_shieldRing == null) return;
            float pulse = 1.8f + Mathf.Sin(_pulseTimer * 5f) * 0.15f;
            _shieldRing.transform.localScale = Vector3.one * pulse;
        }

        // ── Events ──────────────────────────────────────────────────

        private void OnPowerActivated(RunePowerActivatedEvent evt)
        {
            _hasPower = true;
            _powerFlashTimer = 0.2f;
            _targetColor = evt.Type switch
            {
                1 => new Color(1f, 0.5f, 0f),
                2 => new Color(0.3f, 1f, 1f),
                3 => new Color(0.7f, 0.2f, 1f),
                4 => new Color(0.2f, 1f, 0.3f),
                _ => _baseColor
            };

            // Show shield ring for Earth
            if (evt.Type == 4) CreateShieldRing();
        }

        private void OnPowerExpired(RunePowerExpiredEvent evt)
        {
            _hasPower = false;
            _targetColor = _baseColor;
            DestroyShieldRing();
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            _hasPower = true;
            _powerFlashTimer = 0.3f;
            _targetColor = new Color(1f, 0.8f, 0.2f);

            // Flash aura
            if (_aura != null)
            {
                _aura.transform.localScale = Vector3.one * 5.5f;
                _aura.color = new Color(1f, 0.8f, 0.2f, 0.15f);
            }
        }

        private void OnComboExpired(ComboExpiredEvent evt)
        {
            _hasPower = false;
            _targetColor = _baseColor;
        }

        private void OnStateChanged(PlayerStateChangedEvent evt)
        {
            // Show/hide shield ring based on state
            if (evt.NewState == (int)PlayerState.Shielded)
                CreateShieldRing();
            else if (evt.OldState == (int)PlayerState.Shielded)
                DestroyShieldRing();

            // Shadow phase — make translucent
            if (evt.NewState == (int)PlayerState.Phasing && _mainSprite != null)
                _mainSprite.color = new Color(_mainSprite.color.r, _mainSprite.color.g, _mainSprite.color.b, 0.4f);
        }

        private void CreateShieldRing()
        {
            if (_shieldRing != null) return;
            var go = new GameObject("ShieldRing");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _shieldRing = go.AddComponent<SpriteRenderer>();
            _shieldRing.sprite = SpriteHelper.WhiteCircle;
            _shieldRing.color = new Color(0.3f, 1f, 0.3f, 0.18f);
            _shieldRing.sortingOrder = 11;
            go.transform.localScale = Vector3.one * 1.8f;
        }

        private void DestroyShieldRing()
        {
            if (_shieldRing != null)
            {
                Destroy(_shieldRing.gameObject);
                _shieldRing = null;
            }
        }

        private void CleanupTrail()
        {
            if (_trail != null)
                foreach (var t in _trail) if (t != null) Destroy(t.gameObject);
            if (_trailGlow != null)
                foreach (var t in _trailGlow) if (t != null) Destroy(t.gameObject);
        }
    }
}
