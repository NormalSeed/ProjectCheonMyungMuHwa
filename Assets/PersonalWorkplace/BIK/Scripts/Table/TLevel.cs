using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class TLevel : ITable
{
    private Dictionary<int, LevelData> _levels = new();

    public bool IsInitialized { get; private set; } = false;

    public event Action OnLoaded; // 로딩 완료 이벤트

    public void Load(string url)
    {
        CoroutineRunner.instance.StartCoroutine(LoadFromSheet(url));
    }

    private IEnumerator LoadFromSheet(string url)
    {
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success) {
            ParseCSV(req.downloadHandler.text);
            IsInitialized = true;
            OnLoaded?.Invoke();
        }
        else {
            Debug.LogError("LevelTable load failed: " + req.error);
        }
    }

    private void ParseCSV(string csv)
    {
        var lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째 줄은 헤더니까 스킵
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            if (!int.TryParse(cols[0].Trim(), out int level)) {
                Debug.LogWarning($"[ParseCSV] 잘못된 Level 값: {cols[0]} (line {i})");
                continue;
            }

            if (!int.TryParse(cols[1].Trim(), out int exp)) {
                Debug.LogWarning($"[ParseCSV] 잘못된 Exp 값: {cols[1]} (line {i})");
                continue;
            }

            var data = new LevelData(level, exp);
            _levels[level] = data;
        }
    }

    public LevelData GetLevelData(int level)
    {
        return _levels.TryGetValue(level, out var data) ? data : null;
    }

    public int GetRequireExp(int level)
    {
        return _levels.TryGetValue(level, out var data) ? data.Exp : 0;
    }
}

public class LevelData
{
    public int Level { get; }
    public int Exp { get; } // 필요 경험치

    public LevelData(int level, int exp)
    {
        Level = level;
        Exp = exp;
    }
}
