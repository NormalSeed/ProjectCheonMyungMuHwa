using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ForbiddenWordManager
{
    private List<string> _forbiddenWords = new();
    private bool _isReady = false;

    public ForbiddenWordManager()
    {
        LoadFromCSV("BadWord");
    }

    private void LoadFromCSV(string resourcePath)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile == null)
        {
            Debug.LogError($"[ForbiddenWordManager] CSV 파일을 찾을 수 없습니다: {resourcePath}");
            return;
        }

        var lines = csvFile.text.Split('\n');
        _forbiddenWords = lines
            .Skip(1) // 헤더 제거
            .Select(line => line.Trim().ToLower())
            .Where(word => !string.IsNullOrEmpty(word))
            .Distinct()
            .ToList();

        _isReady = true;
        Debug.Log($"[SimpleForbiddenWordManager] 금지어 {_forbiddenWords.Count}개 로드 완료");
    }

    public bool ContainsForbiddenWord(string nickname)
    {
        if (!_isReady) return false;
        if (string.IsNullOrWhiteSpace(nickname)) return false;

        string lower = nickname.Trim().ToLower();
        foreach (var word in _forbiddenWords)
        {
            if (lower.Contains(word))
            {
                Debug.Log($"[SimpleForbiddenWordManager] 금지어 발견: {word}");
                return true;
            }
        }

        return false;
    }
}
