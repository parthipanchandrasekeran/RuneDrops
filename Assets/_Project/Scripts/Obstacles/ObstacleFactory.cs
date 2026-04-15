using UnityEngine;

namespace RuneDrop.Obstacles
{
    public enum ObstacleType
    {
        Spike,
        MovingBlade,
        FallingRock,
        LaserTrap
    }

    /// <summary>
    /// Static factory for creating obstacles with difficulty-weighted type selection.
    /// </summary>
    public static class ObstacleFactory
    {
        /// <summary>
        /// Creates an obstacle of the specified type at the given position.
        /// </summary>
        public static ObstacleBase Create(ObstacleType type, Transform parent, Vector3 position, float difficulty)
        {
            var go = new GameObject($"Obstacle_{type}");
            go.transform.SetParent(parent);
            go.transform.position = position;

            ObstacleBase obstacle = type switch
            {
                ObstacleType.Spike => go.AddComponent<SpikeObstacle>(),
                ObstacleType.MovingBlade => go.AddComponent<MovingBladeObstacle>(),
                ObstacleType.FallingRock => go.AddComponent<FallingRockObstacle>(),
                ObstacleType.LaserTrap => go.AddComponent<LaserTrapObstacle>(),
                _ => go.AddComponent<SpikeObstacle>()
            };

            obstacle.Initialize(difficulty);
            return obstacle;
        }

        /// <summary>
        /// Creates a random obstacle weighted by difficulty.
        /// Low difficulty: mostly spikes. High difficulty: mix of all types.
        /// </summary>
        public static ObstacleBase CreateRandom(Transform parent, Vector3 position, float difficulty, System.Random rng)
        {
            var type = PickType(difficulty, rng);
            return Create(type, parent, position, difficulty);
        }

        private static ObstacleType PickType(float difficulty, System.Random rng)
        {
            // Weights change with difficulty
            // Spikes: always common
            // Blades: appear at 0.1+ difficulty
            // Rocks: appear at 0.2+ difficulty
            // Lasers: appear at 0.4+ difficulty
            float spikeWeight = 4f;
            float bladeWeight = difficulty > 0.1f ? Mathf.Lerp(0f, 3f, difficulty) : 0f;
            float rockWeight = difficulty > 0.2f ? Mathf.Lerp(0f, 2f, difficulty) : 0f;
            float laserWeight = difficulty > 0.4f ? Mathf.Lerp(0f, 2f, difficulty) : 0f;

            float total = spikeWeight + bladeWeight + rockWeight + laserWeight;
            float roll = (float)rng.NextDouble() * total;

            if (roll < spikeWeight) return ObstacleType.Spike;
            roll -= spikeWeight;
            if (roll < bladeWeight) return ObstacleType.MovingBlade;
            roll -= bladeWeight;
            if (roll < rockWeight) return ObstacleType.FallingRock;
            return ObstacleType.LaserTrap;
        }
    }
}
