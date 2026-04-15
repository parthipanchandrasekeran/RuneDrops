using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;

namespace RuneDrop.Player
{
    /// <summary>
    /// Core player controller. Handles fall physics, horizontal movement,
    /// state management, and death/revive logic.
    /// </summary>
    [RequireComponent(typeof(TouchInputHandler))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static PlayerController Instance { get; private set; }

        // ── Configuration ───────────────────────────────────────────
        [Header("Config")]
        [SerializeField] private GameConfigSO _config;

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        // ── State ───────────────────────────────────────────────────
        private PlayerState _currentState = PlayerState.Falling;
        private TouchInputHandler _input;

        private float _currentFallSpeed;
        private float _fallSpeedMultiplier = 1f;
        private float _targetXPosition;
        private float _currentXVelocity;
        private float _depthTraveled;
        private float _runDuration;
        private bool _isAlive;
        private bool _isInvincible;
        private float _invincibilityTimer;
        private int _lastDepthMilestone = -1;

        // Horizontal bounds
        private float _leftBound = -4f;
        private float _rightBound = 4f;

        // ── Properties ──────────────────────────────────────────────
        public float DepthTraveled => _depthTraveled;
        public float CurrentFallSpeed => _currentFallSpeed * _fallSpeedMultiplier;
        public PlayerState CurrentState => _currentState;
        public bool IsAlive => _isAlive;
        public float RunDuration => _runDuration;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            gameObject.tag = "Player";

            ServiceLocator.Register(this);
            _input = GetComponent<TouchInputHandler>();

            // Ensure Rigidbody2D is kinematic (required for trigger collisions)
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) { rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; }
        }

        private void Start()
        {
            // Auto-load config from Resources if not assigned in Inspector
            if (_config == null)
            {
                _config = Resources.Load<GameConfigSO>("Configs/GameConfig");
            }
            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister(this);
                Instance = null;
            }
        }

        private void Update()
        {
            if (!_isAlive || _currentState == PlayerState.Dead) return;

            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentState != GameState.Playing) return;

            _runDuration += Time.deltaTime;
            UpdateFall();
            UpdateHorizontalMovement();
            UpdateInvincibility();
            UpdateBounds();

            // Sync run data to GameManager
            if (gm != null)
            {
                gm.CurrentRunDepth = _depthTraveled;
                gm.CurrentRunDuration = _runDuration;
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        public void Initialize()
        {
            if (_config == null)
            {
                Debug.LogError("[PlayerController] GameConfigSO not assigned!");
                return;
            }

            _isAlive = true;
            _currentState = PlayerState.Falling;
            _currentFallSpeed = _config.BaseFallSpeed;
            _fallSpeedMultiplier = 1f;
            _depthTraveled = 0f;
            _runDuration = 0f;
            _targetXPosition = transform.position.x;
            _isInvincible = false;
            _invincibilityTimer = 0f;

            // Apply slow fall upgrade
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                float reduction = save.Data.UpgradeSlowFall * _config.SlowFallReductionPerLevel;
                _currentFallSpeed *= (1f - reduction);
            }

            // Wire input
            _input.OnDragPosition = OnDragPosition;

            Debug.Log("[PlayerController] Initialized");
        }

        // ── Fall Physics ────────────────────────────────────────────

        private void UpdateFall()
        {
            // Accelerate fall speed
            _currentFallSpeed = Mathf.Min(
                _currentFallSpeed + _config.FallAcceleration * Time.deltaTime,
                _config.MaxFallSpeed
            );

            // Apply fall
            float fallDelta = _currentFallSpeed * _fallSpeedMultiplier * Time.deltaTime;
            transform.Translate(0f, -fallDelta, 0f);
            _depthTraveled += fallDelta;

            // Publish score update once per 10m milestone
            int milestone = Mathf.FloorToInt(_depthTraveled / 10f);
            if (milestone > _lastDepthMilestone)
            {
                _lastDepthMilestone = milestone;
                EventBus.Publish(new ScoreChangedEvent
                {
                    NewDepth = _depthTraveled,
                    NewRuneCount = GameManager.Instance?.CurrentRunRunes ?? 0
                });
            }
        }

        // ── Horizontal Movement ─────────────────────────────────────

        private void OnDragPosition(float worldX)
        {
            _targetXPosition = Mathf.Clamp(worldX, _leftBound, _rightBound);
        }

        private void UpdateHorizontalMovement()
        {
            float newX = Mathf.SmoothDamp(
                transform.position.x,
                _targetXPosition,
                ref _currentXVelocity,
                _config.HorizontalSmoothing
            );

            newX = Mathf.Clamp(newX, _leftBound, _rightBound);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        // ── Bounds ──────────────────────────────────────────────────

        private void UpdateBounds()
        {
            var cam = Camera.main;
            if (cam == null) return;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            _leftBound = -halfWidth + _config.PlayerColliderRadius;
            _rightBound = halfWidth - _config.PlayerColliderRadius;
        }

        // ── Invincibility ───────────────────────────────────────────

        private void UpdateInvincibility()
        {
            if (!_isInvincible) return;

            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0f)
            {
                _isInvincible = false;
                SetSpriteAlpha(1f);
            }
            else
            {
                // Blink effect
                float alpha = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? 1f : 0.3f;
                SetSpriteAlpha(alpha);
            }
        }

        // ── Fall Speed Modifier ─────────────────────────────────────

        public void SetFallSpeedMultiplier(float multiplier)
        {
            _fallSpeedMultiplier = multiplier;
        }

        public void ResetFallSpeedMultiplier()
        {
            _fallSpeedMultiplier = 1f;
        }

        // ── State Transitions ───────────────────────────────────────

        public void TransitionToState(PlayerState newState)
        {
            var oldState = _currentState;
            _currentState = newState;

            EventBus.Publish(new PlayerStateChangedEvent
            {
                OldState = (int)oldState,
                NewState = (int)newState
            });
        }

        // ── Damage / Death / Revive ─────────────────────────────────

        public void TakeDamage()
        {
            if (!_isAlive || _isInvincible) return;
            if (_currentState == PlayerState.Phasing) return;

            if (_currentState == PlayerState.Shielded)
            {
                // Shield absorbs the hit
                TransitionToState(PlayerState.Falling);
                SetInvincible(1f);
                return;
            }

            Kill();
        }

        public void Kill()
        {
            if (!_isAlive) return;

            _isAlive = false;
            TransitionToState(PlayerState.Dead);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndRun();
            }

            Debug.Log($"[PlayerController] Died at depth {_depthTraveled:F0}m");
        }

        public void Revive(float immuneDuration = 2f)
        {
            _isAlive = true;
            TransitionToState(PlayerState.Falling);
            SetInvincible(immuneDuration);
            Debug.Log("[PlayerController] Revived");
        }

        public void SetInvincible(float duration)
        {
            _isInvincible = true;
            _invincibilityTimer = duration;
        }

        // ── Helpers ─────────────────────────────────────────────────

        private void SetSpriteAlpha(float alpha)
        {
            if (_spriteRenderer == null) return;
            var c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
        }
    }
}
