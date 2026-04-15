using UnityEngine;

namespace RuneDrop.Player
{
    /// <summary>
    /// Handles player collision with obstacles and pickups.
    /// Delegates to PlayerController for state-aware damage handling.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCollision : MonoBehaviour
    {
        private PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Layer 7 = Obstacle
            if (other.gameObject.layer == 7)
            {
                _player.TakeDamage();
            }
        }
    }
}
