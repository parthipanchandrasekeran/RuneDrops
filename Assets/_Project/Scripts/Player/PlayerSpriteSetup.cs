using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteSetup : MonoBehaviour
    {
        private void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = SpriteHelper.WhiteSquare;
            }
            // Ensure player is visible size on mobile
            transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        }
    }
}
