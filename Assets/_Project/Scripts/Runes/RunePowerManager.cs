using System.Collections;
using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;
using RuneDrop.Player;

namespace RuneDrop.Runes
{
    /// <summary>
    /// Activates rune combo effects when combos are detected.
    /// Subscribes to ComboActivatedEvent from EventBus.
    /// </summary>
    public class RunePowerManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static RunePowerManager Instance { get; private set; }

        // ── Config ──────────────────────────────────────────────────
        private GameConfigSO _config;
        private Coroutine _activeComboCoroutine;

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

        private void Start()
        {
            _config = Resources.Load<GameConfigSO>("Configs/GameConfig");
            if (_config == null) Debug.LogError("[RunePowerManager] GameConfig missing!");
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<RunePowerActivatedEvent>(OnSinglePowerActivated);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<RunePowerActivatedEvent>(OnSinglePowerActivated);
            if (Instance == this)
            {
                ServiceLocator.Unregister(this);
                Instance = null;
            }
        }

        // ── Event Handler ───────────────────────────────────────────

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var combo = (ComboType)evt.Combo;
            ActivateCombo(combo);
        }

        private void OnSinglePowerActivated(RunePowerActivatedEvent evt)
        {
            var type = (RuneType)evt.Type;
            ActivateSinglePower(type, evt.Duration);
        }

        // ── Single Rune Powers ──────────────────────────────────────

        private Coroutine _activeSingleCoroutine;

        public void ActivateSinglePower(RuneType type, float duration)
        {
            if (_activeSingleCoroutine != null)
                StopCoroutine(_activeSingleCoroutine);

            _activeSingleCoroutine = type switch
            {
                RuneType.Fire => StartCoroutine(FirePower(duration)),
                RuneType.Wind => StartCoroutine(WindPower(duration)),
                RuneType.Shadow => StartCoroutine(ShadowPower(duration)),
                RuneType.Earth => StartCoroutine(EarthPower()),
                _ => null
            };
        }

        private IEnumerator FirePower(float duration)
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log($"[RunePower] FIRE active for {duration}s — destroying nearby obstacles");
            float elapsed = 0f;
            while (elapsed < duration && player.IsAlive)
            {
                elapsed += Time.deltaTime;
                var hits = Physics2D.OverlapCircleAll(player.transform.position, 1.5f);
                foreach (var hit in hits)
                {
                    if (hit.gameObject.layer == 7)
                        Destroy(hit.gameObject);
                }
                yield return null;
            }
            EventBus.Publish(new RunePowerExpiredEvent { Type = (int)RuneType.Fire });
            _activeSingleCoroutine = null;
        }

        private IEnumerator WindPower(float duration)
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log($"[RunePower] WIND active for {duration}s — slowing fall");
            player.SetFallSpeedMultiplier(0.5f);
            yield return new WaitForSeconds(duration);
            player.ResetFallSpeedMultiplier();
            EventBus.Publish(new RunePowerExpiredEvent { Type = (int)RuneType.Wind });
            _activeSingleCoroutine = null;
        }

        private IEnumerator ShadowPower(float duration)
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log($"[RunePower] SHADOW active for {duration}s — phasing");
            player.TransitionToState(PlayerState.Phasing);
            yield return new WaitForSeconds(duration);
            if (player.CurrentState == PlayerState.Phasing)
                player.TransitionToState(PlayerState.Falling);
            EventBus.Publish(new RunePowerExpiredEvent { Type = (int)RuneType.Shadow });
            _activeSingleCoroutine = null;
        }

        private IEnumerator EarthPower()
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log("[RunePower] EARTH — shield active");
            player.TransitionToState(PlayerState.Shielded);
            // Shield persists until hit (handled by PlayerController.TakeDamage)
            EventBus.Publish(new RunePowerExpiredEvent { Type = (int)RuneType.Earth });
            _activeSingleCoroutine = null;
            yield break;
        }

        // ── Combo Activation ────────────────────────────────────────

        public void ActivateCombo(ComboType combo)
        {
            // Cancel any active combo
            if (_activeComboCoroutine != null)
            {
                StopCoroutine(_activeComboCoroutine);
            }

            float duration = _config.ComboDuration;
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                duration += save.Data.UpgradeComboExtend * _config.ComboExtensionPerUpgrade;
            }

            _activeComboCoroutine = combo switch
            {
                ComboType.FlameTrail => StartCoroutine(FlameTrailCombo(duration)),
                ComboType.BlinkDash => StartCoroutine(BlinkDashCombo()),
                ComboType.ExplosiveShield => StartCoroutine(ExplosiveShieldCombo(duration)),
                _ => null
            };
        }

        // ── Flame Trail (Fire + Wind) ───────────────────────────────
        // Slow fall + destroy nearby obstacles

        private IEnumerator FlameTrailCombo(float duration)
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log($"[RunePower] Flame Trail active for {duration}s");

            EventBus.Publish(new RunePowerActivatedEvent
            {
                Type = (int)ComboType.FlameTrail,
                Duration = duration
            });

            // Slow fall slightly
            player.SetFallSpeedMultiplier(0.6f);

            float elapsed = 0f;
            float destroyRadius = 2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Destroy nearby obstacles
                var hits = Physics2D.OverlapCircleAll(player.transform.position, destroyRadius);
                foreach (var hit in hits)
                {
                    if (hit.gameObject.layer == 7) // Obstacle layer
                    {
                        Destroy(hit.gameObject);
                    }
                }

                yield return null;
            }

            player.ResetFallSpeedMultiplier();

            EventBus.Publish(new ComboExpiredEvent { Combo = (int)ComboType.FlameTrail });
            Debug.Log("[RunePower] Flame Trail expired");
            _activeComboCoroutine = null;
        }

        // ── Blink Dash (Shadow + Wind) ──────────────────────────────
        // Instant teleport downward through obstacles

        private IEnumerator BlinkDashCombo()
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log("[RunePower] Blink Dash!");

            EventBus.Publish(new RunePowerActivatedEvent
            {
                Type = (int)ComboType.BlinkDash,
                Duration = 0.5f
            });

            // Teleport down 5 units
            float dashDistance = 5f;
            player.SetInvincible(1f);
            player.transform.Translate(0f, -dashDistance, 0f);

            yield return new WaitForSeconds(0.5f);

            EventBus.Publish(new ComboExpiredEvent { Combo = (int)ComboType.BlinkDash });
            _activeComboCoroutine = null;
        }

        // ── Explosive Shield (Earth + Fire) ─────────────────────────
        // Shield that explodes on hit, destroying nearby obstacles

        private IEnumerator ExplosiveShieldCombo(float duration)
        {
            var player = PlayerController.Instance;
            if (player == null) yield break;

            Debug.Log($"[RunePower] Explosive Shield active for {duration}s");

            EventBus.Publish(new RunePowerActivatedEvent
            {
                Type = (int)ComboType.ExplosiveShield,
                Duration = duration
            });

            player.TransitionToState(PlayerState.Shielded);

            // Wait for shield to break or duration to expire
            float elapsed = 0f;
            while (elapsed < duration && player.CurrentState == PlayerState.Shielded)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // If shield broke (state changed from Shielded), trigger explosion
            if (player.CurrentState != PlayerState.Shielded)
            {
                Debug.Log("[RunePower] Explosive Shield detonated!");
                float explosionRadius = 4f;
                var hits = Physics2D.OverlapCircleAll(player.transform.position, explosionRadius);
                foreach (var hit in hits)
                {
                    if (hit.gameObject.layer == 7)
                    {
                        Destroy(hit.gameObject);
                    }
                }
            }
            else
            {
                // Duration expired without hit
                player.TransitionToState(PlayerState.Falling);
            }

            EventBus.Publish(new ComboExpiredEvent { Combo = (int)ComboType.ExplosiveShield });
            Debug.Log("[RunePower] Explosive Shield expired");
            _activeComboCoroutine = null;
        }
    }
}
