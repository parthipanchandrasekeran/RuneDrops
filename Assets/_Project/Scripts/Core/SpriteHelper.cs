using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Provides reusable sprites. Creates one shared texture per shape
    /// on first access. Uses simple formats compatible with all GPUs.
    /// </summary>
    public static class SpriteHelper
    {
        private static Sprite _square;
        private static Sprite _circle;

        public static Sprite WhiteSquare
        {
            get
            {
                if (_square == null) _square = MakeSquare();
                return _square;
            }
        }

        public static Sprite WhiteCircle
        {
            get
            {
                if (_circle == null) _circle = MakeCircle();
                return _circle;
            }
        }

        // Diamond = square rotated 45 degrees by the caller
        public static Sprite WhiteDiamond => WhiteSquare;

        private static Sprite MakeSquare()
        {
            // 4x4 solid white — tiny but scales perfectly as a filled rect
            var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            var colors = new Color32[16];
            for (int i = 0; i < 16; i++)
                colors[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(colors);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            Debug.Log("[SpriteHelper] Created WhiteSquare");
            return sprite;
        }

        private static Sprite MakeCircle()
        {
            // 16x16 circle — small enough for any GPU
            int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            var colors = new Color32[size * size];
            float center = size / 2f - 0.5f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    bool inside = (dx * dx + dy * dy) <= (radius * radius);
                    colors[y * size + x] = inside ?
                        new Color32(255, 255, 255, 255) :
                        new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(colors);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
            Debug.Log("[SpriteHelper] Created WhiteCircle");
            return sprite;
        }
    }
}
