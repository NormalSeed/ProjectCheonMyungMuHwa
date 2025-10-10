using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "DroppedItemDataSO", menuName = "Scriptable Objects/DroppedItemDataSO")]
public class DroppedItemDataSO : ScriptableObject
{
  public Dictionary<DroppedItemType, Sprite> sprites;

  [SerializeField] Sprite gold;
  [SerializeField] Sprite honbaeg;
  [SerializeField] Sprite spiritStone;

  [SerializeField] Sprite normalChest;
  [SerializeField] Sprite epicChest;


  private void OnEnable()
  {
    sprites = new Dictionary<DroppedItemType, Sprite>()
    {
     {DroppedItemType.Gold, gold },
     {DroppedItemType.Honbaeg, honbaeg },
     {DroppedItemType.SpiritStone, spiritStone },
     {DroppedItemType.NormalChest, normalChest },
     {DroppedItemType.EpicChest, epicChest }

    };
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
          sprites.Add(DroppedItemType.Honbaeg, s);
          break;
        case "영석_0":
          sprites.Add(DroppedItemType.SpiritStone, s);
          break;
        case "일반상자":
          sprites.Add(DroppedItemType.NormalChest, s);
          break;
        case "레어상자":
          sprites.Add(DroppedItemType.EpicChest, s);
          break;
      }
    }
  }
}


public enum DroppedItemType
{
  Gold,
  Honbaeg,
  SpiritStone,
  NormalChest,
  EpicChest
}
