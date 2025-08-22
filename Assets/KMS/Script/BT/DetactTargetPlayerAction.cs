using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetactTargetPlayerAction", story: "[Self] detact nearby [Target] and set bool [IsTargetDetected]", category: "Action", id: "6ca15d30e1052d68a137bbfc3f902935")]
public partial class DetectTargetPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;

    private Vector3 selfPosition => Self.Value.transform.position;

    private float detectDelay = 0.5f;
    private float timer = 0f;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    /// <summary>
    /// Monster 태그를 가진 오브젝트 중 거리가 가장 가까운 오브젝트를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private void GetTarget(ref BlackboardVariable<GameObject> target)
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        if (monsters.Length == 0)
        {
            return;
        }
        float minDistance = Mathf.Infinity;

        foreach (GameObject monster in monsters)
        {
            float distance = Vector3.Distance(selfPosition, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                target.Value = monster;
            }
        }
        IsTargetDetected.Value = true;
        return;
    }

    protected override Status OnUpdate()
    {
        if (timer <= 0f)
        {
            GetTarget(ref Target);
            timer = detectDelay;
        }
        timer -= Time.deltaTime;
        return Status.Running;
    }
}

