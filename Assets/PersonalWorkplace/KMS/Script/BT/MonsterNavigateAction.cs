using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterNavigate", story: "[Target] [Controller] [Movespeed]", category: "Action", id: "cbdf5b83b6e56a50dfcb67530bd4694c")]
public partial class MonsterNavigateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<MonsterController> Controller;
    [SerializeReference] public BlackboardVariable<float> Movespeed;

    protected override Status OnStart()
    {
        Controller.Value.NavAgent.speed = Movespeed.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null || !Target.Value.activeSelf) return Status.Running;
        Controller.Value.NavAgent.SetDestination(Target.Value.transform.position);
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Controller.Value.NavAgent.ResetPath();
    }
}

