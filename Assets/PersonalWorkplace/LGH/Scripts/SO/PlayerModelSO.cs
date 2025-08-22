using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModelSO", menuName = "Game/PlayerModelSO")]
public class PlayerModelSO : ScriptableObject
{
    [Header("고정 값")]
    public string CharID;
    public string CharName;
    public int Rarity;
    public string Faction;
    public int Position;
    public string Role;

    [Header("성장 값")]
    public int Level;
    public int Grade;
    public float Vital;
    public float ExtPow;
    public float InnPow;
    public float CritRate;
    public float CritDamage;

    [Header("능력치 적용 계수")]
    public float HealthRatio;
    public float AttackRatio;
    public float DefRatio;

    [Header("최종 능력치")]
    public double HealthPoint;
    public double ExtAtkPoint;
    public double InnAtkPoint;
    public double DefPoint;
    // 위 4개 능력치는 계산식 적용해서 초기화 필요
    public float AtkSpeed;
    public float MoveSpeed;
    public float AtkRange;
    public string SkillSetID;
}
