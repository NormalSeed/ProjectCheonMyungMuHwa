using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterModelBaseSO", menuName = "Scriptable Objects/MonsterModelBaseSO")]
public class MonsterModelBaseSO : ScriptableObject
{
  public double baseMaxHealth;
  public double baseAttackPower;
  public double baseOuterDefense;
  public double baseInnerDefense;

  public double finalMaxHealth;
  public double finalAttackPower;
  public double finalOuterDefense;
  public double finalInnerDefense;

  public int GoldQuant;
  public int SpiritBackQuant;
  public int SoulStoneQuant;

  public float NormalChestDropChance;
  public float RareChestDropChance;



  //초기 값부터 계산
  public void SetFinal(int stage)
  {
    finalMaxHealth = baseMaxHealth;
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

    GoldQuant = stage;
    SpiritBackQuant = stage;
    SoulStoneQuant = stage;
    NormalChestDropChance = 1;
    RareChestDropChance = 1;
  }
  //다음 스테이지에 맞게 업데이트 (계산 부담 줄이는 용도)
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

  }
  // 보스의 경우 이것을 이용
  public void SetFinalBoss(int stage, MonsterModelBaseSO model)
  {
    finalOuterDefense = baseOuterDefense;
    finalInnerDefense = baseInnerDefense;
    if (stage == 50 || stage == 100)
    {
      finalMaxHealth = model.finalMaxHealth * 7.5;
    }
    else
    {
      finalMaxHealth = model.finalMaxHealth * 5;
    }
    finalAttackPower = model.finalAttackPower * 3;

    if (stage >= 101)
    {
      finalOuterDefense -= 14700;
      finalInnerDefense -= 14850;
    }
    if (stage >= 102)
    {
      int count = Mathf.Min(stage - 101, 99);
      finalOuterDefense += count * 300;
      finalInnerDefense += count * 150;
    }
    if (stage >= 201)
    {
      finalOuterDefense -= 14850;
      finalInnerDefense -= 14850;
    }
    if (stage >= 202)
    {
      int count = Mathf.Min(stage - 201, 99);
      finalOuterDefense += count * 150;
      finalInnerDefense += count * 150;
    }
    if (stage >= 301)
    {
      int count = Mathf.Min(stage - 300, 99);
      finalOuterDefense += count * 300;
      finalInnerDefense += count * 300;
    }


  }
}
