using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BossDeathAction", story: "boss dead / [Controller] [IsSpawnAnimationEnd] [Status]", category: "Action", id: "f988d2c5aea8d722263ec7379e2fbfed")]
public partial class BossDeathAction : Action
{
    [SerializeReference] public BlackboardVariable<BossController> Controller;
    [SerializeReference] public BlackboardVariable<bool> IsSpawnAnimationEnd;
    [SerializeReference] public BlackboardVariable<MonsterStatus> Status;

    protected override Status OnStart()
    {
        IsSpawnAnimationEnd.Value = false;
        Status.Value = MonsterStatus.Idle;
        Controller.Value.OnDeath();
        return Node.Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Node.Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

