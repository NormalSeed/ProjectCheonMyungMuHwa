using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public void Start()
    {
        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;
    }
}
