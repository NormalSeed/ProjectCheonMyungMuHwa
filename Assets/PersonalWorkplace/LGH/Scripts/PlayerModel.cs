using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [field: Header("고정 값")]
    [field: SerializeField] public string   CharID { get; set; }
    [field: SerializeField] public string   CharName { get; set; }
    [field: SerializeField] public int      Rarity { get; set; }
    [field: SerializeField] public string   Faction { get; set; }
    [field: SerializeField] public int      Position { get; set; }
    [field: SerializeField] public string   Role { get; set; }

    [field: Header("성장 값")]
    [field: SerializeField] public int      Level { get; set; }
    [field: SerializeField] public int      Grade { get; set; }
    [field: SerializeField] public float    Vital { get; set; }         // 체력
    [field: SerializeField] public float    ExtPow { get; set; }        // 외공
    [field: SerializeField] public float    InnPow { get; set; }        // 내공
    [field: SerializeField] public float    CritRate { get; set;}       // 치확
    [field: SerializeField] public float    CritDamage { get; set; }    // 치뎀

    [field: Header("능력치 적용 계수")]
    [field: SerializeField] public float    HealthRatio { get; set; }
    [field: SerializeField] public float    AttackRatio { get; set; }
    [field: SerializeField] public float    DefRatio { get; set; }

    [field: Header("최종 능력치")]
    [field: SerializeField] public double   HealthPoint { get; set; }
    [field: SerializeField] public double   ExtAtkPoint { get; set; }
    [field: SerializeField] public double   InnAtkPoint { get; set; }
    [field: SerializeField] public double   DefPoint { get; set; }
    [field: SerializeField] public float    AtkSpeed { get; set; }
    [field: SerializeField] public float    MoveSpeed { get; set; }
    [field: SerializeField] public float    AtkRange { get; set; }
    [field: SerializeField] public string   SkillSetID { get; set; }

    // Observable Properties
    public ObservableProperty<double> CurHealth { get; private set; } = new();
}
