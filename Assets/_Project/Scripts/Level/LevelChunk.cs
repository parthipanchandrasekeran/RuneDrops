using System.Collections.Generic;
using UnityEngine;

namespace RuneDrop.Level
{
    /// <summary>
    /// Container for a single procedural chunk.
    /// Destroy this GameObject to destroy all children (obstacles/runes).
    /// </summary>
    public class LevelChunk : MonoBehaviour
    {
        public float YPosition;
        public float Height;
        public int ChunkIndex;

        public void SetBounds(float yPos, float height)
        {
            YPosition = yPos;
            Height = height;
            transform.position = new Vector3(0f, yPos, 0f);
        }

        /// <summary>
        /// Immediately destroys all children and this GameObject.
        /// </summary>
        public void DestroyImmediate()
        {
            // Destroy all children first
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
            Object.Destroy(gameObject);
        }
    }
}
