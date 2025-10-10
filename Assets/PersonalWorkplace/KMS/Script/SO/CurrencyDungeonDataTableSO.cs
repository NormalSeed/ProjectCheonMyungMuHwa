using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyDungeonDataTableSO", menuName = "Scriptable Objects/CurrencyDungeonDataTableSO")]
public class CurrencyDungeonDataTableSO : ScriptableObject
{

  public List<CurrencyDungeonData> Table = new();

}

[System.Serializable]
public struct CurrencyDungeonData
{
  public int Level;
  public string Name;
  public double BattlePower;
  public double Reward;
}
