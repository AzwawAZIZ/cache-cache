using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CacheCache
{
    public enum GameState { Menu, Playing, Paused, LevelClear, GameOver }

    public struct LevelResult
    {
        public int secondsLeft, timeBonus, levelBonus, perfectBonus, total;
    }

    public class GameOverData
    {
        public int score, level, found, rank;
        public bool isBest;
        public List<ScoreEntry> entries;
    }

    /// <summary>Machine à états centrale — équivalent de App.tsx + rounds.ts.</summary>
    public class GameManager : MonoBehaviour
    {
        public GameState State { get; private set; } = GameState.Menu;
        public int Score { get; private set; }
        public int Level { get; private set; } = 1;
        public int Streak { get; private set; }
        public int Hits { get; private set; }
        public int Hints { get; private set; } = CacheCacheData.StartHints;
        public int FoundCount { get; private set; }
        public int TargetTotal { get; private set; }
        public float TimeLeft { get; private set; }
        public bool TimedOut => State == GameState.GameOver && TimeLeft <= 0f;
        public float Multiplier => CacheCacheData.MultiplierFor(Streak);
        public string ThemeName => theme != null ? theme.displayName : "";
        public int Best => HighScores.Best();
        public List<ScoreEntry> Scores => HighScores.Top();
        public List<HideSpot> Spots { get; private set; } = new List<HideSpot>();

        Camera cam;
        UIManager ui;
        AudioManager audioManager;
        Juice juice;
        RoundBuilder builder;
        SceneTheme theme;
        int misses;
        int lastTick = -1;
        Coroutine hintRoutine;
        Coroutine completeRoutine;

        public void Init(Camera camera, UIManager uiManager, AudioManager audio, Juice fx, RoundBuilder roundBuilder)
        {
            cam = camera;
            ui = uiManager;
            audioManager = audio;
            juice = fx;
            builder = roundBuilder;
        }

        public void ShowMenuAtStart()
        {
            State = GameState.Menu;
            ui.ShowMenu();
        }

        public void StartRun()
        {
            StopAllCoroutines();
            hintRoutine = null;
            completeRoutine = null;
            ui.ShowPause(false);
            ui.ShowLevelClear(false);
            ui.ShowGameOver(null);
            audioManager.PlayStart();
            Score = 0;
            Level = 1;
            Hits = 0;
            Hints = CacheCacheData.StartHints;
            StartLevel();
        }

        void StartLevel()
        {
            var themes = CacheCacheData.Themes();
            var themeIndex = (Level - 1) % themes.Length;
            theme = themes[themeIndex];
            TargetTotal = Mathf.Min(CacheCacheData.TargetForLevel(Level), theme.props.Length);
            TimeLeft = CacheCacheData.TimeForLevel(Level);
            FoundCount = 0;
            Streak = 0;
            misses = 0;
            lastTick = -1;
            Hints = Mathf.Clamp(Hints, 0, CacheCacheData.MaxHints);

            var looks = new List<CharacterLook>(CacheCacheData.Looks());
            Shuffle(looks);
            var indexes = new List<int>();
            for (var i = 0; i < theme.props.Length; i++) indexes.Add(i);
            Shuffle(indexes);
            var chosen = new List<int>();
            for (var i = 0; i < TargetTotal; i++) chosen.Add(indexes[i]);

            Spots = builder.Build(theme, themeIndex, looks.GetRange(0, TargetTotal), chosen);

            State = GameState.Playing;
            ui.ShowMenu(false);
            ui.ShowGameOver(null);
            ui.RebuildRoster(Spots);
            ui.RefreshHud();
            ui.ShowHud(true);
        }

        public void NextLevel()
        {
            if (State != GameState.LevelClear) return;
            ui.ShowLevelClear(false);
            Level++;
            Hints = Mathf.Min(CacheCacheData.MaxHints, Hints + 1);
            StartLevel();
        }

        public void Restart()
        {
            Level = 1;
            Hints = CacheCacheData.StartHints;
            ui.ShowPause(false);
            ui.ShowLevelClear(false);
            ui.ShowGameOver(null);
            StartRun();
        }

        public void QuitToMenu()
        {
            StopAllCoroutines();
            builder.Clear();
            Spots.Clear();
            ui.ShowHud(false);
            ui.ShowMenu(true);
            ui.ShowGameOver(null);
            ui.ShowLevelClear(false);
            ui.ShowPause(false);
            State = GameState.Menu;
            audioManager.PlayClick();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                ui.ShowPause(true);
                audioManager.PlayClick();
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                ui.ShowPause(false);
                audioManager.PlayClick();
            }
        }

        public void ToggleMute()
        {
            ui.SetMuted(audioManager.ToggleMute());
        }

        public void UseHint()
        {
            if (State != GameState.Playing || Hints <= 0) return;
            HideSpot candidate = null;
            foreach (var spot in Spots)
                if (!spot.Found) { candidate = spot; break; }
            if (candidate == null) return;

            var remaining = new List<HideSpot>();
            foreach (var spot in Spots) if (!spot.Found) remaining.Add(spot);
            candidate = remaining[Random.Range(0, remaining.Count)];

            Hints--;
            foreach (var spot in Spots) spot.Hint(false);
            candidate.Hint(true);
            audioManager.PlayHint();
            ui.RefreshHud();
            if (hintRoutine != null) StopCoroutine(hintRoutine);
            hintRoutine = StartCoroutine(ClearHint(candidate));
        }

        IEnumerator ClearHint(HideSpot spot)
        {
            yield return new WaitForSeconds(CacheCacheData.HintDuration);
            if (spot != null) spot.Hint(false);
        }

        public void InspectAt(Vector3 world)
        {
            if (State != GameState.Playing) return;

            HideSpot hit = null;
            HideSpot near = null;
            var bestDistance = float.MaxValue;
            foreach (var spot in Spots)
            {
                var distance = Vector2.Distance(spot.HeadPosition, world);
                if (!spot.Found && distance <= spot.HitRadius && distance < bestDistance)
                {
                    bestDistance = distance;
                    hit = spot;
                }
                if (!spot.Found && distance <= spot.HitRadius * 1.8f) near = spot;
            }

            if (hit == null)
            {
                foreach (var spot in Spots)
                    if (spot.Found && Vector2.Distance(spot.HeadPosition, world) <= spot.HitRadius * 2f) return;

                if (near != null)
                {
                    audioManager.PlayNear();
                    juice.Burst(world, new Color(0.93f, 0.88f, 0.76f), 8);
                    ui.SpawnFloat(world, "Presque !", new Color(1f, 0.93f, 0.8f), true);
                    return;
                }

                Streak = 0;
                misses++;
                TimeLeft = Mathf.Max(0f, TimeLeft - CacheCacheData.MissPenalty);
                audioManager.PlayMiss();
                juice.Burst(world, new Color(0.7f, 0.58f, 0.44f), 10);
                juice.Shake(0.5f);
                ui.SpawnFloat(world, "-" + CacheCacheData.MissPenalty.ToString("0") + "s", new Color(1f, 0.82f, 0.76f), true);
                ui.RefreshHud();
                if (TimeLeft <= 0f) GameOver();
                return;
            }

            Streak++;
            FoundCount++;
            Hits++;
            var gain = Mathf.RoundToInt(CacheCacheData.BasePoints * Multiplier);
            Score += gain;
            hit.MarkFound();
            audioManager.PlayFind(Streak);
            juice.Burst(hit.HeadPosition, hit.look.accent, 30);
            juice.Shake(1f);
            var label = "+" + gain + (Multiplier > 1f ? " x" + Multiplier.ToString("0.#") : "");
            ui.SpawnFloat(hit.HeadPosition, label, new Color(1f, 0.97f, 0.86f));
            ui.RefreshHud();
            ui.RefreshRoster(Spots);

            if (FoundCount >= TargetTotal)
            {
                State = GameState.LevelClear;
                if (completeRoutine != null) StopCoroutine(completeRoutine);
                completeRoutine = StartCoroutine(CompleteLevel());
            }
        }

        IEnumerator CompleteLevel()
        {
            yield return new WaitForSeconds(0.9f);
            var result = new LevelResult
            {
                secondsLeft = Mathf.Max(0, Mathf.CeilToInt(TimeLeft)),
                timeBonus = Mathf.Max(0, Mathf.CeilToInt(TimeLeft)) * 8,
                levelBonus = 50 + Level * 25,
                perfectBonus = misses == 0 ? 150 : 0
            };
            result.total = result.timeBonus + result.levelBonus + result.perfectBonus;
            Score += result.total;
            audioManager.PlayLevelClear();
            ui.RefreshHud();
            ui.ShowLevelClear(result, Level, Hints + 1, CacheCacheData.TargetForLevel(Level + 1),
                CacheCacheData.Themes()[Level % CacheCacheData.Themes().Length].displayName);
        }

        /// <summary>Termine la partie en cours et enregistre le score (bouton "Arrêter ici").</summary>
        public void EndRun()
        {
            if (State == GameState.LevelClear) GameOver();
        }

        void GameOver()
        {
            if (State == GameState.GameOver) return;
            State = GameState.GameOver;
            ui.ShowLevelClear(false);
            var submission = HighScores.Submit(Score, Level, Hits);
            audioManager.PlayGameOver();
            ui.ShowGameOver(new GameOverData
            {
                score = Score,
                level = Level,
                found = Hits,
                rank = submission.rank,
                isBest = submission.isBest,
                entries = submission.entries
            });
        }

        void Update()
        {
            if (State == GameState.Playing)
            {
                TimeLeft = Mathf.Max(0f, TimeLeft - Time.deltaTime);
                var seconds = Mathf.CeilToInt(TimeLeft);
                if (seconds <= 5 && seconds > 0 && seconds != lastTick)
                {
                    lastTick = seconds;
                    audioManager.PlayTick(seconds <= 3);
                }
                else if (seconds > 5) lastTick = seconds;

                ui.RefreshTimerOnly(TimeLeft, TimeForLevel(Level));
                if (TimeLeft > 0f && TimeLeft < 1.2f) ui.PulseTimer();
                if (TimeLeft <= 0f) GameOver();
            }
        }

        public float TimeForLevel(int level) => CacheCacheData.TimeForLevel(level);

        void OnApplicationPause(bool paused)
        {
            if (paused && State == GameState.Playing) TogglePause();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused && State == GameState.Playing) TogglePause();
        }

        static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
