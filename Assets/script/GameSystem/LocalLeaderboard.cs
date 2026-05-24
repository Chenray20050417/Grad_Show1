using System;
using System.Collections.Generic;
using UnityEngine;

public static class LocalLeaderboard
{
    private const string PlayerPrefsKey = "SweatFactoryLocalLeaderboard";
    private const int MaxEntries = 50;

    [Serializable]
    public class Entry
    {
        public string playerName;
        public int score;
        public int brokenStressBalls;
        public int usedSupplements;
        public int hurtCount;
        public string date;
    }

    [Serializable]
    private class LeaderboardData
    {
        public List<Entry> entries = new List<Entry>();
    }

    public static int CalculateScore(int brokenStressBalls, int usedSupplements, int hurtCount)
    {
        int score = brokenStressBalls * 100 + usedSupplements * 30 - hurtCount * 50;
        return Mathf.Max(0, score);
    }

    public static void AddCurrentRun(string playerName)
    {
        AddEntry(
            playerName,
            GameStats.BrokenStressBalls,
            GameStats.UsedSupplements,
            GameStats.HurtCount
        );
    }

    public static void AddEntry(string playerName, int brokenStressBalls, int usedSupplements, int hurtCount)
    {
        LeaderboardData data = LoadData();

        Entry entry = new Entry
        {
            playerName = CleanName(playerName),
            brokenStressBalls = brokenStressBalls,
            usedSupplements = usedSupplements,
            hurtCount = hurtCount,
            score = CalculateScore(brokenStressBalls, usedSupplements, hurtCount),
            date = DateTime.Now.ToString("yyyy/MM/dd")
        };

        data.entries.Add(entry);
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (data.entries.Count > MaxEntries)
            data.entries.RemoveRange(MaxEntries, data.entries.Count - MaxEntries);

        SaveData(data);
    }

    public static List<Entry> GetEntries()
    {
        return new List<Entry>(LoadData().entries);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
    }

    private static LeaderboardData LoadData()
    {
        string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new LeaderboardData();

        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        return data ?? new LeaderboardData();
    }

    private static void SaveData(LeaderboardData data)
    {
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private static string CleanName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return "PLAYER";

        playerName = playerName.Trim();

        if (playerName.Length > 12)
            playerName = playerName.Substring(0, 12);

        return playerName;
    }
}
