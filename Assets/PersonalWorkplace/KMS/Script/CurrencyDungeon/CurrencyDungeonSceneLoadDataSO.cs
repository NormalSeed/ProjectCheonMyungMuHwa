using UnityEngine;


// 재화던전 씬 로드 시 재화던전 측에 필요한 데이터 저장 (씬 간 데이터 공유)

[CreateAssetMenu(fileName = "CurrencyDungeonSceneLoadDataSO", menuName = "Scriptable Objects/CurrencyDungeonSceneLoadDataSO")]
public class CurrencyDungeonSceneLoadDataSO : ScriptableObject
{
  public CurrencyDungeonData data;
  public CurrencyDungeonType type;

  //씬 로드 시 페이드인을 할지 여부
  public bool FadeIn = false;
    
}
