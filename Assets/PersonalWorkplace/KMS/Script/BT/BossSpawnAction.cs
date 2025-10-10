using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BossSpawnAction", story: "Play spawn Animation / [Controller] [IsSpawnAnimationEnd]", category: "Action", id: "d64895419ed9393d2f72ad2cf907f713")]
public partial class BossSpawnAction : Action
{
    [SerializeReference] public BlackboardVariable<BossController> Controller;
    [SerializeReference] public BlackboardVariable<bool> IsSpawnAnimationEnd;
    protected override Status OnStart()
    {
        Controller.Value.OnSpawnAmimEnd += () => IsSpawnAnimationEnd.Value = true;
        Controller.Value.OnSpawn();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Running;
    }
}

