using System.Collections.Generic;
using UnityEngine;

namespace CacheCache
{
    public enum HairStyle { Tuft, Bob, Curly, Bun, Cap, Braids }

    public enum PropKind
    {
        Crate, Trunk, Dresser, Chair, Plant, Stool, Stall, Fountain, Table,
        Cart, Bush, CrateStack, Log, Rock, Campfire, Stump, Books, Desk,
        Armchair, Ladder, Globe, Ledge
    }

    [System.Serializable]
    public class CharacterLook
    {
        public string displayName;
        public Color skin;
        public Color hair;
        public Color shirt;
        public Color accent;
        public HairStyle hairStyle;
        public bool striped;
    }

    [System.Serializable]
    public class SceneTheme
    {
        public string displayName;
        public Color wallTop;
        public Color wallBottom;
        public Color floor;
        public Color floorDark;
        public Color accent;
        public Color prop;
        public Color propDark;
        public Color foliage;
        public PropKind[] props;
        public Vector2[] spots;   // coordonnées normalisées (0..1), origine en haut-gauche
        public float[] scales;
    }

    /// <summary>Toutes les données du jeu, sans ScriptableObject : zéro asset à créer.</summary>
    public static class CacheCacheData
    {
        public const int MaxHints = 3;
        public const int StartHints = 2;
        public const int BasePoints = 100;
        public const float MissPenalty = 2f;
        public const float HintDuration = 2.8f;

        public static int TargetForLevel(int level) => Mathf.Min(6, 3 + Mathf.CeilToInt(level / 2f));
        public static float TimeForLevel(int level) => Mathf.Max(26f, 56f - (level - 1) * 4f);
        public static float MultiplierFor(int streak) => Mathf.Min(4f, 1f + Mathf.Floor(streak / 2f) * 0.5f);

        public static CharacterLook[] Looks()
        {
            var looks = new CharacterLook[10];
            looks[0] = New("Léo", "#D8A077", "#5B3A29", "#E2694D", "#EDB84C", HairStyle.Tuft, true);
            looks[1] = New("Nour", "#E8BD94", "#2F2A28", "#2F6F68", "#E2694D", HairStyle.Bob, false);
            looks[2] = New("Sacha", "#A4714F", "#241D1A", "#E8C9A0", "#2F6F68", HairStyle.Curly, false);
            looks[3] = New("Mila", "#F2CBA8", "#B85C38", "#7A9A63", "#EDB84C", HairStyle.Bun, false);
            looks[4] = New("Timo", "#D9A77C", "#6B4A2F", "#F2E3C7", "#D9785D", HairStyle.Cap, false);
            looks[5] = New("Zélie", "#DFAE86", "#EDB84C", "#8A5A7A", "#E2694D", HairStyle.Braids, false);
            looks[6] = New("Noah", "#8D5A3B", "#1F1B1A", "#4E7B74", "#EDB84C", HairStyle.Tuft, false);
            looks[7] = New("Inès", "#E6B892", "#5A4436", "#D9785D", "#2F6F68", HairStyle.Bob, true);
            looks[8] = New("Alba", "#F0D0B4", "#8A3F2C", "#3F7F7A", "#EDB84C", HairStyle.Bun, true);
            looks[9] = New("Youssef", "#B57B52", "#2A211B", "#EDB84C", "#2F6F68", HairStyle.Cap, false);
            return looks;
        }

