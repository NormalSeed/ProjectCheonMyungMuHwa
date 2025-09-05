using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class TItem : ITable
{
    private Dictionary<int, ItemData> _items = new();

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
            Debug.LogError("ItemTable load failed: " + req.error);
        }
    }

    private void ParseCSV(string csv)
    {
        var lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0번째 줄은 무조건 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');
            if (cols.Length < 6) continue;

            if (!int.TryParse(cols[0].Trim(), out int id)) {
                Debug.LogWarning($"[ParseCSV] 잘못된 ID 값: {cols[0]} (line {i})");
                continue;
            }

            string name = cols[1].Trim();
            string detail = cols[2].Trim();
            string imageKey = cols[3].Trim();

            string stackStr = cols[4].Trim().ToLower();
            bool stackable = stackStr == "1" || stackStr == "true";

            ItemType type = Enum.TryParse(cols[5].Trim(), true, out ItemType parsedType)
                ? parsedType
                : ItemType.None;

            var item = new ItemData(id, name, detail, imageKey, stackable, type);
            _items[id] = item;
        }
    }


    public ItemData GetItem(int id)
    {
        return _items.TryGetValue(id, out var data) ? data : null;
    }

    public string GetIconKey(int id)
    {
        return _items.TryGetValue(id, out var data) ? data.ImageKey : null;
    }
}

public class ItemData
{
    public int Id { get; }                 // Item_ID
    public string Name { get; }            // Item_Name
    public string Detail { get; }          // Item_Detail (아이템 설명)
    public string ImageKey { get; }        // Item_Image (어드레서블 키나 리소스 경로)
    public bool Stackable { get; }         // Item_Stackable (true/false)
    public ItemType Type { get; }          // Item_Type (enum으로 관리 추천)

    public ItemData(
        int id,
        string name,
        string detail,
        string imageKey,
        bool stackable,
        ItemType type)
    {
        Id = id;
        Name = name;
        Detail = detail;
        ImageKey = imageKey;
        Stackable = stackable;
        Type = type;
    }
}
