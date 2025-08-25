using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "MonsterTextures", menuName = "Scriptable Objects/MonsterTextures")]
public class MonsterTextures : ScriptableObject
{
  public Dictionary<string, Sprite> Orc_1;
  public Dictionary<string, Sprite> Orc_2;
  public Dictionary<string, Sprite> Orc_3;
  public Dictionary<string, Sprite> Orc_4;

  void OnEnable()
  {
    Orc_1 = new();
    Orc_2 = new();
    Orc_3 = new();
    Orc_4 = new();
    IList<Object> orc1 = Addressables.LoadAssetAsync<IList<Object>>("Orc_1").WaitForCompletion();
    IList<Object> orc2 = Addressables.LoadAssetAsync<IList<Object>>("Orc_2").WaitForCompletion();
    IList<Object> orc3 = Addressables.LoadAssetAsync<IList<Object>>("Orc_3").WaitForCompletion();
    IList<Object> orc4 = Addressables.LoadAssetAsync<IList<Object>>("Orc_4").WaitForCompletion();
    foreach (var obj in orc1) if (obj is Sprite sprite) Orc_1.Add(sprite.name, sprite);
    foreach (var obj in orc2) if (obj is Sprite sprite) Orc_2.Add(sprite.name, sprite);
    foreach (var obj in orc3) if (obj is Sprite sprite) Orc_3.Add(sprite.name, sprite);
    foreach (var obj in orc4) if (obj is Sprite sprite) Orc_4.Add(sprite.name, sprite);
    
  }

}
