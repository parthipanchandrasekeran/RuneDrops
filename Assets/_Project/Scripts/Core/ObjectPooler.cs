using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Tag-based object pooling system.
    /// Spawn/Despawn objects by string tag for zero-allocation gameplay.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        // ── Singleton ───────────────────────────────────────────────
        public static ObjectPooler Instance { get; private set; }

        // ── Configuration ───────────────────────────────────────────
        [Serializable]
        public struct PoolConfig
        {
            public string Tag;
            public GameObject Prefab;
            public int InitialSize;
            public bool ExpandIfEmpty;
        }

        [SerializeField] private PoolConfig[] _pools;

        // ── Storage ─────────────────────────────────────────────────
        private readonly Dictionary<string, Queue<GameObject>> _poolDict = new();
        private readonly Dictionary<string, PoolConfig> _configDict = new();
        private readonly Dictionary<string, Transform> _parentDict = new();

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
            InitializePools();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<ObjectPooler>();
                Instance = null;
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        private void InitializePools()
        {
            if (_pools == null) return;

            foreach (var config in _pools)
            {
                if (string.IsNullOrEmpty(config.Tag) || config.Prefab == null) continue;

                _configDict[config.Tag] = config;
                _poolDict[config.Tag] = new Queue<GameObject>();

                // Create parent container
                var parent = new GameObject($"Pool_{config.Tag}");
                parent.transform.SetParent(transform);
                _parentDict[config.Tag] = parent.transform;

                // Pre-instantiate
                for (int i = 0; i < config.InitialSize; i++)
                {
                    var obj = CreateInstance(config.Tag);
                    obj.SetActive(false);
                    _poolDict[config.Tag].Enqueue(obj);
                }
            }
        }

        // ── Register Pool At Runtime ────────────────────────────────

        public void RegisterPool(string tag, GameObject prefab, int initialSize, bool expandIfEmpty = true)
        {
            if (_poolDict.ContainsKey(tag)) return;

            var config = new PoolConfig
            {
                Tag = tag,
                Prefab = prefab,
                InitialSize = initialSize,
                ExpandIfEmpty = expandIfEmpty
            };

            _configDict[tag] = config;
            _poolDict[tag] = new Queue<GameObject>();

            var parent = new GameObject($"Pool_{tag}");
            parent.transform.SetParent(transform);
            _parentDict[tag] = parent.transform;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateInstance(tag);
                obj.SetActive(false);
                _poolDict[tag].Enqueue(obj);
            }
        }

        // ── Spawn / Despawn ─────────────────────────────────────────

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.ContainsKey(tag))
            {
                Debug.LogError($"[ObjectPooler] Unknown tag: {tag}");
                return null;
            }

            GameObject obj;

            if (_poolDict[tag].Count > 0)
            {
                obj = _poolDict[tag].Dequeue();
            }
            else if (_configDict[tag].ExpandIfEmpty)
            {
                obj = CreateInstance(tag);
            }
            else
            {
                Debug.LogWarning($"[ObjectPooler] Pool exhausted: {tag}");
                return null;
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(string tag, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);

            if (_poolDict.ContainsKey(tag))
            {
                obj.transform.SetParent(_parentDict[tag]);
                _poolDict[tag].Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        public void DespawnDelayed(string tag, GameObject obj, float delay)
        {
            if (obj == null) return;
            StartCoroutine(DespawnAfterDelay(tag, obj, delay));
        }

        private IEnumerator DespawnAfterDelay(string tag, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(tag, obj);
        }

        // ── Helpers ─────────────────────────────────────────────────

        private GameObject CreateInstance(string tag)
        {
            if (!_configDict.ContainsKey(tag))
            {
                Debug.LogError($"[ObjectPooler] No config for tag: {tag}");
                return null;
            }

            var obj = Instantiate(_configDict[tag].Prefab, _parentDict[tag]);
            obj.name = $"{tag}_{_poolDict[tag].Count}";
            return obj;
        }

        /// <summary>Returns the number of available objects for a tag.</summary>
        public int GetAvailableCount(string tag)
        {
            return _poolDict.ContainsKey(tag) ? _poolDict[tag].Count : 0;
        }

        /// <summary>Despawns all active objects for all pools.</summary>
        public void DespawnAll()
        {
            foreach (var kvp in _parentDict)
            {
                var parent = kvp.Value;
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    var child = parent.GetChild(i).gameObject;
                    if (child.activeSelf)
                    {
                        Despawn(kvp.Key, child);
                    }
                }
            }
        }
    }
}
