using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;

namespace RuneDrop.Utils
{
    /// <summary>
    /// Game feel: camera shake with varying intensity per event type.
    /// Enhanced for dramatic deaths and satisfying combat feedback.
    /// </summary>
    public class GameJuice : MonoBehaviour
    {
        private void Start()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Subscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Subscribe<RuneCollectedEvent>(OnRuneCollected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Unsubscribe<RunePowerActivatedEvent>(OnPowerActivated);
            EventBus.Unsubscribe<RuneCollectedEvent>(OnRuneCollected);
        }

        private void ShakeCamera(float intensity, float duration)
        {
            var cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null) cam.Shake(intensity, duration);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            ShakeCamera(0.8f, 0.5f); // Big dramatic shake
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            ShakeCamera(0.4f, 0.25f);
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            ShakeCamera(0.15f, 0.12f);
        }

        private void OnPowerActivated(RunePowerActivatedEvent evt)
        {
            ShakeCamera(0.25f, 0.15f);
        }

        private void OnRuneCollected(RuneCollectedEvent evt)
        {
            ShakeCamera(0.08f, 0.06f); // Very subtle
        }
    }
}
