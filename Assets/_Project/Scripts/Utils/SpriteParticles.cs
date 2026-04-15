using System.Collections;
using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;

namespace RuneDrop.Utils
{
    /// <summary>
    /// Simple sprite-based particle effects. No ParticleSystem API —
    /// just spawns small GameObjects that move, fade, and self-destruct.
    /// Compatible with all devices including SM-A032F.
    /// </summary>
    public class SpriteParticles : MonoBehaviour
    {
        private void Start()
        {
            EventBus.Subscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
        }

        private void OnRuneCollected(RuneCollectedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            var type = (RuneType)evt.Type;
            Color color = type switch
            {
                RuneType.Fire => new Color(1f, 0.5f, 0f),
                RuneType.Wind => new Color(0.3f, 0.95f, 1f),
                RuneType.Shadow => new Color(0.7f, 0.2f, 1f),
                RuneType.Earth => new Color(0.3f, 1f, 0.3f),
                _ => Color.white
            };

            SpawnBurst(player.transform.position, color, 6, 1.5f, 0.5f);
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            SpawnBurst(player.transform.position, new Color(1f, 0.8f, 0.2f), 12, 2.5f, 0.8f);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            SpawnBurst(player.transform.position, new Color(1f, 0.2f, 0.2f), 15, 3f, 1f);
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            SpawnRing(player.transform.position, new Color(0.3f, 0.8f, 1f), 8, 0.6f);
        }

        // ── Burst Effect ────────────────────────────────────────────

        private void SpawnBurst(Vector3 center, Color color, int count, float speed, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                var dir = Random.insideUnitCircle.normalized;
                var vel = new Vector3(dir.x, dir.y, 0f) * speed * Random.Range(0.5f, 1f);
                SpawnParticle(center, vel, color, Random.Range(0.15f, 0.35f), lifetime);
            }
        }

        // ── Ring Effect ─────────────────────────────────────────────

        private void SpawnRing(Vector3 center, Color color, int count, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                SpawnParticle(center, dir * 2f, color, 0.2f, lifetime);
            }
        }

        // ── Single Particle ─────────────────────────────────────────

        private void SpawnParticle(Vector3 pos, Vector3 velocity, Color color, float size, float lifetime)
        {
            var go = new GameObject("Particle");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteCircle;
            sr.color = color;
            sr.sortingOrder = 15;

            StartCoroutine(AnimateParticle(go, sr, velocity, lifetime));
        }

        private IEnumerator AnimateParticle(GameObject go, SpriteRenderer sr, Vector3 velocity, float lifetime)
        {
            float elapsed = 0f;
            Color startColor = sr.color;
            Vector3 startScale = go.transform.localScale;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Move
                go.transform.position += velocity * Time.deltaTime;
                velocity *= 0.95f; // Drag

                // Fade and shrink
                var c = startColor;
                c.a = (1f - t) * startColor.a;
                sr.color = c;
                go.transform.localScale = startScale * (1f - t * 0.5f);

                yield return null;
            }

            Destroy(go);
        }
    }
}
