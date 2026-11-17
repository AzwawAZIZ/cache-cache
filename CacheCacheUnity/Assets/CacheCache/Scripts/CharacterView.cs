using System.Collections;
using UnityEngine;

namespace CacheCache
{
    /// <summary>Farceur dessiné avec des formes simples, entièrement recolorable.</summary>
    public class CharacterView : MonoBehaviour
    {
        public const int BodyOrder = 10;
        public const int FaceOrder = 45;

        CharacterLook look;
        SpriteRenderer backHair;
        SpriteRenderer head;
        SpriteRenderer fringe;
        SpriteRenderer body;
        SpriteRenderer ring;
        SpriteRenderer glow;
        Vector3 baseScale;
        Coroutine hintRoutine;

        public static CharacterView Create(CharacterLook data, Transform parent, Vector2 position, float scale, bool flip)
        {
            var go = new GameObject("Farceur_" + data.displayName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var view = go.AddComponent<CharacterView>();
            view.Build(data);
            if (flip) go.transform.localScale = new Vector3(-scale, scale, 1f);
            return view;
        }

        void Build(CharacterLook data)
        {
            look = data;
            var root = transform;
            var bodyColor = data.shirt;
            var bodyDark = Color.Lerp(data.shirt, Color.black, 0.18f);
            var hairDark = Color.Lerp(data.hair, Color.black, 0.22f);

            body = RuntimeSprites.AddSprite(root, "body", RuntimeSprites.Rounded, bodyColor, new Vector3(0f, -0.52f, 0f), new Vector3(0.86f, 0.66f, 1f), BodyOrder);
            if (data.striped)
            {
                for (var i = 0; i < 2; i++)
                    RuntimeSprites.AddSprite(root, "stripe", RuntimeSprites.Rounded, new Color(1f, 0.98f, 0.9f, 0.8f), new Vector3(0f, -0.4f - i * 0.2f, 0f), new Vector3(0.8f, 0.07f, 1f), BodyOrder + 1);
            }
            RuntimeSprites.AddSprite(root, "body-shade", RuntimeSprites.Rounded, new Color(bodyDark.r, bodyDark.g, bodyDark.b, 0.5f), new Vector3(0f, -0.78f, 0f), new Vector3(0.78f, 0.24f, 1f), BodyOrder + 1);

            // nuque + cheveux arrière
            backHair = RuntimeSprites.AddSprite(root, "hair-back", RuntimeSprites.Circle, data.hair,
                new Vector3(0f, 0.06f, 0f), new Vector3(0.94f, 0.94f, 1f), FaceOrder - 3);
            if (data.hairStyle == HairStyle.Bun)
                RuntimeSprites.AddSprite(root, "bun", RuntimeSprites.Circle, data.hair, new Vector3(0f, 0.52f, 0f), new Vector3(0.34f, 0.34f, 1f), FaceOrder - 2);
            if (data.hairStyle == HairStyle.Braids)
            {
                RuntimeSprites.AddSprite(root, "braid-l", RuntimeSprites.Circle, data.hair, new Vector3(-0.42f, -0.12f, 0f), new Vector3(0.24f, 0.24f, 1f), FaceOrder - 2);
                RuntimeSprites.AddSprite(root, "braid-r", RuntimeSprites.Circle, data.hair, new Vector3(0.42f, -0.12f, 0f), new Vector3(0.24f, 0.24f, 1f), FaceOrder - 2);
            }

            head = RuntimeSprites.AddSprite(root, "head", RuntimeSprites.Circle, data.skin,
                new Vector3(0f, 0f, 0f), new Vector3(0.84f, 0.86f, 1f), FaceOrder - 1);

            // yeux
            var eyeColor = new Color(0.16f, 0.13f, 0.11f);
            RuntimeSprites.AddSprite(root, "eye-l", RuntimeSprites.Circle, eyeColor, new Vector3(-0.15f, 0.03f, 0f), new Vector3(0.11f, 0.13f, 1f), FaceOrder);
            RuntimeSprites.AddSprite(root, "eye-r", RuntimeSprites.Circle, eyeColor, new Vector3(0.15f, 0.03f, 0f), new Vector3(0.11f, 0.13f, 1f), FaceOrder);
            RuntimeSprites.AddSprite(root, "cheek-l", RuntimeSprites.Circle, data.accent, new Vector3(-0.27f, -0.1f, 0f), new Vector3(0.15f, 0.09f, 1f), FaceOrder);
            RuntimeSprites.AddSprite(root, "cheek-r", RuntimeSprites.Circle, data.accent, new Vector3(0.27f, -0.1f, 0f), new Vector3(0.15f, 0.09f, 1f), FaceOrder);
            RuntimeSprites.AddSprite(root, "mouth", RuntimeSprites.Rounded, new Color(0.45f, 0.22f, 0.16f), new Vector3(0f, -0.17f, 0f), new Vector3(0.16f, 0.06f, 1f), FaceOrder);

            // frange
            fringe = RuntimeSprites.AddSprite(root, "fringe", RuntimeSprites.Rounded, data.hair,
                new Vector3(0f, 0.27f, 0f), new Vector3(0.78f, 0.34f, 1f), FaceOrder + 1);
            if (data.hairStyle == HairStyle.Cap)
            {
                fringe.color = data.accent;
                RuntimeSprites.AddSprite(root, "cap-brim", RuntimeSprites.Rounded, data.accent, new Vector3(0.3f, 0.22f, 0f), new Vector3(0.42f, 0.12f, 1f), FaceOrder + 2);
            }
            if (data.hairStyle == HairStyle.Curly)
            {
                for (var i = 0; i < 5; i++)
                {
                    var a = Mathf.PI * (0.15f + i * 0.175f);
                    RuntimeSprites.AddSprite(root, "curl", RuntimeSprites.Circle, data.hair,
                        new Vector3(Mathf.Cos(a) * 0.4f, Mathf.Sin(a) * 0.4f + 0.16f, 0f), new Vector3(0.34f, 0.34f, 1f), FaceOrder);
                }
            }
            if (data.hairStyle == HairStyle.Tuft)
                RuntimeSprites.AddSprite(root, "tuft", RuntimeSprites.Circle, data.hair, new Vector3(0.08f, 0.44f, 0f), new Vector3(0.2f, 0.2f, 1f), FaceOrder + 2);

            // mains qui dépassent
            RuntimeSprites.AddSprite(root, "hand-l", RuntimeSprites.Circle, data.skin, new Vector3(-0.39f, -0.28f, 0f), new Vector3(0.28f, 0.28f, 1f), FaceOrder + 2);
            RuntimeSprites.AddSprite(root, "hand-r", RuntimeSprites.Circle, data.skin, new Vector3(0.39f, -0.28f, 0f), new Vector3(0.28f, 0.28f, 1f), FaceOrder + 2);

            baseScale = transform.localScale;

            ring = RuntimeSprites.AddSprite(root, "hint-ring", RuntimeSprites.Ring, Color.white,
                Vector3.zero, new Vector3(1.3f, 1.3f, 1f), FaceOrder + 6);
            ring.gameObject.SetActive(false);
            ring.transform.localPosition = new Vector3(0f, 0.16f, 0f);

            glow = RuntimeSprites.AddSprite(root, "glow", RuntimeSprites.Circle, new Color(1f, 0.92f, 0.63f, 0.3f),
                new Vector3(0f, 0.1f, 0f), new Vector3(1.7f, 1.7f, 1f), FaceOrder - 4);
            glow.gameObject.SetActive(false);

            hairDark = Color.Lerp(data.hair, Color.black, 0.2f);
            backHair.color = data.hair;
            head.color = data.skin;
            body.color = bodyColor;
            fringe.color = data.hairStyle == HairStyle.Cap ? data.accent : data.hair;
            _ = hairDark;
        }

        public void SetHint(bool on)
        {
            if (hintRoutine != null) StopCoroutine(hintRoutine);
            ring.gameObject.SetActive(on);
            if (!on) return;
            hintRoutine = StartCoroutine(HintPulse());
        }

        IEnumerator HintPulse()
        {
            var t = 0f;
            while (t < CacheCacheData.HintDuration)
            {
                t += Time.deltaTime;
                var k = (Mathf.Sin(t * 6f) + 1f) * 0.5f;
                ring.transform.localScale = Vector3.one * Mathf.Lerp(1.55f, 1.85f, k);
                var c = ring.color;
                c.a = Mathf.Lerp(0.55f, 1f, k);
                ring.color = c;
                yield return null;
            }
            ring.gameObject.SetActive(false);
        }

        public void Celebrate()
        {
            glow.gameObject.SetActive(true);
            StartCoroutine(Punch());
            StartCoroutine(FloatGlow());
        }

        IEnumerator Punch()
        {
            var t = 0f;
            const float duration = 0.42f;
            var sign = Mathf.Sign(transform.localScale.x);
            while (t < duration)
            {
                t += Time.deltaTime;
                var p = t / duration;
                var s = 1f + Mathf.Sin(p * Mathf.PI) * 0.16f;
                transform.localScale = new Vector3(baseScale.x * s * sign, baseScale.y * s, 1f);
                yield return null;
            }
            transform.localScale = new Vector3(baseScale.x * sign, baseScale.y, 1f);
        }

        IEnumerator FloatGlow()
        {
            var t = 0f;
            while (t < 2f)
            {
                t += Time.deltaTime;
                glow.transform.localPosition = new Vector3(0f, 0.1f + t * 0.08f, 0f);
                var c = glow.color;
                c.a = Mathf.Lerp(0.3f, 0f, t / 2f);
                glow.color = c;
                yield return null;
            }
            glow.gameObject.SetActive(false);
            glow.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        }

        public void SetPoseCrouch()
        {
            transform.localPosition += new Vector3(0f, -0.08f, 0f);
        }
    }
}
