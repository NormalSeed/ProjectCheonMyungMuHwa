using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class KMS_Util
{
  [MenuItem("Utilities/Generate Gold Dungeon Data Scriptable Object")]
  public static void GenerateGoldDungeonDataTable()
  {
    GenerateCurrencyDungeonDataTable("/PersonalWorkplace/KMS/CSV/Gold.csv", CurrencyDungeonType.Gold);
  }
  [MenuItem("Utilities/Generate honbaeg Dungeon Data Scriptable Object")]
  public static void GeneratehonbaegDungeonDataTable()
  {
    GenerateCurrencyDungeonDataTable("/PersonalWorkplace/KMS/CSV/Honbaeg.csv", CurrencyDungeonType.Honbaeg);
  }
  [MenuItem("Utilities/Generate spirit Dungeon Data Scriptable Object")]
  public static void GenerateSpiritDungeonDataTable()
  {
    GenerateCurrencyDungeonDataTable("/PersonalWorkplace/KMS/CSV/Spirit.csv", CurrencyDungeonType.Spirit);
  }
  private static void GenerateCurrencyDungeonDataTable(string path, CurrencyDungeonType type)
  {
    string[] allLines = File.ReadAllLines(Application.dataPath + path);
    CurrencyDungeonDataTableSO so = ScriptableObject.CreateInstance<CurrencyDungeonDataTableSO>();
    for (int i = 1; i < allLines.Length; i++)
    {
      string[] splitData = allLines[i].Split(',');
      CurrencyDungeonData data = new CurrencyDungeonData();
      data.Level = int.Parse(splitData[0]);
      data.Name = $"{DungeonTypeToName[type]} {splitData[0]}단계";
      data.BattlePower = double.Parse(splitData[2]);
      data.Reward = double.Parse(splitData[3]);
      so.Table.Add(data);
    }
    AssetDatabase.CreateAsset(so, $"Assets/PersonalWorkplace/KMS/Script/SO/table.asset");

    AssetDatabase.SaveAssets();
  }

  public static Dictionary<CurrencyDungeonType, string> DungeonTypeToName = new Dictionary<CurrencyDungeonType, string>()
  {
    {CurrencyDungeonType.Gold, "금화"},
    {CurrencyDungeonType.Honbaeg, "혼백"},
    {CurrencyDungeonType.Spirit, "영석"}
  };
}
