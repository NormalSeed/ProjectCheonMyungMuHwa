using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterDeathAction", story: "[Self] Death / reset [Target] [IsTargetDetected] [CurrentDistance] & [Controller]", category: "Action", id: "3e3efd3ed64218b8afb593d8516cc2c6")]
public partial class MonsterDeathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;

    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<MonsterController> Controller;



    protected override Status OnStart()
    {
        Target.Value = null;
        IsTargetDetected.Value = false;
        Controller.Value.OnDeath();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Running;

    }
}

