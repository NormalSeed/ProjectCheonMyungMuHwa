using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterDisable", story: "Disable [Self] / reset [Status]", category: "Action", id: "0574589b6c33262b0381dd87d65930fd")]
public partial class MonsterDisableAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<MonsterStatus> Status;

    protected override Status OnStart()
    {
        Self.Value.SetActive(false);
        Status.Value = MonsterStatus.Idle;
        return Node.Status.Success;
    }
}

