using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public PlayerModelSO modelSO;

    // 최종 능력치
    public double Health;
    public double ExtAtk;
    public double InnAtk;
    public double Def;
    public float CritRate;
    public float CritDamage;

    public float bossDamageBonus = 1;
    public float normalDamageBonus = 1;
    public float skillDamageBonus = 1;

    // 총 전투력
    public double combatPower;

    // Observable Properties
    public ObservableProperty<double> CurHealth { get; private set; } = new();

    /// <summary>
    /// Status의 계산값을 적용시키는 메서드
    /// </summary>
    public void SetPoints()
    {
        modelSO.HealthPoint = (modelSO.Vital * (modelSO.Level * modelSO.Vital_Increase)) * (modelSO.HealthRatio + (modelSO.HealthRatio_Increase * modelSO.Grade));
        modelSO.ExtAtkPoint = (modelSO.ExtPow * (modelSO.Level * modelSO.ExtPow_Increase)) * (modelSO.AttackRatio + (modelSO.AttackRatio_Increase * modelSO.Grade));
        modelSO.InnAtkPoint = (modelSO.InnPow * (modelSO.Level * modelSO.InnPow_Increase)) * (modelSO.AttackRatio + (modelSO.AttackRatio_Increase * modelSO.Grade));
        modelSO.DefPoint = (modelSO.ExtPow + modelSO.InnPow) * (modelSO.DefRatio + (modelSO.DefRatio_Increase * modelSO.Grade));
        // 전투력에 들어갈 공격력이 어떻게 계산되어야 할지 물어봐야 함

        // 최종 능력치 캐싱
        Health = modelSO.HealthPoint;
        ExtAtk = modelSO.ExtAtkPoint;
        InnAtk = modelSO.InnAtkPoint;
        Def = modelSO.DefPoint;
        CritRate = modelSO.CritRate;
        CritDamage = modelSO.CritDamage;

        // 초기 체력 설정
        CurHealth.Value = Health;
    }
}
