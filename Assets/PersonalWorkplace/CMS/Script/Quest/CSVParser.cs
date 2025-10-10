using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CSVParser
{
    /// <summary>
    /// CSV 파일을 파싱해서 string[] 리스트로 반환
    /// 첫 줄(헤더)은 자동으로 스킵
    /// </summary>
    public static List<string[]> Parse(TextAsset csvFile)
    {
        var result = new List<string[]>();
        if (csvFile == null)
        {
            Debug.LogError("CSVParser: csvFile이 null 입니다");
            return result;
        }

        // 줄 단위로 나누기 (윈도우 \r\n, 맥 \n 모두 대응)
        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSVParser: 데이터가 없습니다");
            return result;
        }

        // 0번 줄은 헤더니까 skip
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = line.Split(',');
            result.Add(values.Select(v => v.Trim()).ToArray());
        }

        return result;
    }
}