using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Linq;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateDistance", story: "Update [Self] and [Target] [CurrentDistance]", category: "Action", id: "e3c7858e8b92f9290bd7715f255f2cc2")]
public partial class UpdateDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;

    protected override Status OnUpdate()
    {
        Target.Value = GetTarget();
        if (Target.Value != null)
        {
            CurrentDistance.Value = Vector2.Distance(Self.Value.transform.position, Target.Value.transform.position);
        }
        else
        {
            CurrentDistance.Value = 999f;
        }

        return Status.Success;
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
}

