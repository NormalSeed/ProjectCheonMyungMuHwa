using UnityEngine;

[CreateAssetMenu(fileName = "MonsterModelBaseSO", menuName = "Scriptable Objects/MonsterModelBaseSO")]
public class MonsterModelBaseSO : ScriptableObject
{
  [SerializeField] MonsterStateDataTableSO table;
  public MonsterStateData data;

  public double finalMaxHealth => data.HP;
  public double finalAttackPower => data.Attack;
  public double finalOuterDefense => data.ExpPowArmor;
  public double finalInnerDefense => data.InnPowArmor;

  public float GoldQuant => data.GoldRate;
  public float SpiritBackQuant => data.SpiritRate;
  public float SoulStoneQuant => data.SoulRate;

  public int Exp => data.Exp;
  public int CurrentDoor;



  public void SetState(int door)
  {
    CurrentDoor = door;
    data = table.NormalMonster[CurrentDoor - 1];
  }
  // 보스의 경우 이것을 이용
  public void SetBossState(int door)
  {
    CurrentDoor = door;
    data = table.BossMonster[CurrentDoor - 1];
    //if (CurrentDoor == door) return;
    //CurrentDoor = door;
    //finalOuterDefense = baseOuterDefense;
    //finalInnerDefense = baseInnerDefense;
    //int final = CurrentDoor % 100;
    //if (final == 25 || final == 50)
    //{
    //  finalMaxHealth = model.finalMaxHealth * 6;
    //}
    //else if (final == 0 || final == 75)
    //{
    //  finalMaxHealth = model.finalMaxHealth * 7.5;
    //}
    //else
    //{
    //  finalMaxHealth = model.finalMaxHealth * 5;
    //}
    //finalAttackPower = model.finalAttackPower * 3;
    //
    //if (door >= 26)
    //{
    //  finalOuterDefense -= 14700;
    //  finalInnerDefense -= 14850;
    //}
    //if (door >= 102)
    //{
    //  int count = Mathf.Min(door - 101, 99);
    //  finalOuterDefense += count * 300;
    //  finalInnerDefense += count * 150;
    //}
    //if (door >= 201)
    //{
    //  finalOuterDefense -= 14850;
    //  finalInnerDefense -= 14850;
    //}
    //if (door >= 202)
    //{
    //  int count = Mathf.Min(door - 201, 99);
    //  finalOuterDefense += count * 150;
    //  finalInnerDefense += count * 150;
    //}
    //if (door >= 301)
    //{
    //  int count = Mathf.Min(door - 300, 99);
    //  finalOuterDefense += count * 300;
    //  finalInnerDefense += count * 300;
    //}


  }
}
