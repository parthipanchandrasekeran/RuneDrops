using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;
using RuneDrop.Player;

namespace RuneDrop.Anchor
{
    /// <summary>
    /// Tap-to-anchor mechanic. Slows fall speed temporarily.
    /// Limited charges per run, upgraded via meta progression.
    /// </summary>
    public class AnchorController : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static AnchorController Instance { get; private set; }

        // ── Configuration ───────────────────────────────────────────
        private GameConfigSO _config;

        // ── State ───────────────────────────────────────────────────
        private int _maxCharges;
        private int _currentCharges;
        private bool _isAnchored;
        private float _anchorTimer;
        private float _cooldownTimer;

        // ── Properties ──────────────────────────────────────────────
        public int CurrentCharges => _currentCharges;
        public int MaxCharges => _maxCharges;
        public bool IsAnchored => _isAnchored;

        // ── Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private bool _inputWired;

        private void Start()
        {
            _config = Resources.Load<GameConfigSO>("Configs/GameConfig");
            Initialize();
            TryWireInput();
        }

        private void TryWireInput()
        {
            if (_inputWired) return;
            var player = PlayerController.Instance;
            if (player != null)
            {
                var input = player.GetComponent<TouchInputHandler>();
                if (input != null)
                {
                    input.OnTap += TryAnchor;
                    _inputWired = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<AnchorController>();
                Instance = null;
            }

            // Unwire
            var player = PlayerController.Instance;
            if (player != null)
            {
                var input = player.GetComponent<TouchInputHandler>();
                if (input != null)
                {
                    input.OnTap -= TryAnchor;
                }
            }
        }

        private void Update()
        {
            // Retry input wiring if not done yet
            if (!_inputWired) TryWireInput();

            // Cooldown
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            // Anchor duration
            if (_isAnchored)
            {
                _anchorTimer -= Time.deltaTime;
                if (_anchorTimer <= 0f)
                {
                    EndAnchor();
                }
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        public void Initialize()
        {
            int upgradeBonus = 0;
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                upgradeBonus = save.Data.UpgradeAnchorCharges;
            }

            _maxCharges = _config.BaseAnchorCharges + upgradeBonus;
            _currentCharges = _maxCharges;
            _isAnchored = false;
            _anchorTimer = 0f;
            _cooldownTimer = 0f;

            Debug.Log($"[Anchor] Initialized with {_maxCharges} charges");
        }

        // ── Anchor ──────────────────────────────────────────────────

        public void TryAnchor()
        {
            if (_currentCharges <= 0) return;
            if (_isAnchored) return;
            if (_cooldownTimer > 0f) return;
            if (Time.timeScale == 0f) return; // Don't anchor during pause/tutorial

            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive) return;

            _currentCharges--;
            _isAnchored = true;
            _anchorTimer = _config.AnchorDuration;

            // Slow the player
            player.SetFallSpeedMultiplier(_config.AnchorFallSpeedMultiplier);
            player.TransitionToState(PlayerState.Anchored);

            EventBus.Publish(new AnchorUsedEvent
            {
                ChargesRemaining = _currentCharges,
                MaxCharges = _maxCharges
            });

            Debug.Log($"[Anchor] Used! Charges: {_currentCharges}/{_maxCharges}");
        }

        private void EndAnchor()
        {
            _isAnchored = false;
            _cooldownTimer = _config.AnchorCooldown;

            var player = PlayerController.Instance;
            if (player != null && player.IsAlive)
            {
                player.ResetFallSpeedMultiplier();
                player.TransitionToState(PlayerState.Falling);
            }

            EventBus.Publish(new AnchorExpiredEvent());
            Debug.Log("[Anchor] Expired, resuming fall");
        }

        // ── Utility ─────────────────────────────────────────────────

        public void RefundCharge()
        {
            _currentCharges = Mathf.Min(_currentCharges + 1, _maxCharges);
        }
    }
}
