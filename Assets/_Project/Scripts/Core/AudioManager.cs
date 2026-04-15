using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Centralized audio management. Singleton with DontDestroyOnLoad.
    /// Handles SFX via pooled AudioSources and music via crossfading.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static AudioManager Instance { get; private set; }

        // ── Configuration ───────────────────────────────────────────
        [Serializable]
        public struct SoundEntry
        {
            public string Key;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume;
        }

        [Header("Sound Library")]
        [SerializeField] private SoundEntry[] _sounds;

        [Header("SFX Pool")]
        [SerializeField] private int _sfxPoolSize = 8;

        [Header("Music")]
        [SerializeField] private float _musicCrossfadeDuration = 1f;

        // ── Storage ─────────────────────────────────────────────────
        private Dictionary<string, SoundEntry> _soundDict;
        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private bool _musicAIsActive;

        private float _sfxVolume = 1f;
        private float _musicVolume = 0.7f;

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
            InitializeAudio();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister(this);
                Instance = null;
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        private void InitializeAudio()
        {
            // Build sound dictionary
            _soundDict = new Dictionary<string, SoundEntry>();
            if (_sounds != null)
            {
                foreach (var entry in _sounds)
                {
                    if (!string.IsNullOrEmpty(entry.Key) && entry.Clip != null)
                    {
                        _soundDict[entry.Key] = entry;
                    }
                }
            }

            // Create SFX pool
            _sfxSources = new AudioSource[_sfxPoolSize];
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                _sfxSources[i] = go.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
            }

            // Create music sources
            var musicGoA = new GameObject("Music_A");
            musicGoA.transform.SetParent(transform);
            _musicSourceA = musicGoA.AddComponent<AudioSource>();
            _musicSourceA.loop = true;
            _musicSourceA.playOnAwake = false;

            var musicGoB = new GameObject("Music_B");
            musicGoB.transform.SetParent(transform);
            _musicSourceB = musicGoB.AddComponent<AudioSource>();
            _musicSourceB.loop = true;
            _musicSourceB.playOnAwake = false;

            _musicAIsActive = true;

            // Load volume from save
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                _sfxVolume = save.Data.SfxVolume;
                _musicVolume = save.Data.MusicVolume;
            }
        }

        // ── SFX ─────────────────────────────────────────────────────

        public void PlaySFX(string key)
        {
            if (!_soundDict.TryGetValue(key, out var entry))
            {
                return;
            }

            var source = _sfxSources[_sfxIndex];
            source.clip = entry.Clip;
            source.volume = entry.Volume * _sfxVolume;
            source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            source.Play();

            _sfxIndex = (_sfxIndex + 1) % _sfxPoolSize;
        }

        // ── Music ───────────────────────────────────────────────────

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            var incoming = _musicAIsActive ? _musicSourceB : _musicSourceA;
            var outgoing = _musicAIsActive ? _musicSourceA : _musicSourceB;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            StartCoroutine(Crossfade(outgoing, incoming));
            _musicAIsActive = !_musicAIsActive;
        }

        public void StopMusic()
        {
            StartCoroutine(FadeOut(_musicAIsActive ? _musicSourceA : _musicSourceB));
        }

        private IEnumerator Crossfade(AudioSource outgoing, AudioSource incoming)
        {
            float elapsed = 0f;
            float outVol = outgoing.volume;

            while (elapsed < _musicCrossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _musicCrossfadeDuration;
                outgoing.volume = Mathf.Lerp(outVol, 0f, t);
                incoming.volume = Mathf.Lerp(0f, _musicVolume, t);
                yield return null;
            }

            outgoing.Stop();
            outgoing.volume = 0f;
            incoming.volume = _musicVolume;
        }

        private IEnumerator FadeOut(AudioSource source)
        {
            float startVol = source.volume;
            float elapsed = 0f;

            while (elapsed < _musicCrossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, elapsed / _musicCrossfadeDuration);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
        }

        // ── Volume Control ──────────────────────────────────────────

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                save.Data.SfxVolume = _sfxVolume;
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            var activeSource = _musicAIsActive ? _musicSourceA : _musicSourceB;
            activeSource.volume = _musicVolume;

            if (ServiceLocator.TryGet<SaveSystem>(out var save))
            {
                save.Data.MusicVolume = _musicVolume;
            }
        }

        public float SFXVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
    }
}
