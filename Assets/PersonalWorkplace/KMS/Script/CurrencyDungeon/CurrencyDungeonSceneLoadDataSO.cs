using UnityEngine;


// 재화던전 씬 로드 및 메인 씬으로 돌아올 때 서로 간에 필요한 데이터 저장

[CreateAssetMenu(fileName = "CurrencyDungeonSceneLoadDataSO", menuName = "Scriptable Objects/CurrencyDungeonSceneLoadDataSO")]
public class CurrencyDungeonSceneLoadDataSO : ScriptableObject
{
  public CurrencyDungeonData data; //던전 레벨, 리워드 개수 등
  public CurrencyDungeonType type; // 던전의 타입 (금화 혼백 영석)
  public CurrencyDungeonClearData clearData; // 현재 몇 레벨까지 클리어 되어있는지
  public UIType MainUiToOpen; //던전에서 돌아왔을때 열 UI
  void OnEnable()
  {
    MainUiToOpen = 0;
  }
}
