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
            // Use circle sprite for the player — looks like a glowing orb
            sr.sprite = SpriteHelper.WhiteCircle;
            sr.color = new Color(0.5f, 0.9f, 1f); // Bright cyan
            sr.sortingOrder = 10;
            transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }
    }
}
