using UnityEngine;
using RuneDrop.Core;

namespace RuneDrop.Level
{
    /// <summary>
    /// Infinite scrolling dark background using cached sprites.
    /// </summary>
    public class BackgroundScroller : MonoBehaviour
    {
        [SerializeField] private Color _backgroundColor = new Color(0.05f, 0.02f, 0.1f);
        [SerializeField] private float _parallaxFactor = 0.95f;

        private SpriteRenderer _bgA;
        private SpriteRenderer _bgB;
        private float _spriteHeight;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null) return;
            CreateBackgroundSprites();
        }

        private void LateUpdate()
        {
            if (_camera == null || _bgA == null) return;

            float camY = _camera.transform.position.y;
            float bgY = camY * _parallaxFactor;
            float offset = bgY % _spriteHeight;
            _bgA.transform.position = new Vector3(0, bgY - offset, 5f);
            _bgB.transform.position = new Vector3(0, bgY - offset - _spriteHeight, 5f);
        }

        private void CreateBackgroundSprites()
        {
            float camHeight = _camera.orthographicSize * 2f;
            float camWidth = camHeight * _camera.aspect;
            _spriteHeight = camHeight + 2f;

            _bgA = CreateBG("BG_A", camWidth, _spriteHeight);
            _bgB = CreateBG("BG_B", camWidth, _spriteHeight);
        }

        private SpriteRenderer CreateBG(string name, float width, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.WhiteSquare;
            sr.color = _backgroundColor;
            sr.sortingOrder = -10;
            go.transform.localScale = new Vector3(width + 2f, height, 1f);
            return sr;
        }
    }
}
