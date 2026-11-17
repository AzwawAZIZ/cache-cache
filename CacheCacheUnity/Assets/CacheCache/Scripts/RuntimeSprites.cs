using System.Collections.Generic;
using UnityEngine;

namespace CacheCache
{
    /// <summary>Génère et met en cache des sprites de base : disque, rectangle arrondi, anneau.</summary>
    public static class RuntimeSprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        public const int PixelsPerUnit = 64;

        public static Sprite Circle
        {
            get
            {
                if (Cache.TryGetValue("circle", out var s)) return s;
                const int size = 64;
                var tex = NewTexture(size);
                var r = size * 0.5f;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d)));
                }
                tex.Apply();
                return Save("circle", tex);
            }
        }

        public static Sprite Rounded
        {
            get
            {
                if (Cache.TryGetValue("rounded", out var s)) return s;
                const int size = 64;
                const float radius = 16f;
                var tex = NewTexture(size);
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;
                    var cx = Mathf.Clamp(px, radius, size - radius);
                    var cy = Mathf.Clamp(py, radius, size - radius);
                    var d = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d)));
                }
                tex.Apply();
                return Save("rounded", tex);
            }
        }

        /// <summary>Version 9-slice pour l'UI : les coins gardent un rayon constant.</summary>
        public static Sprite RoundedSliced
        {
            get
            {
                if (Cache.TryGetValue("rounded-sliced", out var s)) return s;
                const int size = 64;
                const float radius = 16f;
                var tex = NewTexture(size);
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;
                    var cx = Mathf.Clamp(px, radius, size - radius);
                    var cy = Mathf.Clamp(py, radius, size - radius);
                    var d = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d)));
                }
                tex.Apply();
                var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(20f, 20f, 20f, 20f));
                Cache["rounded-sliced"] = sprite;
                return sprite;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (Cache.TryGetValue("ring", out var s)) return s;
                const int size = 96;
                var tex = NewTexture(size);
                var r = size * 0.5f;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    var a = Mathf.Clamp01(5f - Mathf.Abs(d - (r - 7f)));
                    tex.SetPixel(x, y, new Color(1f, 0.85f, 0.45f, a));
                }
                tex.Apply();
                return Save("ring", tex);
            }
        }

        /// <summary>Dégradé vertical, utilisé pour les murs.</summary>
        public static Sprite Gradient(Color top, Color bottom)
        {
            const int w = 4;
            const int h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (var y = 0; y < h; y++)
            {
                var c = Color.Lerp(bottom, top, y / (float)(h - 1));
                for (var x = 0; x < w; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            var key = "gradient_" + ColorUtility.ToHtmlStringRGB(top) + ColorUtility.ToHtmlStringRGB(bottom);
            return Save(key, tex);
        }

        static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        static Sprite Save(string key, Texture2D tex)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            Cache[key] = sprite;
            return sprite;
        }

        public static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite, Color color, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite, Color color, Vector3 position, Vector3 size, int order, float rotation = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
