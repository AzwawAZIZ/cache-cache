using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CacheCache
{
    /// <summary>UI 100% générée par code (uGUI + TextMeshPro). Aucun prefab à créer.</summary>
    public class UIManager : MonoBehaviour
    {
        static readonly Color Cream = new Color(0.973f, 0.941f, 0.878f);
        static readonly Color Ink = new Color(0.169f, 0.129f, 0.102f);
        static readonly Color Soft = new Color(0.43f, 0.36f, 0.30f);
        static readonly Color Coral = new Color(0.886f, 0.412f, 0.302f);
        static readonly Color Teal = new Color(0.184f, 0.435f, 0.408f);
        static readonly Color Mustard = new Color(0.929f, 0.722f, 0.298f);
        static readonly Color Track = new Color(0.169f, 0.129f, 0.102f, 0.12f);

        Canvas canvas;
        RectTransform root;
        TMP_FontAsset font;
        GameManager game;

        GameObject hudRoot, rosterRoot, menuScreen, pauseScreen, clearScreen, overScreen;
        TextMeshProUGUI scoreText, bestText, themeText, subtitleText, levelText, streakText, timeText,
            hintsLabel, foundText, muteLabel, menuSoundLabel, menuRecords, clearChip, clearTitle, clearBody, clearScore,
            overChip, overTitle, overBody, overList, pauseStats;

        Image timeFill;
        readonly List<TextMeshProUGUI> rosterNames = new List<TextMeshProUGUI>();
        readonly List<Image> rosterBgs = new List<Image>();
        readonly List<RectTransform> rosterPortraits = new List<RectTransform>();
        int lastScore = -1;

        public void Build(Canvas targetCanvas)
        {
            canvas = targetCanvas;
            root = canvas.GetComponent<RectTransform>();
            font = TMP_Settings.defaultFontAsset;
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            BuildHud();
            BuildMenu();
            BuildPause();
            BuildClear();
            BuildOver();
            ShowHud(false);
            ShowMenu(true);
        }

        public void Bind(GameManager manager)
        {
            game = manager;
            RefreshHud();
            RefreshMenu();
        }

        /* ---------------- primitives ---------------- */

        RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        Image Img(RectTransform rt, Color color, bool rounded = true, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            if (rounded)
            {
                img.sprite = RuntimeSprites.RoundedSliced;
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.sprite = RuntimeSprites.Circle;
                img.type = Image.Type.Simple;
            }
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        TextMeshProUGUI Label(RectTransform parent, string value, float size, Color color, TextAlignmentOptions align, float top, float height)
        {
            var rt = Rect("Label", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -top), new Vector2(-80f, height));
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        Button Btn(RectTransform parent, string label, Vector2 position, Vector2 size, Color background, Color textColor, float textSize = 30f)
        {
            var rt = Rect("Button_" + label, parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, size);
            var img = Img(rt, background, true, true);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            var hit = Rect("Text", rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var text = hit.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = label;
            text.fontSize = textSize;
            text.color = textColor;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return button;
        }

        RectTransform Card(string name, Vector2 size, out GameObject screen)
        {
            var overlay = Rect(name, root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var dim = Img(overlay, new Color(0.10f, 0.08f, 0.09f, 0.66f), false, true);
            dim.sprite = null;
            var card = Rect("Card", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), size);
            Img(card, Cream, true, true);
            var strip = Rect("TopStrip", card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(180f, 14f));
            Img(strip, Mustard);
            screen = overlay.gameObject;
            return card;
        }

        void SmallChip(RectTransform card, string text, Color color, float top)
        {
            var chip = Rect("Chip", card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -top), new Vector2(300f, 46f));
            Img(chip, color);
            var t = Label(chip, text, 18f, Ink, TextAlignmentOptions.Center, 11f, 30f);
            t.fontStyle = FontStyles.Bold;
        }

        /* ---------------- HUD ---------------- */

        void BuildHud()
        {
            hudRoot = Rect("HUD", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero).gameObject;

            var bar = Rect("TopBar", hudRoot.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(-36f, 172f));
            Img(bar, new Color(0.995f, 0.973f, 0.918f, 0.97f), true, true);

            var badge = Rect("LevelBadge", bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -24f), new Vector2(80f, 80f));
            Img(badge, Coral);
            levelText = Label(badge, "1", 44f, Cream, TextAlignmentOptions.Center, 12f, 60f);

            themeText = Rect("Theme", bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(124f, -26f), new Vector2(560f, 40f)).gameObject.AddComponent<TextMeshProUGUI>();
            themeText.font = font;
            themeText.text = "Le Grenier de Mamie";
            themeText.fontSize = 32f;
            themeText.color = Ink;
            themeText.alignment = TextAlignmentOptions.Left;
            themeText.raycastTarget = false;

            subtitleText = Rect("Subtitle", bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(124f, -68f), new Vector2(560f, 30f)).gameObject.AddComponent<TextMeshProUGUI>();
            subtitleText.font = font;
            subtitleText.text = "Poussière dorée et vieilles malles";
            subtitleText.fontSize = 21f;
            subtitleText.color = Soft;
            subtitleText.alignment = TextAlignmentOptions.Left;
            subtitleText.raycastTarget = false;

            foundText = Rect("Found", bar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(124f, -108f), new Vector2(400f, 30f)).gameObject.AddComponent<TextMeshProUGUI>();
            foundText.font = font;
            foundText.text = "0 / 3 trouvés";
            foundText.fontSize = 23f;
            foundText.color = Teal;
            foundText.fontStyle = FontStyles.Bold;
            foundText.alignment = TextAlignmentOptions.Left;
            foundText.raycastTarget = false;

            // score
            var scoreGroup = Rect("Score", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-628f, -20f), new Vector2(210f, 120f));
            var scoreCaption = Label(scoreGroup, "SCORE", 16f, Soft, TextAlignmentOptions.Right, 6f, 24f);
            scoreCaption.fontStyle = FontStyles.Bold;
            scoreText = Label(scoreGroup, "0", 54f, Ink, TextAlignmentOptions.Right, 30f, 60f);
            bestText = Label(scoreGroup, "record 0", 18f, Soft, TextAlignmentOptions.Right, 88f, 24f);

            // timer
            var timerGroup = Rect("Timer", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-408f, -20f), new Vector2(200f, 120f));
            var timeCaption = Label(timerGroup, "TEMPS", 16f, Soft, TextAlignmentOptions.Left, 6f, 24f);
            timeCaption.fontStyle = FontStyles.Bold;
            timeText = Label(timerGroup, "56", 54f, Ink, TextAlignmentOptions.Left, 28f, 60f);
            var track = Rect("Track", timerGroup, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(0f, 14f));
            Img(track, Track);
            var fill = Rect("Fill", track, Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            timeFill = Img(fill, Teal);
            timeFill.type = Image.Type.Filled;
            timeFill.fillMethod = Image.FillMethod.Horizontal;
            timeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            timeFill.fillAmount = 1f;

            // streak
            var streakChip = Rect("Streak", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-284f, -44f), new Vector2(108f, 100f));
            Img(streakChip, new Color(0.929f, 0.722f, 0.298f, 0.28f));
            var streakCaption = Label(streakChip, "SÉRIE", 15f, Soft, TextAlignmentOptions.Center, 10f, 22f);
            streakCaption.fontStyle = FontStyles.Bold;
            streakText = Label(streakChip, "0", 40f, Ink, TextAlignmentOptions.Center, 30f, 46f);

            // buttons
            var hintsButtonRt = Rect("Hints", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-168f, -44f), new Vector2(100f, 100f));
            var hintsImg = Img(hintsButtonRt, Mustard, true, true);
            var hintsButton = hintsButtonRt.gameObject.AddComponent<Button>();
            hintsButton.targetGraphic = hintsImg;
            hintsButton.onClick.AddListener(() => { if (game != null) game.UseHint(); });
            hintsLabel = Label(hintsButtonRt, "2", 40f, Ink, TextAlignmentOptions.Center, 22f, 56f);
            var hintCaption = Label(hintsButtonRt, "INDICE", 13f, new Color(0.22f, 0.16f, 0.08f), TextAlignmentOptions.Center, 72f, 20f);
            hintCaption.fontStyle = FontStyles.Bold;

            var pauseRt = Rect("Pause", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-88f, -44f), new Vector2(72f, 100f));
            var pauseImg = Img(pauseRt, new Color(1f, 1f, 1f, 0.85f), true, true);
            var pauseButton = pauseRt.gameObject.AddComponent<Button>();
            pauseButton.targetGraphic = pauseImg;
            pauseButton.onClick.AddListener(() => { if (game != null) game.TogglePause(); });
            var pauseIcon = Label(pauseRt, "II", 32f, Ink, TextAlignmentOptions.Center, 30f, 44f);
            pauseIcon.fontStyle = FontStyles.Bold;

            var muteRt = Rect("Mute", bar, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -44f), new Vector2(60f, 100f));
            var muteImg = Img(muteRt, new Color(1f, 1f, 1f, 0.85f), true, true);
            var muteButton = muteRt.gameObject.AddComponent<Button>();
            muteButton.targetGraphic = muteImg;
            muteButton.onClick.AddListener(() => { if (game != null) game.ToggleMute(); });
            muteLabel = Label(muteRt, "ON", 22f, Ink, TextAlignmentOptions.Center, 40f, 30f);

            rosterRoot = Rect("Roster", hudRoot.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -202f), new Vector2(-36f, 92f)).gameObject;
            var rosterBlocker = rosterRoot.gameObject.AddComponent<Image>();
            rosterBlocker.color = new Color(0f, 0f, 0f, 0f);
            rosterBlocker.raycastTarget = true;

            var hintNotice = Rect("Hint", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(880f, 46f));
            var notice = hintNotice.gameObject.AddComponent<TextMeshProUGUI>();
            notice.font = font;
            notice.text = "Clique sur les visages qui dépassent  •  Flèches + Espace au clavier  •  H indice  •  P pause";
            notice.fontSize = 21f;
            notice.color = Cream;
            notice.alignment = TextAlignmentOptions.Center;
            notice.raycastTarget = false;
        }

        /* ---------------- screens ---------------- */

        void BuildMenu()
        {
            var card = Card("MenuScreen", new Vector2(900f, 760f), out menuScreen);
            SmallChip(card, "JEU D'OBSERVATION", new Color(0.184f, 0.435f, 0.408f, 0.18f), 54f);
            var title = Label(card, "Cache-Cache !", 82f, Ink, TextAlignmentOptions.Center, 112f, 100f);
            title.fontStyle = FontStyles.Bold;
            Label(card, "Ouvre l'œil. Trouve-les. Recommence.", 30f, Coral, TextAlignmentOptions.Center, 214f, 46f);
            Label(card,
                "Les farceurs du village se cachent dans quatre décors. Déniche-les avant la fin du sablier, enchaîne les trouvailles et bats ton record.",
                24f, Soft, TextAlignmentOptions.Center, 268f, 100f);
            menuRecords = Label(card, "Meilleur score : —", 26f, Ink, TextAlignmentOptions.Center, 376f, 150f);
            menuRecords.fontStyle = FontStyles.Bold;

            Btn(card, "COMMENCER LA PARTIE", new Vector2(0f, 196f), new Vector2(620f, 88f), Coral, Cream, 30f)
                .onClick.AddListener(() => { if (game != null) game.StartRun(); });
            var soundButton = Btn(card, "SON : ON", new Vector2(0f, 96f), new Vector2(300f, 70f), new Color(0.973f, 0.941f, 0.878f), Ink, 24f);
            soundButton.onClick.AddListener(() => { if (game != null) game.ToggleMute(); });
            menuSoundLabel = soundButton.GetComponentInChildren<TextMeshProUGUI>();
            Label(card, "Jouable à la souris, au doigt et au clavier. Ton meilleur score local est conservé sur cet appareil.", 18f, Soft, TextAlignmentOptions.Center, 692f, 50f);
        }

        void BuildPause()
        {
            var card = Card("PauseScreen", new Vector2(820f, 640f), out pauseScreen);
            SmallChip(card, "ENTRACTE", new Color(0.184f, 0.435f, 0.408f, 0.18f), 54f);
            var title = Label(card, "Pause, on souffle.", 56f, Ink, TextAlignmentOptions.Center, 112f, 70f);
            title.fontStyle = FontStyles.Bold;
            Label(card, "Le chrono est suspendu. Le village retient son souffle.", 24f, Soft, TextAlignmentOptions.Center, 186f, 44f);
            pauseStats = Label(card, "", 28f, Teal, TextAlignmentOptions.Center, 254f, 130f);
            pauseStats.fontStyle = FontStyles.Bold;
            Btn(card, "REPRENDRE", new Vector2(0f, 196f), new Vector2(540f, 82f), Coral, Cream, 28f)
                .onClick.AddListener(() => { if (game != null) game.TogglePause(); });
            Btn(card, "RECOMMENCER", new Vector2(0f, 106f), new Vector2(540f, 74f), Cream, Ink, 24f)
                .onClick.AddListener(() => { if (game != null) game.Restart(); });
            Btn(card, "MENU PRINCIPAL", new Vector2(0f, 22f), new Vector2(540f, 70f), Cream, Soft, 22f)
                .onClick.AddListener(() => { if (game != null) game.QuitToMenu(); });
            ShowPause(false);
        }

        void BuildClear()
        {
            var card = Card("ClearScreen", new Vector2(860f, 740f), out clearScreen);
            clearChip = Label(card, "NIVEAU RÉUSSI", 20f, Coral, TextAlignmentOptions.Center, 46f, 32f);
            clearChip.fontStyle = FontStyles.Bold;
            clearTitle = Label(card, "Tous démasqués !", 58f, Ink, TextAlignmentOptions.Center, 84f, 80f);
            clearTitle.fontStyle = FontStyles.Bold;
            clearBody = Label(card, "", 26f, Soft, TextAlignmentOptions.Center, 178f, 300f);
            clearScore = Label(card, "", 54f, Ink, TextAlignmentOptions.Center, 490f, 70f);
            clearScore.fontStyle = FontStyles.Bold;
            Btn(card, "NIVEAU SUIVANT", new Vector2(0f, 176f), new Vector2(580f, 86f), Coral, Cream, 30f)
                .onClick.AddListener(() => { if (game != null) game.NextLevel(); });
            Btn(card, "ARRÊTER ET ENREGISTRER", new Vector2(0f, 78f), new Vector2(400f, 68f), Cream, Soft, 20f)
                .onClick.AddListener(() => { if (game != null) game.EndRun(); });
            ShowLevelClear(false);
        }

        void BuildOver()
        {
            var card = Card("OverScreen", new Vector2(900f, 800f), out overScreen);
            overChip = Label(card, "FIN DE LA PARTIE", 20f, Coral, TextAlignmentOptions.Center, 44f, 32f);
            overChip.fontStyle = FontStyles.Bold;
            overTitle = Label(card, "Le sablier a parlé.", 54f, Ink, TextAlignmentOptions.Center, 82f, 76f);
            overTitle.fontStyle = FontStyles.Bold;
            overBody = Label(card, "", 28f, Mustard, TextAlignmentOptions.Center, 166f, 120f);
            overBody.fontStyle = FontStyles.Bold;
            overList = Label(card, "", 22f, Soft, TextAlignmentOptions.Center, 300f, 240f);
            Btn(card, "REJOUER", new Vector2(0f, 176f), new Vector2(560f, 86f), Coral, Cream, 30f)
                .onClick.AddListener(() => { if (game != null) game.Restart(); });
            Btn(card, "RETOUR AU MENU", new Vector2(0f, 76f), new Vector2(360f, 68f), Cream, Soft, 22f)
                .onClick.AddListener(() => { if (game != null) game.QuitToMenu(); });
            ShowGameOver(null);
        }

        /* ---------------- public API ---------------- */

        public Transform CreateReticle()
        {
            var go = new GameObject("Reticle");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSprites.Ring;
            renderer.color = Cream;
            renderer.sortingOrder = 100;
            go.transform.localScale = Vector3.one * 0.85f;
            go.SetActive(false);
            return go.transform;
        }

        public void ShowMenu(bool visible = true)
        {
            if (menuScreen != null) menuScreen.SetActive(visible);
            if (visible) RefreshMenu();
        }

        public void RefreshMenu()
        {
            if (game == null || menuRecords == null) return;
            var entries = game.Scores;
            menuRecords.text = "Meilleur score : " + game.Best.ToString("N0");
            if (entries.Count > 0)
            {
                menuRecords.text += "\n";
                for (var i = 0; i < Mathf.Min(3, entries.Count); i++)
                    menuRecords.text += (i + 1) + ". " + entries[i].score.ToString("N0") + "  (niv. " + entries[i].level + ")    ";
            }
        }

        public void ShowHud(bool visible)
        {
            if (hudRoot != null) hudRoot.SetActive(visible);
        }

        public void ShowPause(bool visible)
        {
            if (pauseScreen == null) return;
            if (visible && game != null && pauseStats != null)
                pauseStats.text = "Score " + game.Score.ToString("N0") + "    •    Niveau " + game.Level + "    •    " + game.Hits + " trouvés";
            pauseScreen.SetActive(visible);
        }

        public void ShowLevelClear(bool visible)
        {
            if (clearScreen != null) clearScreen.SetActive(visible);
        }

        public void ShowLevelClear(LevelResult result, int level, int nextHints, int nextTarget, string nextTheme)
        {
            if (clearScreen == null) return;
            clearChip.text = "NIVEAU " + level + " RÉUSSI";
            clearTitle.text = "Tous démasqués !";
            clearBody.text = "Temps restant  " + result.secondsLeft + " s  →  +" + result.timeBonus.ToString("N0") +
                             "\nNiveau terminé  →  +" + result.levelBonus.ToString("N0") +
                             (result.perfectBonus > 0 ? "\nSans faute  →  +" + result.perfectBonus.ToString("N0") : "") +
                             "\n\nProchaine escale : " + nextTheme +
                             "\n" + nextTarget + " farceurs  •  " + Mathf.Min(CacheCacheData.MaxHints, nextHints) + " indices";
            clearScore.text = "Score " + game.Score.ToString("N0");
            clearScreen.SetActive(true);
        }

        public void ShowGameOver(GameOverData data)
        {
            if (overScreen == null) return;
            if (data == null)
            {
                overScreen.SetActive(false);
                return;
            }
            overChip.text = data.isBest ? "NOUVEAU RECORD !" : "FIN DE LA PARTIE";
            overChip.color = data.isBest ? Mustard : Coral;
            overTitle.text = data.isBest ? "Tu es un as de la cachette." : "Le sablier a parlé.";
            overBody.text = data.score.ToString("N0") + " pts    •    niveau " + data.level + "    •    " + data.found + " farceurs";
            overList.text = FormatScores(data.entries, data.rank);
            overScreen.SetActive(true);
        }

        string FormatScores(List<ScoreEntry> entries, int rank)
        {
            if (entries == null || entries.Count == 0) return "Aucun record pour le moment.";
            var text = "TABLEAU DES RECORDS\n";
            for (var i = 0; i < Mathf.Min(5, entries.Count); i++)
            {
                text += (i + 1) + ".  " + entries[i].score.ToString("N0") + " pts   niveau " + entries[i].level;
                if (i == rank) text += "   ← toi";
                text += "\n";
            }
            return text;
        }

        public void RebuildRoster(List<HideSpot> spots)
        {
            if (rosterRoot == null) return;
            rosterNames.Clear();
            rosterBgs.Clear();
            rosterPortraits.Clear();
            for (var i = rosterRoot.transform.childCount - 1; i >= 0; i--)
            {
                var old = rosterRoot.transform.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
            if (spots == null) return;

            for (var i = 0; i < spots.Count; i++)
            {
                var chip = Rect("Chip" + i, rosterRoot.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f + i * 214f, -10f), new Vector2(202f, 72f));
                var bg = Img(chip, new Color(1f, 1f, 1f, 0.65f));
                var portrait = Rect("Portrait", chip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(52f, 52f));
                var portraitImg = Img(portrait, spots[i].look.skin, false);
                portraitImg.sprite = RuntimeSprites.Circle;
                var hair = Rect("Hair", portrait, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(54f, 26f));
                Img(hair, spots[i].look.hair);
                var name = Rect("Name", chip, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(34f, -12f), new Vector2(-70f, 30f)).gameObject.AddComponent<TextMeshProUGUI>();
                name.font = font;
                name.text = "???";
                name.fontSize = 21f;
                name.color = Ink;
                name.fontStyle = FontStyles.Bold;
                name.raycastTarget = false;
                var status = Rect("Status", chip, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(34f, -42f), new Vector2(-70f, 24f)).gameObject.AddComponent<TextMeshProUGUI>();
                status.font = font;
                status.text = "CACHÉ";
                status.fontSize = 15f;
                status.color = Soft;
                status.fontStyle = FontStyles.Bold;
                status.raycastTarget = false;

                rosterBgs.Add(bg);
                rosterPortraits.Add(portrait);
                rosterNames.Add(name);
            }
            RefreshRoster(spots);
        }

        public void RefreshRoster(List<HideSpot> spots)
        {
            if (spots == null) return;
            for (var i = 0; i < spots.Count && i < rosterNames.Count; i++)
            {
                var found = spots[i].Found;
                rosterNames[i].text = found ? spots[i].look.displayName : "???";
                rosterNames[i].color = found ? Teal : Ink;
                rosterBgs[i].color = found ? new Color(0.184f, 0.435f, 0.408f, 0.18f) : new Color(1f, 1f, 1f, 0.65f);
            }
        }

        public void RefreshHud()
        {
            if (game == null || scoreText == null) return;
            if (lastScore != game.Score)
            {
                lastScore = game.Score;
                scoreText.text = game.Score.ToString("N0");
            }
            bestText.text = "record " + game.Best.ToString("N0");
            themeText.text = game.ThemeName;
            levelText.text = game.Level.ToString();
            foundText.text = game.FoundCount + " / " + game.TargetTotal + " trouvés";
            streakText.text = game.Streak + "  ×" + game.Multiplier.ToString("0.#");
            hintsLabel.text = game.Hints.ToString();
            subtitleText.text = "Série ×" + game.Multiplier.ToString("0.#") + "  •  " + game.Hints + " indice(s)";
        }

        int lastDisplayedSecond = -1;

        public void RefreshTimerOnly(float time, float total)
        {
            if (timeText == null) return;
            var seconds = Mathf.CeilToInt(time);
            if (seconds != lastDisplayedSecond)
            {
                lastDisplayedSecond = seconds;
                timeText.text = seconds.ToString();
                timeText.color = seconds <= 10 ? Coral : Ink;
                timeText.rectTransform.localScale = Vector3.one;
                if (timeFill != null) timeFill.color = seconds <= 10 ? Coral : Teal;
            }
            if (timeFill != null) timeFill.fillAmount = total <= 0f ? 0f : Mathf.Clamp01(time / total);
        }

        public void PulseTimer()
        {
            if (timeText == null) return;
            var s = 1f + Mathf.Sin(Time.unscaledTime * 9f) * 0.08f;
            timeText.rectTransform.localScale = new Vector3(s, s, 1f);
        }

        public void SetMuted(bool muted)
        {
            if (muteLabel != null) muteLabel.text = muted ? "OFF" : "ON";
            if (menuSoundLabel != null) menuSoundLabel.text = muted ? "SON : OFF" : "SON : ON";
        }

        public void SpawnFloat(Vector3 world, string message, Color color, bool miss = false)
        {
            if (canvas == null) return;
            var cam = Camera.main;
            if (cam == null) return;
            var screen = cam.WorldToScreenPoint(world + Vector3.up * 0.5f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local);
            var rt = Rect("Float", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), local, new Vector2(400f, 70f));
            var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = message;
            text.fontSize = miss ? 30f : 40f;
            text.color = color;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            StartCoroutine(FloatRoutine(text, local));
        }

        IEnumerator FloatRoutine(TextMeshProUGUI text, Vector2 start)
        {
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                text.rectTransform.anchoredPosition = start + Vector2.up * Mathf.Lerp(20f, 120f, t);
                var c = text.color;
                c.a = 1f - Mathf.Pow(t, 2f);
                text.color = c;
                yield return null;
            }
            Destroy(text.gameObject);
        }
    }
}
