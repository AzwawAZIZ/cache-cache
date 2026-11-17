using System.Collections.Generic;
using UnityEngine;

namespace CacheCache
{
    /// <summary>Construit le décor procédural et place les farceurs dans les cachettes.</summary>
    public class RoundBuilder : MonoBehaviour
    {
        public const float SceneWidth = 10f;
        public const float SceneHeight = 6.4f;

        Transform currentRoot;

        public static Vector3 ToWorld(Vector2 normalized)
        {
            return new Vector3((normalized.x - 0.5f) * SceneWidth, (0.5f - normalized.y) * SceneHeight, 0f);
        }

        public List<HideSpot> Build(SceneTheme theme, int themeIndex, List<CharacterLook> looks, List<int> chosenIndices)
        {
            Clear();
            currentRoot = new GameObject("Scene_" + theme.displayName).transform;

            BuildBackdrop(theme, themeIndex);
            var spots = new List<HideSpot>();
            if (looks == null || looks.Count == 0) return spots;

            for (var i = 0; i < theme.props.Length; i++)
            {
                var world = ToWorld(theme.spots[i]);
                BuildProp(theme.props[i], world, theme, 40);
                if (chosenIndices != null && !chosenIndices.Contains(i)) continue;

                var look = looks[spots.Count % looks.Count];
                var view = CharacterView.Create(look, currentRoot, world + new Vector3(0f, 0.78f * theme.scales[i], 0f), theme.scales[i], i % 2 == 1);
                var go = new GameObject("Spot_" + i);
                go.transform.SetParent(currentRoot, false);
                var spot = go.AddComponent<HideSpot>();
                spot.spotId = theme.displayName + "_" + i;
                spot.look = look;
                spot.view = view;
                spot.basePosition = world;
                spot.scale = theme.scales[i];
                spots.Add(spot);
            }

            BuildForegroundDetails(theme, themeIndex);
            return spots;
        }

        public void Clear()
        {
            if (currentRoot != null) Destroy(currentRoot.gameObject);
            currentRoot = null;
        }

