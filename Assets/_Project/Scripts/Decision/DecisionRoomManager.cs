using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;
using RuneDrop.Player;
using RuneDrop.Runes;
using RuneDrop.Anchor;

namespace RuneDrop.Decision
{
    /// <summary>
    /// Spawns decision rooms at intervals during gameplay.
    /// Each room presents 2 choices — safe upgrade vs risky reward.
    /// </summary>
    public class DecisionRoomManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static DecisionRoomManager Instance { get; private set; }

        // ── Config ──────────────────────────────────────────────────
        private GameConfigSO _config;

        // ── State ───────────────────────────────────────────────────
        private float _timeSinceLastRoom;
        private bool _roomActive;
        private DecisionRoom _activeRoom;
        private int _roomsSpawned;

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
            _timeSinceLastRoom = 0f;
            _roomsSpawned = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<DecisionRoomManager>();
                Instance = null;
            }
        }

        private void Update()
        {
            if (_roomActive) return;

            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive) return;

            _timeSinceLastRoom += Time.deltaTime;

            if (_timeSinceLastRoom >= _config.DecisionRoomInterval)
            {
                SpawnDecisionRoom();
                _timeSinceLastRoom = 0f;
            }
        }

        // ── Spawn Room ──────────────────────────────────────────────

        private void SpawnDecisionRoom()
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            float roomY = player.transform.position.y - 15f; // Spawn ahead

            var roomGO = new GameObject($"DecisionRoom_{_roomsSpawned}");
            roomGO.transform.position = new Vector3(0f, roomY, 0f);

            _activeRoom = roomGO.AddComponent<DecisionRoom>();
            _activeRoom.Initialize(this, PickChoices());
            _roomActive = true;
            _roomsSpawned++;

            EventBus.Publish(new DecisionRoomAppearedEvent());
            Debug.Log($"[Decision] Room spawned at Y={roomY:F0}");
        }

        // ── Choice Selection ────────────────────────────────────────

        private DecisionChoice[] PickChoices()
        {
            // Always pair: one safe + one risky
            var safe = GetRandomSafeChoice();
            var risky = GetRandomRiskyChoice();
            return new[] { safe, risky };
        }

        private DecisionChoice GetRandomSafeChoice()
        {
            int pick = Random.Range(0, 4);
            return pick switch
            {
                0 => new DecisionChoice
                {
                    Id = "grant_anchor",
                    Name = "+1 Anchor",
                    Description = "Refund an anchor charge",
                    IsSafe = true,
                    Color = new Color(0.2f, 0.6f, 1f),
                    Apply = () =>
                    {
                        AnchorController.Instance?.RefundCharge();
                        Debug.Log("[Decision] Granted anchor charge");
                    }
                },
                1 => new DecisionChoice
                {
                    Id = "slow_fall",
                    Name = "Slow Fall",
                    Description = "Reduce speed for 10s",
                    IsSafe = true,
                    Color = new Color(0f, 0.8f, 1f),
                    Apply = () =>
                    {
                        var player = PlayerController.Instance;
                        if (player != null)
                        {
                            player.SetFallSpeedMultiplier(0.6f);
                            Instance.StartCoroutine(ResetSpeedAfter(10f));
                        }
                        Debug.Log("[Decision] Slow fall applied for 10s");
                    }
                },
                2 => new DecisionChoice
                {
                    Id = "grant_shield",
                    Name = "Earth Shield",
                    Description = "Absorb next hit",
                    IsSafe = true,
                    Color = new Color(0.2f, 0.8f, 0.1f),
                    Apply = () =>
                    {
                        var player = PlayerController.Instance;
                        if (player != null) player.TransitionToState(PlayerState.Shielded);
                        Debug.Log("[Decision] Shield granted");
                    }
                },
                _ => new DecisionChoice
                {
                    Id = "clear_path",
                    Name = "Clear Path",
                    Description = "Destroy nearby obstacles",
                    IsSafe = true,
                    Color = new Color(1f, 1f, 0.3f),
                    Apply = () =>
                    {
                        var player = PlayerController.Instance;
                        if (player == null) return;
                        var hits = Physics2D.OverlapCircleAll(player.transform.position, 6f);
                        foreach (var hit in hits)
                        {
                            if (hit.gameObject.layer == 7)
                                Destroy(hit.gameObject);
                        }
                        Debug.Log("[Decision] Path cleared");
                    }
                }
            };
        }

        private DecisionChoice GetRandomRiskyChoice()
        {
            int pick = Random.Range(0, 2);
            return pick switch
            {
                0 => new DecisionChoice
                {
                    Id = "speed_rush",
                    Name = "Speed Rush",
                    Description = "2x speed, +50 shards if survive 5s",
                    IsSafe = false,
                    Color = new Color(1f, 0.3f, 0f),
                    Apply = () =>
                    {
                        var player = PlayerController.Instance;
                        if (player != null)
                        {
                            player.SetFallSpeedMultiplier(2f);
                            Instance.StartCoroutine(SpeedRushTimer(5f, 50));
                        }
                        Debug.Log("[Decision] Speed Rush! Good luck...");
                    }
                },
                _ => new DecisionChoice
                {
                    Id = "double_runes",
                    Name = "Rune Frenzy",
                    Description = "More runes + more obstacles",
                    IsSafe = false,
                    Color = new Color(0.9f, 0f, 0.9f),
                    Apply = () =>
                    {
                        Debug.Log("[Decision] Rune Frenzy activated");
                    }
                }
            };
        }

        // ── Room Complete ───────────────────────────────────────────

        public void OnChoiceMade(DecisionChoice choice)
        {
            choice.Apply?.Invoke();
            _roomActive = false;

            EventBus.Publish(new DecisionMadeEvent
            {
                ChoiceId = choice.Id,
                Category = choice.IsSafe ? 0 : 1
            });

            if (_activeRoom != null)
            {
                Destroy(_activeRoom.gameObject);
                _activeRoom = null;
            }

            Debug.Log($"[Decision] Player chose: {choice.Name} ({(choice.IsSafe ? "Safe" : "Risky")})");
        }

        // ── Timer Coroutines ────────────────────────────────────────

        private static System.Collections.IEnumerator ResetSpeedAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            var player = PlayerController.Instance;
            if (player != null && player.IsAlive)
            {
                player.ResetFallSpeedMultiplier();
                Debug.Log("[Decision] Slow fall expired");
            }
        }

        private static System.Collections.IEnumerator SpeedRushTimer(float seconds, int shardReward)
        {
            yield return new WaitForSeconds(seconds);
            var player = PlayerController.Instance;
            if (player != null && player.IsAlive)
            {
                player.ResetFallSpeedMultiplier();
                // Survived! Award shards
                if (ServiceLocator.TryGet<SaveSystem>(out var save))
                {
                    save.Data.SoulShards += shardReward;
                    save.Save();
                    Debug.Log($"[Decision] Speed Rush survived! +{shardReward} shards");
                }
            }
            else
            {
                Debug.Log("[Decision] Speed Rush failed — player died");
            }
        }
    }

    // ── Decision Choice Data ────────────────────────────────────────

    public class DecisionChoice
    {
        public string Id;
        public string Name;
        public string Description;
        public bool IsSafe;
        public Color Color;
        public System.Action Apply;
    }
}
