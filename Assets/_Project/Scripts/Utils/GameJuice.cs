using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;

namespace RuneDrop.Utils
{
    /// <summary>
    /// Game feel: screen shake only. Particle effects removed to fix
    /// rendering issues on low-end Android devices.
    /// </summary>
    public class GameJuice : MonoBehaviour
    {
        private void Start()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void ShakeCamera(float intensity, float duration)
        {
            var cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null) cam.Shake(intensity, duration);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            ShakeCamera(0.5f, 0.3f);
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            ShakeCamera(0.3f, 0.2f);
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            ShakeCamera(0.15f, 0.1f);
        }
    }
}
