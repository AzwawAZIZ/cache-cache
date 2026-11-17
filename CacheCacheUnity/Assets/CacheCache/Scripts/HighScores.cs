using System;
using System.Collections.Generic;
using UnityEngine;

namespace CacheCache
{
    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public int level;
        public int found;
        public string date;
    }

    [Serializable]
    public class ScoreTable
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    /// <summary>Top 7 local, sauvegardé en JSON dans PlayerPrefs.</summary>
    public static class HighScores
    {
        const string Key = "cache-cache:highscores:v1";
        public const int MaxEntries = 7;

        public static List<ScoreEntry> Top()
        {
            if (!PlayerPrefs.HasKey(Key)) return new List<ScoreEntry>();
            var json = PlayerPrefs.GetString(Key, "{}");
            ScoreTable table;
            try { table = JsonUtility.FromJson<ScoreTable>(json); }
            catch (ArgumentException) { return new List<ScoreEntry>(); }
            var list = table != null && table.entries != null ? table.entries : new List<ScoreEntry>();
            list.RemoveAll(entry => entry == null);
            list.Sort((a, b) => b.score.CompareTo(a.score));
            return list.Count <= MaxEntries ? list : list.GetRange(0, MaxEntries);
        }

        public static int Best()
        {
            var list = Top();
            return list.Count > 0 ? list[0].score : 0;
        }

        public static (List<ScoreEntry> entries, int rank, bool isBest) Submit(int score, int level, int found)
        {
            var list = Top();
            var entry = new ScoreEntry { score = Mathf.Max(0, score), level = Mathf.Max(1, level), found = Mathf.Max(0, found), date = DateTime.UtcNow.ToString("o") };
            list.Add(entry);
            list.RemoveAll(entry => entry == null);
            list.Sort((a, b) => b.score.CompareTo(a.score));
            if (list.Count > MaxEntries) list.RemoveRange(MaxEntries, list.Count - MaxEntries);
            var rank = list.FindIndex(e => ReferenceEquals(e, entry));
            var table = new ScoreTable { entries = list };
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(table));
            PlayerPrefs.Save();
            return (list, rank, rank == 0 && score > 0);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
