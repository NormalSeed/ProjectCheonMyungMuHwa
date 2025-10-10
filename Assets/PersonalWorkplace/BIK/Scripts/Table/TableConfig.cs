using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TableConfig", menuName = "Configs/TableConfig")]
public class TableConfig : ScriptableObject
{
    [Serializable]
    public class TableEntry
    {
        public TableType type;
        public string sheetUrl;
    }

    [SerializeField] private List<TableEntry> _tables = new();

    private static TableConfig _instance;
    public static TableConfig Instance {
        get {
            if (_instance == null)
                _instance = Resources.Load<TableConfig>("TableConfig");
            return _instance;
        }
    }

    public string GetUrl(TableType type)
    {
        foreach (var entry in _tables)
            if (entry.type == type)
                return entry.sheetUrl;
        return null;
    }

    public List<TableEntry> GetAll() => _tables;
}
