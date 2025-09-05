using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TableManager : IStartable
{
    private readonly Dictionary<TableType, ITable> _tables = new();

    public bool AllInitialized {
        get {
            foreach (var kv in _tables)
                if (!kv.Value.IsInitialized)
                    return false;
            return true;
        }
    }

    public void Start()
    {
        LoadAllTables();
    }

    private void LoadAllTables()
    {
        var all = TableConfig.Instance.GetAll();
        Debug.Log($"[TableManager] LoadAllTables: config count = {all.Count}");

        foreach (var entry in all) {
            Debug.Log($"[TableManager] entry: {entry.type}, url={entry.sheetUrl}");
            ITable table = CreateTable(entry.type);
            if (table == null) {
                Debug.LogWarning($"[TableManager] {entry.type} 테이블 클래스 미구현");
                continue;
            }

            // 1) 딕셔너리에 먼저 넣고
            _tables[entry.type] = table;

            try {
                // 2) 그 다음 Load (예외 나도 _tables는 유지)
                table.Load(entry.sheetUrl);
            }
            catch (System.Exception e) {
                Debug.LogError($"[TableManager] {entry.type} Load() 예외: {e}");
            }
        }

        Debug.Log($"[TableManager] after loop, _tables.Count = {_tables.Count}");
    }


    private ITable CreateTable(TableType type)
    {
        return type switch {
            TableType.Item => new TItem(), // 여기에 새로운 테이블 타입이 추가될 때마다 case를 추가
            _ => null
        };
    }

    public T GetTable<T>(TableType type) where T : class, ITable
    {
        return _tables.TryGetValue(type, out var table) ? table as T : null;
    }
}
