using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterChaseAction", story: "check [Target] / set [IsTargetDetected] / set [CurrentDistance] based on [AttackDistance] and [Self]", category: "Action", id: "bcc7bc30e5e62c353ed1834ee12847c1")]
public partial class MonsterChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<float> AttackDistance;

    [SerializeReference] public BlackboardVariable<GameObject> Self;
    private SPUM_Prefabs spum;

    protected override Status OnStart()
    {
        if (spum == null) spum = Self.Value.GetComponent<SPUM_Prefabs>();
        spum.PlayAnimation(PlayerState.MOVE, 0);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null || !Target.Value.activeSelf)
        {
            Target.Value = null;
            IsTargetDetected.Value = false;
            CurrentDistance.Value = float.MaxValue;
            return Status.Running;
        }
        Vector3 dir = Target.Value.transform.position - Self.Value.transform.position;
        CurrentDistance.Value = Vector3.Magnitude(dir);
        float dot = Vector3.Dot(Vector3.right, dir);
        if (dot >= 0)
        {
            Self.Value.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            Self.Value.transform.localScale = new Vector3(1,1,1);
        }



        return Status.Running;
    }
}

