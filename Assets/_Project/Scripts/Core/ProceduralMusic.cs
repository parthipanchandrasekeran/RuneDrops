using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Generates a dark ambient drone that responds to gameplay.
    /// Low pulsing bass with higher harmonics that intensify with speed.
    /// No audio files needed.
    /// </summary>
    public class ProceduralMusic : MonoBehaviour
    {
        private AudioSource _source;
        private AudioClip _droneClip;
        private const int SAMPLE_RATE = 44100;
        private const int DRONE_LENGTH_SECONDS = 8; // Loop length
        private float _intensity = 0.3f;

        private void Start()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = 0.12f;

            GenerateDrone();
            _source.clip = _droneClip;
            _source.Play();
        }

        private void Update()
        {
            // Scale intensity based on game state
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentState == GameState.Playing)
            {
                // Gradually increase intensity with depth
                float depthRatio = Mathf.Clamp01(gm.CurrentRunDepth / 300f);
                _intensity = Mathf.Lerp(_intensity, 0.08f + depthRatio * 0.18f, Time.deltaTime * 2f);
            }
            else
            {
                _intensity = Mathf.Lerp(_intensity, 0.05f, Time.deltaTime);
            }

            _source.volume = _intensity;
        }

        private void GenerateDrone()
        {
            int totalSamples = SAMPLE_RATE * DRONE_LENGTH_SECONDS;
            float[] data = new float[totalSamples];

            // Base frequency in D minor (dark fantasy key)
            float baseFreq = 73.42f; // D2

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float loopT = t / DRONE_LENGTH_SECONDS; // 0-1 over loop

                // Slow pulsing envelope
                float pulse = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);

                // Bass drone (fundamental + sub)
                float wave = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.4f;
                wave += Mathf.Sin(2f * Mathf.PI * baseFreq * 0.5f * t) * 0.3f; // Sub octave

                // Dark fifth (Ab - tritone-ish for tension)
                wave += Mathf.Sin(2f * Mathf.PI * baseFreq * 1.498f * t) * 0.12f;

                // High shimmer (octave + fifth, very quiet)
                wave += Mathf.Sin(2f * Mathf.PI * baseFreq * 3f * t) * 0.06f;
                wave += Mathf.Sin(2f * Mathf.PI * baseFreq * 4f * t) * 0.03f;

                // Slow filter sweep (simulated by amplitude modulation of harmonics)
                float sweep = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);
                wave += Mathf.Sin(2f * Mathf.PI * baseFreq * 5f * t) * 0.04f * sweep;

                // Apply pulse and slight noise for texture
                wave *= pulse;
                wave += Random.Range(-0.01f, 0.01f); // Very subtle noise floor

                // Crossfade loop point to avoid click
                float fadeIn = Mathf.Clamp01(t * 10f);
                float fadeOut = Mathf.Clamp01((DRONE_LENGTH_SECONDS - t) * 10f);
                wave *= fadeIn * fadeOut;

                data[i] = Mathf.Clamp(wave, -0.9f, 0.9f);
            }

            _droneClip = AudioClip.Create("ambient_drone", totalSamples, 1, SAMPLE_RATE, false);
            _droneClip.SetData(data, 0);
        }
    }
}
