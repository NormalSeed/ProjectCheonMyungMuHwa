using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

// 재화던전 UI관련 세팅 값들을 저장할 용도의 스크립터블 오브젝트

[CreateAssetMenu(fileName = "CurrencyDungeonDict", menuName = "Scriptable Objects/CurrencyDungeonDict")]
public class CurrencyDungeonDict : ScriptableObject
{
  [SerializeField] int goldLevelCount;
  [SerializeField] int HonbaegLevelCount;
  [SerializeField] int spriteLevelCount;
  [SerializeField] CurrencyDungeonDataTableSO goldTable;
  [SerializeField] CurrencyDungeonDataTableSO honbaegTable;
  [SerializeField] CurrencyDungeonDataTableSO spiritTable;

  public Dictionary<CurrencyDungeonType, int> DungeonCounts;
  public Dictionary<CurrencyDungeonType, CurrencyDungeonDataTableSO> DungeonTables;

  public Dictionary<CurrencyDungeonType, Sprite> DungeonSprites;


  void OnEnable()
  {
    DungeonCounts = new Dictionary<CurrencyDungeonType, int>()
    {
      {CurrencyDungeonType.Gold, goldLevelCount},
      {CurrencyDungeonType.Honbaeg, HonbaegLevelCount},
      {CurrencyDungeonType.Spirit, spriteLevelCount},
    };
    DungeonTables = new Dictionary<CurrencyDungeonType, CurrencyDungeonDataTableSO>()
    {
      {CurrencyDungeonType.Gold, goldTable},
      {CurrencyDungeonType.Honbaeg, honbaegTable},
      {CurrencyDungeonType.Spirit, spiritTable},
    };
    DungeonSprites = new();
    InitSprites();


  }
  private async void InitSprites()
  {
    var handle = Addressables.LoadAssetsAsync<Sprite>("droppeditem");
    IList<Sprite> loadedSprites = await handle.Task;
    foreach (Sprite s in loadedSprites)
    {
      switch (s.name)
      {
        case "금화_0":
          DungeonSprites.Add(CurrencyDungeonType.Gold, s);
          break;
        case "혼백_0":
          DungeonSprites.Add(CurrencyDungeonType.Honbaeg, s);
          break;
        case "영석_0":
          DungeonSprites.Add(CurrencyDungeonType.Spirit, s);
          break;
      }
    }
  }
}
