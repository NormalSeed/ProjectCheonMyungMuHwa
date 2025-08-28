using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public PlayerModelSO modelSO;

    // Observable Properties
    public ObservableProperty<double> CurHealth { get; private set; } = new();

    /// <summary>
    /// Status의 계산값을 적용시키는 메서드
    /// </summary>
    public void SetPoints()
    {
        modelSO.HealthPoint = (modelSO.Vital + (modelSO.Level * modelSO.Vital_Increase)) * (modelSO.HealthRatio + (modelSO.HealthRatio_Increase * modelSO.Level));
        modelSO.ExtAtkPoint = (modelSO.ExtPow + (modelSO.Level * modelSO.ExtPow_Increase)) * (modelSO.AttackRatio + (modelSO.AttackRatio_Increase * modelSO.Level));
        modelSO.InnAtkPoint = (modelSO.InnPow + (modelSO.Level * modelSO.InnPow_Increase)) * (modelSO.AttackRatio + (modelSO.AttackRatio_Increase * modelSO.Level));
        modelSO.DefPoint = (modelSO.ExtPow + modelSO.InnPow) * (modelSO.DefRatio + (modelSO.DefRatio_Increase * modelSO.Level));
    }
}
