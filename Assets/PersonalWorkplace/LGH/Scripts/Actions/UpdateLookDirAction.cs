using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateLookDir", story: "Update [Self] LookDirection by MoveDir", category: "Action", id: "b953eb4c6833c67cf97593a80ad7c7bd")]
public partial class UpdateLookDirAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private PlayerController controller;
    private GameObject SPUMAsset;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || controller == null || controller.SPUMAsset == null)
            return Status.Failure;

        var agent = Self.Value.GetComponent<NavMeshAgent>();
        var transform = controller.SPUMAsset?.transform;
        if (transform == null) return Status.Failure;

        if (agent != null)
        {
            float moveX = agent.velocity.x;

            if (Mathf.Abs(moveX) > 0.01f) // 움직이고 있을 때만 반전
            {
                Vector3 scale = transform.localScale;
                scale.x = moveX > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

