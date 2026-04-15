using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Generates atmospheric procedural sound effects.
    /// Dark fantasy style — deep tones, reverb-like tails, layered harmonics.
    /// No audio files needed.
    /// </summary>
    public class ProceduralAudio : MonoBehaviour
    {
        private AudioSource _source;
        private const int SAMPLE_RATE = 44100;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        private void Start()
        {
            EventBus.Subscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Subscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Subscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Subscribe<PlayerDiedEvent>(OnDied);
            EventBus.Subscribe<DecisionRoomAppearedEvent>(OnDecisionRoom);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RuneCollectedEvent>(OnRuneCollected);
            EventBus.Unsubscribe<ComboActivatedEvent>(OnComboActivated);
            EventBus.Unsubscribe<AnchorUsedEvent>(OnAnchorUsed);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnDied);
            EventBus.Unsubscribe<DecisionRoomAppearedEvent>(OnDecisionRoom);
        }

        // ── Events ──────────────────────────────────────────────────

        private void OnRuneCollected(RuneCollectedEvent evt)
        {
            // Crystal chime — two harmonics with quick decay
            PlaySound(CreateChime(700f, 0.15f), 0.45f);
        }

        private void OnComboActivated(ComboActivatedEvent evt)
        {
            // Power surge — rising tone with sub bass
            PlaySound(CreatePowerUp(200f, 600f, 0.4f), 0.55f);
        }

        private void OnAnchorUsed(AnchorUsedEvent evt)
        {
            // Heavy impact — low thud with decay
            PlaySound(CreateImpact(80f, 0.25f), 0.5f);
        }

        private void OnDied(PlayerDiedEvent evt)
        {
            // Death rumble — descending low tone
            PlaySound(CreateDeath(180f, 60f, 0.6f), 0.6f);
        }

        private void OnDecisionRoom(DecisionRoomAppearedEvent evt)
        {
            // Mystical whoosh — filtered noise sweep
            PlaySound(CreateWhoosh(0.3f), 0.2f);
        }

        // ── Sound Generators ────────────────────────────────────────

        private AudioClip CreateChime(float freq, float duration)
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float env = Mathf.Exp(-t * 15f); // Fast exponential decay

                // Two harmonics for richness
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                           + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f
                           + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.1f;

                data[i] = wave * env;
            }

            return MakeClip("chime", data);
        }

        private AudioClip CreatePowerUp(float startFreq, float endFreq, float duration)
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float p = t / duration;
                float env = (1f - p) * Mathf.Exp(-t * 3f);

                // Rising frequency
                float freq = Mathf.Lerp(startFreq, endFreq, p * p);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                           + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.3f; // Sub bass

                data[i] = wave * env;
            }

            return MakeClip("powerup", data);
        }

        private AudioClip CreateImpact(float freq, float duration)
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float env = Mathf.Exp(-t * 20f); // Very fast decay

                // Descending pitch for impact feel
                float f = freq * (1f + (1f - t / duration) * 2f);
                float wave = Mathf.Sin(2f * Mathf.PI * f * t) * 0.7f;

                // Add noise for texture
                wave += Random.Range(-0.15f, 0.15f) * env;

                data[i] = wave * env;
            }

            return MakeClip("impact", data);
        }

        private AudioClip CreateDeath(float startFreq, float endFreq, float duration)
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float p = t / duration;
                float env = (1f - p);

                // Descending frequency
                float freq = Mathf.Lerp(startFreq, endFreq, p);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                           + Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.4f;

                // Distortion for dark feel
                wave = Mathf.Clamp(wave * 1.5f, -0.8f, 0.8f);

                data[i] = wave * env;
            }

            return MakeClip("death", data);
        }

        private AudioClip CreateWhoosh(float duration)
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float p = t / duration;

                // Envelope: swell up then down
                float env = Mathf.Sin(p * Mathf.PI) * 0.5f;

                // Filtered noise
                float noise = Random.Range(-1f, 1f);
                float freq = Mathf.Lerp(200f, 800f, p);
                float toneFilter = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f;

                data[i] = (noise * 0.3f + toneFilter) * env;
            }

            return MakeClip("whoosh", data);
        }

        // ── Helpers ─────────────────────────────────────────────────

        private AudioClip MakeClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip != null && _source != null)
                _source.PlayOneShot(clip, volume);
        }
    }
}
