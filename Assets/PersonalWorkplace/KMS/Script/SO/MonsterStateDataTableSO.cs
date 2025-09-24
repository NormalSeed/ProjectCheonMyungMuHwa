using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStateDataTableSO", menuName = "Scriptable Objects/MonsterStateDataTableSO")]
public class MonsterStateDataTableSO : ScriptableObject
{
  public List<MonsterStateData> NormalMonster = new();
  public List<MonsterStateData> BossMonster = new();
}

[Serializable]
public struct MonsterStateData
{
  public double HP;
  public float Attack;
  public float ExpPowArmor;
  public float InnPowArmor;
  public float GoldRate;
  public float SoulRate;
  public float SpiritRate;

}
