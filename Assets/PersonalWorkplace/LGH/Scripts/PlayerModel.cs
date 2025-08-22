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
        modelSO.HealthPoint = modelSO.Vital * modelSO.HealthRatio;
        modelSO.ExtAtkPoint = modelSO.ExtPow * modelSO.AttackRatio;
        modelSO.InnAtkPoint = modelSO.InnPow * modelSO.AttackRatio;
        modelSO.DefPoint = (modelSO.ExtPow + modelSO.InnPow) * modelSO.DefRatio;
    }
}
