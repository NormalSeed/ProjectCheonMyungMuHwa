using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LoadedMonsterSO", menuName = "Scriptable Objects/LoadedMonsterSO")]
public class LoadedMonsterSO : ScriptableObject
{
  public Dictionary<string, GameObject> Objects = new();
}
