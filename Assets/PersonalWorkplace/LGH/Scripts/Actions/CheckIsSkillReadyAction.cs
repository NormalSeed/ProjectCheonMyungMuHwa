using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckIsSkillReady", story: "Check [Self] [isSkillReady]", category: "Action", id: "38c5be03933769337c83b9e5d12b54da")]
public partial class CheckIsSkillReadyAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> IsSkillReady;

    private PlayerController controller;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (controller.isSkillReady)
        {
            IsSkillReady.Value = true;
        }
        else
        {
            IsSkillReady.Value = false;
        }

        return Status.Success;
    }
}