        static CharacterLook New(string n, string skin, string hair, string shirt, string accent, HairStyle style, bool striped)
        {
            return new CharacterLook
            {
                displayName = n,
                skin = Hex(skin),
                hair = Hex(hair),
                shirt = Hex(shirt),
                accent = Hex(accent),
                hairStyle = style,
                striped = striped
            };
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static SceneTheme[] Themes()
        {
            return new[]
            {
                new SceneTheme
                {
                    displayName = "Le Grenier de Mamie",
                    wallTop = Hex("#F4E2C2"), wallBottom = Hex("#E5C79F"),
                    floor = Hex("#C4935F"), floorDark = Hex("#A3713F"),
                    accent = Hex("#E2694D"), prop = Hex("#B5835A"), propDark = Hex("#8A5D3B"),
                    foliage = Hex("#7A9A63"),
                    props = new[] { PropKind.Trunk, PropKind.Crate, PropKind.Dresser, PropKind.Chair, PropKind.Plant, PropKind.Stool },
                    spots = new[] { new Vector2(0.296f, 0.65f), new Vector2(0.462f, 0.81f), new Vector2(0.596f, 0.716f), new Vector2(0.75f, 0.522f), new Vector2(0.902f, 0.759f), new Vector2(0.11f, 0.631f) },
                    scales = new[] { 0.8f, 0.74f, 0.78f, 0.84f, 0.7f, 0.72f }
                },
                new SceneTheme
                {
                    displayName = "Le Marché aux fleurs",
                    wallTop = Hex("#FBE8CD"), wallBottom = Hex("#F6D2AD"),
                    floor = Hex("#C5A98A"), floorDark = Hex("#A98A6D"),
                    accent = Hex("#2F6F68"), prop = Hex("#C69A68"), propDark = Hex("#A97C4F"),
                    foliage = Hex("#7A9A63"),
                    props = new[] { PropKind.Stall, PropKind.Cart, PropKind.Fountain, PropKind.Table, PropKind.CrateStack, PropKind.Ledge },
                    spots = new[] { new Vector2(0.15f, 0.635f), new Vector2(0.806f, 0.656f), new Vector2(0.505f, 0.731f), new Vector2(0.356f, 0.716f), new Vector2(0.646f, 0.781f), new Vector2(0.884f, 0.525f) },
                    scales = new[] { 0.8f, 0.78f, 0.72f, 0.74f, 0.72f, 0.7f }
                },
                new SceneTheme
                {
                    displayName = "La Forêt des lucioles",
                    wallTop = Hex("#102F3A"), wallBottom = Hex("#2B5B52"),
                    floor = Hex("#234A3F"), floorDark = Hex("#163029"),
                    accent = Hex("#EDB84C"), prop = Hex("#6B4C35"), propDark = Hex("#8A6244"),
                    foliage = Hex("#5F8A58"),
                    props = new[] { PropKind.Log, PropKind.Rock, PropKind.Campfire, PropKind.Bush, PropKind.Crate, PropKind.Stump },
                    spots = new[] { new Vector2(0.214f, 0.728f), new Vector2(0.452f, 0.703f), new Vector2(0.600f, 0.78f), new Vector2(0.792f, 0.684f), new Vector2(0.138f, 0.625f), new Vector2(0.934f, 0.672f) },
                    scales = new[] { 0.8f, 0.76f, 0.74f, 0.78f, 0.72f, 0.74f }
                },
                new SceneTheme
                {
                    displayName = "La Grande Bibliothèque",
                    wallTop = Hex("#2F5348"), wallBottom = Hex("#254237"),
                    floor = Hex("#7B4B32"), floorDark = Hex("#5F3826"),
                    accent = Hex("#C96E51"), prop = Hex("#7B5236"), propDark = Hex("#5D3C29"),
                    foliage = Hex("#6F9A63"),
                    props = new[] { PropKind.Books, PropKind.Desk, PropKind.Armchair, PropKind.Ladder, PropKind.Globe, PropKind.Ledge },
                    spots = new[] { new Vector2(0.168f, 0.731f), new Vector2(0.382f, 0.672f), new Vector2(0.772f, 0.525f), new Vector2(0.906f, 0.391f), new Vector2(0.592f, 0.781f), new Vector2(0.112f, 0.625f) },
                    scales = new[] { 0.78f, 0.8f, 0.84f, 0.7f, 0.72f, 0.7f }
                }
            };
        }
    }
}