        void BuildBackdrop(SceneTheme theme, int sceneIndex)
        {
            var gradient = RuntimeSprites.Gradient(theme.wallTop, theme.wallBottom);
            RuntimeSprites.AddSprite(currentRoot, "wall", gradient, Color.white, new Vector3(0f, 0.4f, 0f), new Vector3(14f, 9f, 1f), -60);
            RuntimeSprites.AddSprite(currentRoot, "floor", RuntimeSprites.Rounded, theme.floor, new Vector3(0f, -2.1f, 0f), new Vector3(14f, 3.9f, 1f), -50);
            RuntimeSprites.AddSprite(currentRoot, "baseboard", RuntimeSprites.Rounded, theme.floorDark, new Vector3(0f, -1.28f, 0f), new Vector3(14f, 0.16f, 1f), -45);

            switch (sceneIndex)
            {
                case 0: // grenier
                    RuntimeSprites.AddSprite(currentRoot, "beamL", RuntimeSprites.Rounded, theme.propDark, new Vector3(-4.6f, -0.4f, 0f), new Vector3(0.5f, 4.6f, 1f), -40, 14f);
                    RuntimeSprites.AddSprite(currentRoot, "beamR", RuntimeSprites.Rounded, theme.propDark, new Vector3(4.6f, -0.4f, 0f), new Vector3(0.5f, 4.6f, 1f), -40, -14f);
                    RuntimeSprites.AddSprite(currentRoot, "roof", RuntimeSprites.Rounded, theme.propDark, new Vector3(0f, 3.3f, 0f), new Vector3(11f, 0.4f, 1f), -40);
                    RuntimeSprites.AddSprite(currentRoot, "window-out", RuntimeSprites.Circle, new Color(0.22f, 0.3f, 0.38f), new Vector3(2.6f, 1.5f, 0f), Vector3.one * 1.9f, -38);
                    RuntimeSprites.AddSprite(currentRoot, "window-frame", RuntimeSprites.Ring, theme.prop, new Vector3(2.6f, 1.5f, 0f), Vector3.one * 2.05f, -37);
                    RuntimeSprites.AddSprite(currentRoot, "moon", RuntimeSprites.Circle, new Color(1f, 0.94f, 0.76f), new Vector3(2.85f, 1.72f, 0f), Vector3.one * 0.42f, -36);
                    RuntimeSprites.AddSprite(currentRoot, "shelf", RuntimeSprites.Rounded, theme.prop, new Vector3(-3.2f, 1.1f, 0f), new Vector3(2.1f, 0.16f, 1f), -38);
                    break;
                case 1: // marché
                    RuntimeSprites.AddSprite(currentRoot, "buildingL", RuntimeSprites.Rounded, new Color(0.93f, 0.84f, 0.71f), new Vector3(-3.6f, 0.9f, 0f), new Vector3(3.2f, 3.6f, 1f), -38);
                    RuntimeSprites.AddSprite(currentRoot, "buildingR", RuntimeSprites.Rounded, new Color(0.92f, 0.79f, 0.65f), new Vector3(3.9f, 1.1f, 0f), new Vector3(2.8f, 4.2f, 1f), -38);
                    for (var i = 0; i < 5; i++)
                        RuntimeSprites.AddSprite(currentRoot, "flag", RuntimeSprites.Circle, i % 2 == 0 ? theme.accent : theme.prop, new Vector3(-4f + i * 1.9f, 3.15f - Mathf.Abs(2 - i) * 0.16f, 0f), Vector3.one * 0.28f, -32);
                    RuntimeSprites.AddSprite(currentRoot, "awning-out", RuntimeSprites.Rounded, theme.accent, new Vector3(-3.2f, 1.9f, 0f), new Vector3(2.8f, 0.7f, 1f), -34);
                    RuntimeSprites.AddSprite(currentRoot, "window", RuntimeSprites.Rounded, new Color(0.55f, 0.7f, 0.68f), new Vector3(4f, 1.7f, 0f), new Vector3(1.2f, 1.5f, 1f), -36);
                    break;
                case 2: // forêt
                    RuntimeSprites.AddSprite(currentRoot, "moon-glow", RuntimeSprites.Circle, new Color(1f, 0.95f, 0.78f, 0.18f), new Vector3(3.2f, 2.1f, 0f), Vector3.one * 3f, -44);
                    RuntimeSprites.AddSprite(currentRoot, "moon", RuntimeSprites.Circle, new Color(1f, 0.95f, 0.8f), new Vector3(3.2f, 2.1f, 0f), Vector3.one * 1f, -42);
                    for (var i = 0; i < 6; i++)
                    {
                        var x = -4.4f + i * 1.8f;
                        RuntimeSprites.AddSprite(currentRoot, "trunk", RuntimeSprites.Rounded, new Color(0.2f, 0.16f, 0.14f), new Vector3(x, 0.2f, 0f), new Vector3(0.52f, 4.6f, 1f), -39);
                        RuntimeSprites.AddSprite(currentRoot, "canopy", RuntimeSprites.Circle, new Color(0.11f, 0.28f, 0.26f), new Vector3(x, 2.5f, 0f), new Vector3(2.6f, 1.6f, 1f), -40);
                    }
                    for (var i = 0; i < 12; i++)
                    {
                        var px = -4.5f + (i * 0.83f) % 9f;
                        var py = -0.2f + Mathf.Sin(i * 2.3f) * 1.4f;
                        RuntimeSprites.AddSprite(currentRoot, "firefly", RuntimeSprites.Circle, new Color(1f, 0.92f, 0.65f), new Vector3(px, py, 0f), Vector3.one * 0.08f, -30);
                    }
                    break;
                default: // bibliothèque
                    RuntimeSprites.AddSprite(currentRoot, "shelf", RuntimeSprites.Rounded, theme.propDark, new Vector3(0.4f, 0.6f, 0f), new Vector3(6.4f, 3.9f, 1f), -40);
                    for (var row = 0; row < 4; row++)
                    {
                        for (var i = 0; i < 22; i++)
                        {
                            var h = 0.3f + ((i * 7 + row * 5) % 5) * 0.055f;
                            var color = Color.HSVToRGB(((i * 37 + row * 13) % 100) / 100f, 0.38f, 0.72f);
                            RuntimeSprites.AddSprite(currentRoot, "book", RuntimeSprites.Rounded, color,
                                new Vector3(-2.5f + i * 0.27f, -1.1f + row * 0.86f + h * 0.5f, 0f), new Vector3(0.22f, h, 1f), -38);
                        }
                        RuntimeSprites.AddSprite(currentRoot, "shelf-board", RuntimeSprites.Rounded, theme.prop,
                            new Vector3(0.4f, -1.12f + row * 0.86f, 0f), new Vector3(6.4f, 0.1f, 1f), -37);
                    }
                    RuntimeSprites.AddSprite(currentRoot, "window", RuntimeSprites.Rounded, new Color(0.28f, 0.44f, 0.47f), new Vector3(-3.3f, 1.3f, 0f), new Vector3(1.9f, 2.8f, 1f), -39);
                    RuntimeSprites.AddSprite(currentRoot, "window-moon", RuntimeSprites.Circle, new Color(1f, 0.94f, 0.78f), new Vector3(-3.05f, 1.85f, 0f), Vector3.one * 0.34f, -38);
                    break;
            }
        }

