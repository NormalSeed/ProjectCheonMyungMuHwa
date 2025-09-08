using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyBossModelBaseSO", menuName = "Scriptable Objects/CurrencyBossModelBaseSO")]
public class CurrencyBossModelBaseSO : MonsterModelBaseSO
{

  public void SetCurrencyBossFinalState(int level)
  {
    SetFinal(level * 5);
    SetFinalBoss(level * 5, this);
  }
}
