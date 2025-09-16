using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "DroppedItemDataSO", menuName = "Scriptable Objects/DroppedItemDataSO")]
public class DroppedItemDataSO : ScriptableObject
{
  public Dictionary<DroppedItemType, Sprite> sprites;


  private void OnEnable()
  {
    sprites = new();
    Init();
  }

  private async void Init()
  {
    var handle = Addressables.LoadAssetsAsync<Sprite>("droppeditem");
    IList<Sprite> loadedSprites = await handle.Task;
    foreach (Sprite s in loadedSprites)
    {
      switch (s.name)
      {
        case "금화_0":
          sprites.Add(DroppedItemType.Gold, s);
          break;
        case "혼백_0":
          sprites.Add(DroppedItemType.SpiritBack, s);
          break;
        case "영석_0":
          sprites.Add(DroppedItemType.SoulStone, s);
          break;
        case "일반상자":
          sprites.Add(DroppedItemType.NormalChest, s);
          break;
        case "레어상자":
          sprites.Add(DroppedItemType.RareChest, s);
          break;
      }
    }
  }
}


public enum DroppedItemType
{
  Gold,
  SpiritBack,
  SoulStone,
  NormalChest,
  RareChest
}
