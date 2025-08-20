using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("고정 값")]
    public string   CharID { get; set; }
    public string   CharName { get; set; }
    public int      Rarity { get; set; }
    public string   Faction { get; set; }
    public int      Position { get; set; }
    public string   Role { get; set; }

    [Header("성장 값")]
    public int      Level { get; set; }
    public int      Grade { get; set; }
    public float    Vital { get; set; }         // 체력
    public float    ExtPow { get; set; }        // 외공
    public float    InnPow { get; set; }        // 내공
    public float    CritRate { get; set;}       // 치확
    public float    CritDamage { get; set; }    // 치뎀

    [Header("능력치 적용 계수")]
    public float    HealthRatio { get; set; }
    public float    AttackRatio { get; set; }
    public float    DefRatio { get; set; }

    [Header("최종 능력치")]
    public double   HealthPoint { get; set; }
    public double   ExtAtkPoint { get; set; }
    public double   InnAtkPoint { get; set; }
    public double   DefPoint { get; set; }
    public float    AtkSpeed { get; set; }
    public float    MoveSpeed { get; set; }
    public float    AtkRange { get; set; }
    public string   SkillSetID { get; set; }

    // Observable Properties
    public ObservableProperty<double> CurHealth { get; private set; } = new();
}
