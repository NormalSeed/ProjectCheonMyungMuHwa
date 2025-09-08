using UnityEngine;


// 재화던전 씬 로드 시 재화던전 측에 필요한 데이터 저장

[CreateAssetMenu(fileName = "CurrencyDungeonSceneLoadDataSO", menuName = "Scriptable Objects/CurrencyDungeonSceneLoadDataSO")]
public class CurrencyDungeonSceneLoadDataSO : ScriptableObject
{
  public CurrencyDungeonData data;
  public CurrencyDungeonType type;
    
}
