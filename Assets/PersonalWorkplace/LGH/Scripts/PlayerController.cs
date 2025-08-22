using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour, IDamagable
{
    private PlayerModel model;
    private PlayerView view;

    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public bool isSkillReady = true;
    public float skillCooldown = 3f;

    private void Start()
    {
        model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();

        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;
    }

    private void Update()
    {
        // Behavior Graph용 스킬 사용 가능 온오프 테스트코드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isSkillReady = !isSkillReady;
        }
    }

    public void TakeDamage(double amount)
    {
        model.CurHealth.Value -= amount;
        Debug.Log($"현재 체력 : {model.CurHealth.Value}");
    }
}
