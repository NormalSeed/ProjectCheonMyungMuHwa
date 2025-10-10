using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckTargetDetected", story: "Check [Self] [isTargetDetected] [Target]", category: "Action", id: "fab027fcca41758e44623068c7813471")]
public partial class CheckTargetDetectedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private PlayerController controller;
    private SPUM_Prefabs spumC;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        spumC = controller.spumController;

        spumC.PlayAnimation(PlayerState.IDLE, 0);

        Target.Value = GetTarget();

        return Status.Running;
    }

    /// <summary>
    /// Monster 태그를 가진 오브젝트 중 거리가 가장 가까운 오브젝트를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private GameObject GetTarget()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 selfPosition = Self.Value.transform.position;

        foreach (GameObject monster in monsters)
        {
            float distance = Vector3.Distance(selfPosition, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = monster;
            }
        }

        return closest;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value != null)
        {
            IsTargetDetected.Value = true;
        }
        else
        {
            IsTargetDetected.Value = false;
        }

        return Status.Success;
    }
}

