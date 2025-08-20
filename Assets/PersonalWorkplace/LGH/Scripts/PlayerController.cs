using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public bool isSkillReady = true;

    private void Start()
    {
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
}
