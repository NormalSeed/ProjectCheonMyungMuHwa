using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterModelBaseSO", menuName = "Scriptable Objects/MonsterModelBaseSO")]
public class MonsterModelBaseSO : ScriptableObject
{
  public double baseMaxHealth;

  public double baseDefense;

  public double baseAttackPower;

  public double finalMaxHealth;
  public double finalDefense;
  public double finalAttackPower;

  public int GoldQuant;
  public int SpiritBackQuant;
  public int SoulStoneQuant;

  public float NormalChestDropChance;
  public float RareChestDropChance;



  //초기 값부터 계산
  public void SetFinal(int stage)
  {
    finalMaxHealth = baseMaxHealth;
    finalDefense = baseDefense;
    finalAttackPower = baseAttackPower;
    for (int i = 2; i <= stage; i++)
    {
      int last = i % 100;
      if (last == 51 || last == 81 || last == 91)
      {
        finalMaxHealth *= 1.14;
      }
      else if (i > 200 && last == 1)
      {
        finalMaxHealth *= 1.18;
      }
      else
      {
        finalMaxHealth *= 1.07;
      }
      finalAttackPower *= 1.009;
    }
    if (stage >= 201)
    {
      finalDefense = 300 * stage;
    }

    GoldQuant = stage;
    SpiritBackQuant = stage;
    SoulStoneQuant = stage;
    NormalChestDropChance = 1;
    RareChestDropChance = 1;
  }
  //다음 스테이지에 맞게 업데이트
  public void UpdateFinal(int stage)
  {
    int last = stage % 100;
    if (last == 51 || last == 81 || last == 91)
    {
      finalMaxHealth *= 1.14;
    }
    else if (stage > 200 && last == 1)
    {
      finalMaxHealth *= 1.18;
    }
    else
    {
      finalMaxHealth *= 1.07;
    }
    finalAttackPower *= 1.009;

    if (stage >= 201)
    {
      finalDefense = 300 * stage;
    }

  }
  // 보스의 경우 이것을 이용
  public void SetFinalBoss(int stage, MonsterModelBaseSO model)
  {
    if (stage == 50 || stage == 100)
    {
      finalMaxHealth = model.finalMaxHealth * 7.5;
    }
    else
    {
      finalMaxHealth = model.finalMaxHealth * 2000;
    }
    finalAttackPower = model.finalAttackPower * 3;
    finalDefense = stage * baseDefense;


  }
}
