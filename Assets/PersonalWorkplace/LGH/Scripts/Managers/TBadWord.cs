using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class TBadWord : ITable
{
    public bool IsInitialized { get; private set; } = false;

    public event Action OnLoaded;

    private List<string> _badWords = new();

    public IReadOnlyList<string> BadWords => _badWords;

    public void Load(string url)
    {
        CoroutineRunner.instance.StartCoroutine(LoadFromSheet(url));
    }

    private IEnumerator LoadFromSheet(string url)
    {
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ParseCSV(req.downloadHandler.text);
            IsInitialized = true;
            OnLoaded?.Invoke();
        }
        else
        {
            Debug.LogError("[TBadWord] 금지어 테이블 로딩 실패: " + req.error);
        }
    }

    private void ParseCSV(string csv)
    {
        var lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째 줄은 "Words" 헤더니까 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string word = lines[i].Trim().ToLower();
            if (!string.IsNullOrEmpty(word) && !_badWords.Contains(word))
            {
                _badWords.Add(word);
            }
        }

        Debug.Log($"[TBadWord] 금지어 {_badWords.Count}개 로드됨");
    }

    public bool Contains(string nickname)
    {
        string lower = nickname.ToLower();
        foreach (var word in _badWords)
        {
            if (lower.Contains(word))
                return true;
        }
        return false;
    }
}
