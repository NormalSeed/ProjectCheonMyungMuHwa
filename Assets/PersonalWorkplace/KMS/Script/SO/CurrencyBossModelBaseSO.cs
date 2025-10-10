using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyBossModelBaseSO", menuName = "Scriptable Objects/CurrencyBossModelBaseSO")]
public class CurrencyBossModelBaseSO : MonsterModelBaseSO
{

  public void SetCurrencyBossFinalState(int level)
  {
    SetBossState(level * 5);
  }
}
