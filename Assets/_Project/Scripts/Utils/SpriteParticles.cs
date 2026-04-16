using System.Collections;
using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Player;
using RuneDrop.Runes;

namespace RuneDrop.Utils
{
    /// <summary>
    /// Professional sprite-based particle effects.
    /// Multi-phase death, combo eruptions, rune collection bursts.
    /// No ParticleSystem API — pure GameObjects for device compatibility.
    /// </summary>
    public class SpriteParticles : MonoBehaviour
    {
        private void Start()
        {
            EventBus.Subscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Subscribe<RunePowerActivatedEvent>(OnPowerActivated);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Unsubscribe<RunePowerActivatedEvent>(OnPowerActivated);
        }

        // ── Event Handlers ──────────────────────────────────────────

        private void OnRuneCollected(RuneCollectedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            Color color = ((RuneType)evt.Type) switch
            {
                RuneType.Fire => new Color(1f, 0.5f, 0f),
                RuneType.Wind => new Color(0.3f, 0.95f, 1f),
                RuneType.Shadow => new Color(0.7f, 0.2f, 1f),
                RuneType.Earth => new Color(0.3f, 1f, 0.3f),
                _ => Color.white
            };

            SpawnBurst(player.transform.position, color, 8, 2f, 0.6f);
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;
            Vector3 pos = player.transform.position;

            // Gold screen flash
            StartCoroutine(ScreenFlash(new Color(1f, 0.85f, 0.3f, 0.25f), 0.2f));

            // Expanding ring of 16 particles
            SpawnExpandingRing(pos, new Color(1f, 0.8f, 0.2f, 0.5f), 16, 0.5f, 4f, 0.8f);

            // Inner burst — mix gold and white
            SpawnBurst(pos, new Color(1f, 0.8f, 0.2f), 12, 3f, 0.9f);
            SpawnBurst(pos, new Color(1f, 1f, 0.8f), 6, 4f, 1.1f);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;
            Vector3 pos = player.transform.position;

            // Phase 1: Red screen flash
            StartCoroutine(ScreenFlash(new Color(1f, 0.2f, 0.1f, 0.6f), 0.15f));

            // Phase 2: Expanding death ring
            SpawnExpandingRing(pos, new Color(1f, 0.3f, 0.15f, 0.5f), 16, 0.3f, 4f, 0.5f);

            // Phase 3: Mixed particle explosion
            SpawnBurst(pos, new Color(1f, 0.2f, 0.1f), 14, 4f, 1.2f);
            SpawnBurst(pos, new Color(1f, 0.5f, 0.1f), 8, 3f, 1f);
            SpawnBurst(pos, new Color(0.4f, 0.1f, 0.3f), 6, 2f, 1.3f);

            // Phase 4: Lingering embers (float upward slowly)
            StartCoroutine(SpawnEmbers(pos, 8, 2.5f));
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;
            SpawnExpandingRing(player.transform.position, new Color(0.3f, 0.8f, 1f, 0.4f), 10, 0.3f, 2.5f, 0.5f);
        }

        private void OnPowerActivated(RunePowerActivatedEvent evt)
        {
            var player = PlayerController.Instance;
            if (player == null) return;

            Color color = evt.Type switch
            {
                1 => new Color(1f, 0.5f, 0f),
                2 => new Color(0.3f, 1f, 1f),
                3 => new Color(0.7f, 0.2f, 1f),
                4 => new Color(0.2f, 1f, 0.3f),
                _ => Color.white
            };

            SpawnExpandingRing(player.transform.position, color, 10, 0.3f, 3f, 0.7f);
            SpawnBurst(player.transform.position, color, 6, 2.5f, 0.7f);
        }

        // ── Effect Methods ──────────────────────────────────────────

