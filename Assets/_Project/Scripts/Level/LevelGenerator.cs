using System.Collections.Generic;
using UnityEngine;
using RuneDrop.Core;
using RuneDrop.Data;
using RuneDrop.Player;
using RuneDrop.Runes;
using RuneDrop.Obstacles;

namespace RuneDrop.Level
{
    /// <summary>
    /// Procedural level generator. Spawns chunks of obstacles and runes
    /// ahead of the player, destroys chunks behind.
    /// SIMPLIFIED: fewer obstacles, clear spacing, proper cleanup.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }

        [SerializeField] private GameConfigSO _config;

        private readonly List<LevelChunk> _activeChunks = new();
        private float _nextChunkY;
        private int _chunksGenerated;
        private System.Random _rng;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            if (_config == null)
                _config = Resources.Load<GameConfigSO>("Configs/GameConfig");
            Initialize(Random.Range(0, int.MaxValue));
        }

        private void OnDestroy()
        {
            if (Instance == this) { ServiceLocator.Unregister<LevelGenerator>(); Instance = null; }
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive) return;

            float playerY = player.transform.position.y;

            // Spawn chunks ahead
            float spawnThreshold = playerY - (_config.ChunksAhead * _config.ChunkHeight);
            while (_nextChunkY > spawnThreshold)
            {
                SpawnChunk(_nextChunkY);
                _nextChunkY -= _config.ChunkHeight;
            }

            // Despawn chunks that are above the player (already passed)
            float despawnY = playerY + _config.ChunkHeight * 2f;
            for (int i = _activeChunks.Count - 1; i >= 0; i--)
            {
                var chunk = _activeChunks[i];
                if (chunk == null || chunk.YPosition > despawnY)
                {
                    if (chunk != null) chunk.DestroyImmediate();
                    _activeChunks.RemoveAt(i);
                }
            }
        }

        // ── Initialize ──────────────────────────────────────────────

        public void Initialize(int seed)
        {
            _rng = new System.Random(seed);
            _nextChunkY = -5f; // Start slightly below player
            _chunksGenerated = 0;

            // Clear all existing chunks
            foreach (var chunk in _activeChunks)
            {
                if (chunk != null) chunk.DestroyImmediate();
            }
            _activeChunks.Clear();

            // Also clear any orphaned chunk objects
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // Spawn initial chunks
            for (int i = 0; i < _config.ChunksAhead + 1; i++)
            {
                SpawnChunk(_nextChunkY);
                _nextChunkY -= _config.ChunkHeight;
            }

            Debug.Log($"[LevelGenerator] Initialized with seed {seed}");
        }

        // ── Chunk Spawning ──────────────────────────────────────────

        private void SpawnChunk(float yPosition)
        {
            var chunkGO = new GameObject($"Chunk_{_chunksGenerated}");
            chunkGO.transform.SetParent(transform);

            var chunk = chunkGO.AddComponent<LevelChunk>();
            chunk.SetBounds(yPosition, _config.ChunkHeight);
            chunk.ChunkIndex = _chunksGenerated;

            // First 2 chunks are safe (no obstacles)
            if (_chunksGenerated > 1)
            {
                PopulateChunk(chunk);
            }

            _activeChunks.Add(chunk);
            _chunksGenerated++;
        }

        // ── Population ──────────────────────────────────────────────

        private void PopulateChunk(LevelChunk chunk)
        {
            float difficulty = GetDifficulty();

            var bounds = WorldBounds.Instance;
            float leftX = bounds != null ? bounds.LeftBound + 1.5f : -3f;
            float rightX = bounds != null ? bounds.RightBound - 1.5f : 3f;

            float chunkTop = chunk.YPosition;
            float chunkBottom = chunk.YPosition - _config.ChunkHeight;

            // ── Obstacles: 2-4 per chunk, well-spaced ───────────────
            int obstacleCount = 2 + Mathf.FloorToInt(difficulty * 2f); // 2-4
            float spacing = _config.ChunkHeight / (obstacleCount + 1);

            for (int i = 0; i < obstacleCount; i++)
            {
                float yPos = chunkTop - spacing * (i + 1);
                float xPos = Mathf.Lerp(leftX, rightX, (float)_rng.NextDouble());

                // Create obstacle
                var obs = ObstacleFactory.CreateRandom(chunk.transform,
                    new Vector3(xPos, yPos, 0f), difficulty, _rng);
            }

            // ── Runes: 1-2 per chunk, placed in gaps between obstacles ──
            int runeCount = 1 + (_rng.Next(100) < 40 ? 1 : 0); // 60% chance of 1, 40% chance of 2
            for (int i = 0; i < runeCount; i++)
            {
                // Place runes between obstacle rows
                float yPos = chunkTop - spacing * (0.5f + _rng.Next(obstacleCount));
                float xPos = Mathf.Lerp(leftX, rightX, (float)_rng.NextDouble());

                SpawnRune(chunk.transform, new Vector3(xPos, yPos, 0f));
            }
        }

        private void SpawnRune(Transform parent, Vector3 position)
        {
            RuneType[] types = { RuneType.Fire, RuneType.Wind, RuneType.Shadow, RuneType.Earth };
            var type = types[_rng.Next(types.Length)];

            var runeGO = new GameObject($"Rune_{type}");
            runeGO.transform.SetParent(parent);
            runeGO.transform.position = position;

            var pickup = runeGO.AddComponent<RunePickup>();
            pickup.Initialize(type);
        }

        // ── Difficulty ──────────────────────────────────────────────

        public float GetDifficulty()
        {
            return Mathf.Clamp01(_chunksGenerated / 50f);
        }
    }
}
