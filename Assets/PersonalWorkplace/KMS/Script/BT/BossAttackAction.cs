using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BossAttackAction", story: "boss attack / [Controller] [AttackDelay]", category: "Action", id: "15c1a9675c0250cb4ddedd0e9dafee1a")]
public partial class BossAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<BossController> Controller;
    [SerializeReference] public BlackboardVariable<float> AttackDelay;
    private float timer;

    protected override Status OnStart()
    {
        Controller.Value.OnIdle();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (timer <= 0f)
        {
            Attack();
            timer = AttackDelay.Value;
        }
        timer -= Time.deltaTime;
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }

    private void Attack()
    {
        Controller.Value.OnAttack(null, null);
        
    }
}