        private void SpawnBurst(Vector3 center, Color color, int count, float speed, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                var dir = Random.insideUnitCircle.normalized;
                var vel = new Vector3(dir.x, dir.y, 0f) * speed * Random.Range(0.5f, 1f);
                bool isCircle = Random.value > 0.3f;
                SpawnParticle(center, vel, color, Random.Range(0.08f, 0.2f), lifetime, isCircle);
            }
        }

        private void SpawnExpandingRing(Vector3 center, Color color, int count,
            float startRadius, float endRadius, float lifetime)
        {
            StartCoroutine(AnimateRing(center, color, count, startRadius, endRadius, lifetime));
        }

        private IEnumerator AnimateRing(Vector3 center, Color color, int count,
            float startRadius, float endRadius, float lifetime)
        {
            var ringObjects = new GameObject[count];
            var ringRenderers = new SpriteRenderer[count];

            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                var go = new GameObject("RingParticle");
                go.transform.position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * startRadius;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteHelper.WhiteCircle;
                sr.color = color;
                sr.sortingOrder = 18;
                go.transform.localScale = Vector3.one * 0.12f;
                ringObjects[i] = go;
                ringRenderers[i] = sr;
            }

            float elapsed = 0f;
            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;
                float radius = Mathf.Lerp(startRadius, endRadius, t);
                float alpha = color.a * (1f - t);
                float scale = Mathf.Lerp(0.12f, 0.04f, t);

                for (int i = 0; i < count; i++)
                {
                    if (ringObjects[i] == null) continue;
                    float angle = (float)i / count * Mathf.PI * 2f;
                    ringObjects[i].transform.position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                    ringRenderers[i].color = new Color(color.r, color.g, color.b, alpha);
                    ringObjects[i].transform.localScale = Vector3.one * scale;
                }
                yield return null;
            }

            for (int i = 0; i < count; i++)
                if (ringObjects[i] != null) Destroy(ringObjects[i]);
        }

        private IEnumerator ScreenFlash(Color color, float duration)
        {
            var go = new GameObject("ScreenFlash");
            var cam = Camera.main;
            if (cam == null) { Destroy(go); yield break; }

            go.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteSquare;
            sr.color = color;
            sr.sortingOrder = 20;
            go.transform.localScale = new Vector3(25f, 50f, 1f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var c = color;
                c.a = color.a * (1f - elapsed / duration);
                sr.color = c;
                yield return null;
            }
            Destroy(go);
        }

        private IEnumerator SpawnEmbers(Vector3 center, int count, float lifetime)
        {
            yield return new WaitForSeconds(0.3f); // Delay after death burst

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Ember");
                go.transform.position = center + (Vector3)Random.insideUnitCircle * 0.5f;
                go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteHelper.WhiteCircle;
                sr.color = new Color(1f, Random.Range(0.3f, 0.5f), 0.1f, 0.3f);
                sr.sortingOrder = 16;
                StartCoroutine(AnimateEmber(go, sr, lifetime));
            }
        }

        private IEnumerator AnimateEmber(GameObject go, SpriteRenderer sr, float lifetime)
        {
            float elapsed = 0f;
            float xDrift = Random.Range(-0.3f, 0.3f);
            Color startColor = sr.color;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;
                go.transform.position += new Vector3(
                    Mathf.Sin(elapsed * 2f + xDrift) * 0.3f * Time.deltaTime,
                    0.4f * Time.deltaTime, 0);
                var c = startColor;
                c.a = startColor.a * (1f - t);
                sr.color = c;
                yield return null;
            }
            Destroy(go);
        }

        // ── Single Particle ─────────────────────────────────────────

        private void SpawnParticle(Vector3 pos, Vector3 velocity, Color color, float size, float lifetime, bool circle)
        {
            var go = new GameObject("Particle");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;
            if (!circle) go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = circle ? SpriteHelper.WhiteCircle : SpriteHelper.WhiteSquare;
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
                go.transform.position += velocity * Time.deltaTime;
                velocity *= 0.95f;
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
