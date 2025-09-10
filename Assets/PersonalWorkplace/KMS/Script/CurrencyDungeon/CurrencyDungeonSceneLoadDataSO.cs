using UnityEngine;


// 재화던전 씬 로드 및 메인 씬으로 돌아올 때 서로 간에 필요한 데이터 저장

[CreateAssetMenu(fileName = "CurrencyDungeonSceneLoadDataSO", menuName = "Scriptable Objects/CurrencyDungeonSceneLoadDataSO")]
public class CurrencyDungeonSceneLoadDataSO : ScriptableObject
{
  public CurrencyDungeonData data;
  public CurrencyDungeonType type;

  //재화 던전에서 메인으로 돌아왔는지에 대한 여부
  public bool BackToMain;
  void OnEnable()
  {
    BackToMain = false;
  }

}