        void BuildForegroundDetails(SceneTheme theme, int sceneIndex)
        {
            RuntimeSprites.AddSprite(currentRoot, "rug", RuntimeSprites.Circle, new Color(theme.accent.r, theme.accent.g, theme.accent.b, 0.22f),
                new Vector3(0f, -2.25f, 0f), new Vector3(7.2f, 0.9f, 1f), -44);
            if (sceneIndex == 2)
                RuntimeSprites.AddSprite(currentRoot, "grass", RuntimeSprites.Rounded, theme.foliage, new Vector3(0f, -2.5f, 0f), new Vector3(14f, 0.5f, 1f), -43);
        }

        void BuildProp(PropKind kind, Vector3 p, SceneTheme theme, int order)
        {
            var prop = theme.prop;
            var dark = theme.propDark;
            switch (kind)
            {
                case PropKind.Crate:
                case PropKind.CrateStack:
                    Box(p + new Vector3(0f, 0.3f, 0f), new Vector2(1.5f, 1f), prop, order);
                    Box(p + new Vector3(0f, 0.62f, 0f), new Vector2(1.6f, 0.14f), Color.Lerp(prop, Color.white, 0.18f), order + 1);
                    Box(p + new Vector3(0f, 0.3f, 0f), new Vector2(1.3f, 0.1f), dark, order + 2, 34f);
                    if (kind == PropKind.CrateStack)
                    {
                        Box(p + new Vector3(0.05f, 0.98f, 0f), new Vector2(1.1f, 0.7f), Color.Lerp(prop, dark, 0.25f), order + 1);
                        Box(p + new Vector3(0.05f, 1.2f, 0f), new Vector2(1.2f, 0.12f), prop, order + 2);
                    }
                    break;
                case PropKind.Trunk:
                    Box(p + new Vector3(0f, 0.34f, 0f), new Vector2(1.9f, 0.95f), dark, order);
                    Box(p + new Vector3(0f, 0.72f, 0f), new Vector2(1.95f, 0.3f), Color.Lerp(dark, Color.white, 0.12f), order + 1);
                    Box(p + new Vector3(0f, 0.29f, 0f), new Vector2(1.85f, 0.09f), theme.accent, order + 2);
                    Box(p + new Vector3(0f, 0.3f, 0f), new Vector2(0.3f, 0.28f), theme.accent, order + 3);
                    break;
                case PropKind.Dresser:
                    Box(p + new Vector3(0f, 0.55f, 0f), new Vector2(1.5f, 1.5f), prop, order);
                    Box(p + new Vector3(0f, 0.9f, 0f), new Vector2(1.35f, 0.34f), dark, order + 1);
                    Box(p + new Vector3(0f, 0.45f, 0f), new Vector2(1.35f, 0.34f), dark, order + 1);
                    Circle(p + new Vector3(0.35f, 0.9f, 0f), 0.1f, theme.accent, order + 2);
                    Circle(p + new Vector3(0.35f, 0.45f, 0f), 0.1f, theme.accent, order + 2);
                    break;
                case PropKind.Chair:
                case PropKind.Armchair:
                    Box(p + new Vector3(0f, 0.95f, 0f), new Vector2(kind == PropKind.Armchair ? 1.9f : 1.5f, 1.3f), prop, order);
                    Box(p + new Vector3(0f, 0.7f, 0f), new Vector2(kind == PropKind.Armchair ? 1.7f : 1.4f, 0.35f), dark, order + 1);
                    Box(p + new Vector3(0f, 0.42f, 0f), new Vector2(kind == PropKind.Armchair ? 2.1f : 1.6f, 0.4f), Color.Lerp(prop, Color.white, 0.1f), order + 2);
                    Box(p + new Vector3(-1.0f, 0.75f, 0f), new Vector2(0.32f, 0.8f), dark, order + 3);
                    Box(p + new Vector3(1.0f, 0.75f, 0f), new Vector2(0.32f, 0.8f), dark, order + 3);
                    Box(p + new Vector3(0f, 0.56f, 0f), new Vector2(1.2f, 0.2f), theme.accent, order + 4);
                    break;
                case PropKind.Plant:
                    Box(p + new Vector3(0f, 0.35f, 0f), new Vector2(1.2f, 0.75f), theme.accent, order);
                    for (var i = 0; i < 4; i++)
                        Circle(p + new Vector3(-0.4f + i * 0.27f, 0.95f + (i % 2) * 0.22f, 0f), 0.32f, theme.foliage, order + 1);
                    break;
                case PropKind.Stool:
                    Box(p + new Vector3(0f, 0.35f, 0f), new Vector2(1.1f, 0.25f), prop, order);
                    Box(p + new Vector3(-0.35f, 0.05f, 0f), new Vector2(0.14f, 0.6f), dark, order);
                    Box(p + new Vector3(0.35f, 0.05f, 0f), new Vector2(0.14f, 0.6f), dark, order);
                    break;
                case PropKind.Stall:
                    Box(p + new Vector3(0f, 0.32f, 0f), new Vector2(2.8f, 1.1f), prop, order);
                    Box(p + new Vector3(0f, 0.24f, 0f), new Vector2(2.6f, 0.34f), new Color(0.95f, 0.92f, 0.82f), order + 1);
                    for (var i = 0; i < 5; i++)
                        Circle(p + new Vector3(-1.15f + i * 0.58f, 0.62f, 0f), 0.22f, i % 2 == 0 ? theme.accent : theme.foliage, order + 2);
                    for (var i = 0; i < 5; i++)
                        Box(p + new Vector3(-1.3f + i * 0.64f, 1.55f, 0f), new Vector2(0.42f, 0.8f), i % 2 == 0 ? theme.accent : Color.Lerp(theme.accent, Color.white, 0.55f), order + 3);
                    break;
                case PropKind.Fountain:
                    Circle(p + new Vector3(0f, 0.32f, 0f), 0.95f, Color.Lerp(prop, Color.white, 0.35f), order);
                    Box(p + new Vector3(0f, 0.5f, 0f), new Vector2(0.55f, 0.5f), Color.Lerp(prop, Color.white, 0.5f), order + 1);
                    Circle(p + new Vector3(0f, 0.8f, 0f), 0.32f, new Color(0.62f, 0.82f, 0.8f), order + 2);
                    Circle(p + new Vector3(0f, 0.32f, 0f), 0.78f, new Color(0.55f, 0.78f, 0.78f), order + 2);
                    break;
                case PropKind.Table:
                    Box(p + new Vector3(0f, 0.62f, 0f), new Vector2(1.8f, 0.2f), new Color(0.96f, 0.93f, 0.85f), order);
                    Box(p + new Vector3(0f, 0.15f, 0f), new Vector2(0.16f, 1.2f), dark, order);
                    Box(p + new Vector3(0f, -0.4f, 0f), new Vector2(0.9f, 0.12f), dark, order + 1);
                    break;
                case PropKind.Cart:
                    Box(p + new Vector3(0f, 0.25f, 0f), new Vector2(2.1f, 1f), prop, order);
                    Circle(p + new Vector3(-0.62f, 0.02f, 0f), 0.34f, dark, order + 1);
                    Circle(p + new Vector3(0.62f, 0.02f, 0f), 0.34f, dark, order + 1);
                    Circle(p + new Vector3(-0.62f, 0.02f, 0f), 0.14f, new Color(0.9f, 0.85f, 0.75f), order + 2);
                    Circle(p + new Vector3(0.62f, 0.02f, 0f), 0.14f, new Color(0.9f, 0.85f, 0.75f), order + 2);
                    Circle(p + new Vector3(0f, 1.05f, 0f), 0.9f, theme.accent, order + 3);
                    Box(p + new Vector3(0f, 1.05f, 0f), new Vector2(0.1f, 1.8f), dark, order + 4);
                    break;
                case PropKind.Bush:
                    Circle(p + new Vector3(-0.42f, 0.35f, 0f), 0.6f, theme.foliage, order);
                    Circle(p + new Vector3(0.42f, 0.32f, 0f), 0.56f, Color.Lerp(theme.foliage, Color.white, 0.1f), order);
                    Circle(p + new Vector3(0f, 0.68f, 0f), 0.48f, theme.foliage, order + 1);
                    Circle(p + new Vector3(0.2f, 0.7f, 0f), 0.12f, theme.accent, order + 2);
                    break;
                case PropKind.Log:
                    Box(p + new Vector3(0f, 0.2f, 0f), new Vector2(2.3f, 0.6f), prop, order);
                    Circle(p + new Vector3(1.12f, 0.2f, 0f), 0.34f, Color.Lerp(prop, Color.white, 0.22f), order + 1);
                    Circle(p + new Vector3(1.12f, 0.2f, 0f), 0.16f, dark, order + 2);
                    Circle(p + new Vector3(-0.5f, 0.55f, 0f), 0.22f, theme.foliage, order + 2);
                    break;
                case PropKind.Rock:
                    Circle(p + new Vector3(0f, 0.32f, 0f), 0.9f, new Color(0.42f, 0.48f, 0.46f), order);
                    Circle(p + new Vector3(0.42f, 0.22f, 0f), 0.55f, new Color(0.36f, 0.42f, 0.4f), order + 1);
                    Circle(p + new Vector3(-0.3f, 0.55f, 0f), 0.24f, theme.foliage, order + 2);
                    break;
                case PropKind.Campfire:
                    Box(p + new Vector3(0f, 0.12f, 0f), new Vector2(1.6f, 0.26f), prop, order, 8f);
                    Box(p + new Vector3(0f, 0.12f, 0f), new Vector2(1.4f, 0.24f), dark, order + 1, -10f);
                    Circle(p + new Vector3(0f, 0.55f, 0f), 0.28f, new Color(1f, 0.78f, 0.28f), order + 2);
                    Circle(p + new Vector3(0f, 0.5f, 0f), 0.16f, new Color(1f, 0.92f, 0.65f), order + 3);
                    Circle(p + new Vector3(0f, 0.5f, 0f), 0.9f, new Color(1f, 0.82f, 0.4f, 0.12f), order);
                    break;
                case PropKind.Stump:
                    Box(p + new Vector3(0f, 0.25f, 0f), new Vector2(1f, 0.8f), prop, order);
                    Circle(p + new Vector3(0f, 0.66f, 0f), 0.5f, Color.Lerp(prop, Color.white, 0.15f), order + 1);
                    Circle(p + new Vector3(0f, 0.66f, 0f), 0.24f, dark, order + 2);
                    break;
                case PropKind.Books:
                    Box(p + new Vector3(0f, 0.12f, 0f), new Vector2(1.7f, 0.24f), theme.accent, order);
                    Box(p + new Vector3(0.05f, 0.34f, 0f), new Vector2(1.45f, 0.22f), new Color(0.3f, 0.5f, 0.47f), order + 1);
                    Box(p + new Vector3(-0.04f, 0.54f, 0f), new Vector2(1.2f, 0.18f), new Color(0.83f, 0.63f, 0.3f), order + 2);
                    Box(p + new Vector3(0f, 0.85f, 0f), new Vector2(0.02f, 1.2f), dark, order);
                    break;
                case PropKind.Desk:
                    Box(p + new Vector3(0f, 0.62f, 0f), new Vector2(2.3f, 0.2f), prop, order);
                    Box(p + new Vector3(-0.9f, 0.05f, 0f), new Vector2(0.18f, 1.2f), dark, order);
                    Box(p + new Vector3(0.9f, 0.05f, 0f), new Vector2(0.18f, 1.2f), dark, order);
                    Box(p + new Vector3(-0.45f, 0.82f, 0f), new Vector2(0.7f, 0.2f), theme.accent, order + 1);
                    Box(p + new Vector3(-0.4f, 0.98f, 0f), new Vector2(0.6f, 0.16f), new Color(0.31f, 0.5f, 0.47f), order + 1);
                    Circle(p + new Vector3(0.7f, 0.98f, 0f), 0.24f, new Color(1f, 0.87f, 0.6f), order + 1);
                    break;
                case PropKind.Ladder:
                    Box(p + new Vector3(-0.55f, 0.95f, 0f), new Vector2(0.14f, 2.2f), prop, order, -8f);
                    Box(p + new Vector3(0.55f, 0.95f, 0f), new Vector2(0.14f, 2.2f), prop, order, -8f);
                    for (var i = 0; i < 5; i++)
                        Box(p + new Vector3(0.05f + i * 0.02f, 0.2f + i * 0.42f, 0f), new Vector2(1.05f, 0.1f), Color.Lerp(prop, Color.white, 0.35f), order + 1, -8f);
                    break;
                case PropKind.Globe:
                    Box(p + new Vector3(0f, 0.18f, 0f), new Vector2(0.7f, 0.16f), dark, order);
                    Circle(p + new Vector3(0f, 0.65f, 0f), 0.45f, new Color(0.5f, 0.68f, 0.68f), order - 1);
                    Circle(p + new Vector3(-0.1f, 0.72f, 0f), 0.16f, theme.foliage, order);
                    Circle(p + new Vector3(0.12f, 0.55f, 0f), 0.12f, theme.foliage, order);
                    break;
                case PropKind.Ledge:
                    Box(p + new Vector3(0f, 0.55f, 0f), new Vector2(2.2f, 0.16f), prop, order);
                    Circle(p + new Vector3(-0.6f, 0.82f, 0f), 0.22f, theme.foliage, order + 1);
                    Box(p + new Vector3(0.5f, 0.78f, 0f), new Vector2(0.18f, 0.42f), theme.accent, order + 1);
                    break;
            }
        }

        void Box(Vector3 position, Vector2 size, Color color, int order, float rotation = 0f)
        {
            RuntimeSprites.AddSprite(currentRoot, "prop", RuntimeSprites.Rounded, color, position, new Vector3(size.x, size.y, 1f), order, rotation);
        }

        void Circle(Vector3 position, float radius, Color color, int order)
        {
            RuntimeSprites.AddSprite(currentRoot, "prop", RuntimeSprites.Circle, color, position, new Vector3(radius * 2f, radius * 2f, 1f), order);
        }
    }
}
