using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Runes
{
    /// <summary>
    /// 2-slot rune inventory with auto combo detection.
    /// Collecting a rune fills slots A then B, then shifts.
    /// When both slots are full, combo is checked automatically.
    /// </summary>
    public class RuneInventory : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static RuneInventory Instance { get; private set; }

        // ── State ───────────────────────────────────────────────────
        private RuneType _slotA = RuneType.None;
        private RuneType _slotB = RuneType.None;
        private int _totalRunesCollected;

        // ── Properties ──────────────────────────────────────────────
        public RuneType SlotA => _slotA;
        public RuneType SlotB => _slotB;
        public int TotalRunesCollected => _totalRunesCollected;

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<RuneInventory>();
                Instance = null;
            }
        }

        // ── Collect ─────────────────────────────────────────────────

        public void CollectRune(RuneType type)
        {
            if (type == RuneType.None) return;

            _totalRunesCollected++;

            // Update GameManager rune count
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentRunRunes = _totalRunesCollected;
            }

            if (_slotA == RuneType.None)
            {
                _slotA = type;
                EventBus.Publish(new RuneCollectedEvent
                {
                    Type = (int)type,
                    SlotIndex = 0,
                    TotalCollected = _totalRunesCollected
                });
                Debug.Log($"[Runes] Collected {type} -> SlotA");
            }
            else if (_slotB == RuneType.None)
            {
                _slotB = type;
                EventBus.Publish(new RuneCollectedEvent
                {
                    Type = (int)type,
                    SlotIndex = 1,
                    TotalCollected = _totalRunesCollected
                });
                Debug.Log($"[Runes] Collected {type} -> SlotB");
                CheckAndActivateCombo();
            }
            else
            {
                // Both full — shift B out, new goes to B
                _slotA = _slotB;
                _slotB = type;
                EventBus.Publish(new RuneCollectedEvent
                {
                    Type = (int)type,
                    SlotIndex = 1,
                    TotalCollected = _totalRunesCollected
                });
                Debug.Log($"[Runes] Collected {type} -> SlotB (shifted)");
                CheckAndActivateCombo();
            }
        }

        // ── Combo ───────────────────────────────────────────────────

        private void CheckAndActivateCombo()
        {
            var combo = ComboDetector.Detect(_slotA, _slotB);
            if (combo != ComboType.None)
            {
                Debug.Log($"[Runes] COMBO: {combo}!");
                EventBus.Publish(new ComboActivatedEvent
                {
                    Combo = (int)combo,
                    RuneA = (int)_slotA,
                    RuneB = (int)_slotB
                });

                // Consume both slots
                _slotA = RuneType.None;
                _slotB = RuneType.None;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CurrentRunCombos++;
                }
            }
        }

        // ── Clear ─────────────────────────────────────────��─────────

        public void ClearSlots()
        {
            _slotA = RuneType.None;
            _slotB = RuneType.None;
        }

        public void Reset()
        {
            _slotA = RuneType.None;
            _slotB = RuneType.None;
            _totalRunesCollected = 0;
        }
    }
}
