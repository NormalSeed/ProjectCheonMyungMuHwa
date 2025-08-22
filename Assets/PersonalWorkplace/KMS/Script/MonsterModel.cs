using UnityEngine;

public class MonsterModel : MonoBehaviour
{
    [SerializeField] double healthPoint;
    public double HealthPoint { get => healthPoint; }
    public ObservableProperty<double> CurHealth;

  void Awake()
  {
    
  }


  // 몬스터 소환 시에 BT에 한번 적용 후 죽기 전까진 변하지 않을 값들
  public float AttackDistance; //공격하기 위해 멈추는 사정거리
    public float AttackDistanceWithClearance; //공격 중에 벗어날 경우 다시 추적하기 위한 사정거리 (Attackdistance보다 약간 높게)
    public float AttackDelay; //공격 쿨타임
    public float MoveSpeed; //이동속도
}
