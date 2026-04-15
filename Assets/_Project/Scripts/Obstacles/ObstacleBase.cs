using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Obstacles
{
    /// <summary>
    /// Abstract base class for all obstacles. Uses SpriteHelper cached sprites.
    /// </summary>
    public abstract class ObstacleBase : MonoBehaviour
    {
        protected SpriteRenderer SpriteRenderer;
        protected float Difficulty;

        protected virtual void Awake()
        {
            gameObject.layer = 7;
        }

        public virtual void Initialize(float difficulty)
        {
            Difficulty = difficulty;
            SetupVisuals();
            SetupCollider();
        }

        protected abstract void SetupVisuals();
        protected abstract void SetupCollider();

        protected Sprite CreateSquareSprite() => SpriteHelper.WhiteSquare;
        protected Sprite CreateDiamondSprite() => SpriteHelper.WhiteDiamond;
        protected Sprite CreateCircleSprite() => SpriteHelper.WhiteCircle;

        protected SpriteRenderer EnsureSpriteRenderer()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            if (SpriteRenderer == null)
                SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            SpriteRenderer.sortingOrder = 2;
            return SpriteRenderer;
        }
    }
}
