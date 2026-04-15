using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Lightweight publish-subscribe event bus using struct-based events.
    /// All events are value types for memory efficiency.
    /// </summary>
    public static class EventBus
    {
        // ── Storage ─────────────────────────────────────────────────
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        // ── Subscribe / Unsubscribe ─────────────────────────────────
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<Delegate>();
            }
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        // ── Publish ─────────────────────────────────────────────────
        public static void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list)) return;

            // Iterate copy to allow safe unsubscribe during publish
            var snapshot = new List<Delegate>(list);
            foreach (var handler in snapshot)
            {
                try
                {
                    ((Action<T>)handler).Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Exception in {type.Name} handler: {e}");
                }
            }
        }

        // ── Cleanup ─────────────────────────────────────────────────
        public static void Clear()
        {
            _handlers.Clear();
        }
    }

    // ── Game State ──────────────────────────────────────────────────

    public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        DecisionRoom,
        Paused,
        Dead,
        Reviving
    }

    // ── Game State Events ───────────────────────────────────────────

    public struct GameStateChangedEvent
    {
        public GameState OldState;
        public GameState NewState;
    }

    public struct RunStartedEvent { }

    public struct PlayerDiedEvent
    {
        public float DepthReached;
        public int RunesCollected;
        public float RunDuration;
    }

    public struct ScoreChangedEvent
    {
        public float NewDepth;
        public int NewRuneCount;
    }

    // ── Player Events ───────────────────────────────────────────────

    public struct PlayerStateChangedEvent
    {
        public int OldState;
        public int NewState;
    }

    // ── Anchor Events ───────────────────────────────────────────────

    public struct AnchorUsedEvent
    {
        public int ChargesRemaining;
        public int MaxCharges;
    }

    public struct AnchorExpiredEvent { }

    // ── Rune Events ─────────────────────────────────────────────────

    public struct RuneCollectedEvent
    {
        public int Type;         // RuneType enum cast to int
        public int SlotIndex;    // 0 = A, 1 = B
        public int TotalCollected;
    }

    public struct ComboActivatedEvent
    {
        public int Combo;        // ComboType enum cast to int
        public int RuneA;
        public int RuneB;
    }

    public struct ComboExpiredEvent
    {
        public int Combo;
    }

    // ── Rune Power Events ───────────────────────────────────────────

    public struct RunePowerActivatedEvent
    {
        public int Type;
        public float Duration;
    }

    public struct RunePowerExpiredEvent
    {
        public int Type;
    }

    // ── Decision Events ─────────────────────────────────────────────

    public struct DecisionRoomAppearedEvent { }

    public struct DecisionMadeEvent
    {
        public string ChoiceId;
        public int Category;     // 0 = Safe, 1 = Risky
    }

    // ── Progression Events ──────────────────────────────────────────

    public struct SoulShardsChangedEvent
    {
        public int OldAmount;
        public int NewAmount;
        public int Delta;
    }

    public struct UpgradePurchasedEvent
    {
        public string UpgradeId;
        public int NewLevel;
    }

    // ── Tutorial Events ─────────────────────────────────────────────

    public struct TutorialCompletedEvent { }
}
