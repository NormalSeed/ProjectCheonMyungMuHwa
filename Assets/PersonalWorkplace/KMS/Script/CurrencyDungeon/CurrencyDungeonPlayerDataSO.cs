using System.Collections.Generic;
using UnityEngine;



// 메인 씬에서 재화던전 측에 공유할 플레이어 데이터들
[CreateAssetMenu(fileName = "CurrencyDungeonPlayerDataSO", menuName = "Scriptable Objects/CurrencyDungeonPlayerDataSO")]
public class CurrencyDungeonPlayerDataSO : ScriptableObject
{
  public List<(string id, CardInfo info)> currentPlayerDataList = new();
}
