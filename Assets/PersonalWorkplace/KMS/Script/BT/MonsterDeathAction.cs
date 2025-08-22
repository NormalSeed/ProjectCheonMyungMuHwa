using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterDeathAction", story: "[Self] Death / reset [Target] [IsTargetDetected] [CurrentDistance]", category: "Action", id: "3e3efd3ed64218b8afb593d8516cc2c6")]
public partial class MonsterDeathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;

    [SerializeReference] public BlackboardVariable<float> CurrentDistance;

    private SPUM_Prefabs spum;
    private MonsterController monster;



    protected override Status OnStart()
    {
        if (monster == null) monster = Self.Value.GetComponent<MonsterController>();

        if (spum == null) spum = Self.Value.GetComponent<SPUM_Prefabs>();
        Target.Value = null;
        IsTargetDetected.Value = false;
        CurrentDistance.Value = float.MaxValue;
        monster.BlastCoins();
        spum.PlayAnimation(PlayerState.DEATH, 0);
        return Status.Success;
    }
}

