using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OfflineRewardDataTableSO", menuName = "Scriptable Objects/OfflineRewardDataTableSO")]
public class OfflineRewardDataTableSO : ScriptableObject
{
  public List<OfflineRewardData> Table = new();
}
[Serializable]
public struct OfflineRewardData
{
  public int Stage;
  public double Gold;
  public double Soul;
  public double Stone;
  public double Exp;
  public double EquipTicket;
  public double HeroTicket;

  public void Multiply(int val)
  {
    Gold *= val;
    Soul *= val;
    Stone *= val;
    Exp *= val;
    EquipTicket *= val;
    HeroTicket *= val;
  }
}