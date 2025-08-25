using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterAttackAction", story: "[Self] attack [Target] by [AttackDelay] / set [IsTargetDetected] and [CurrentDistance] & [Controller]", category: "Action", id: "e6a0e8fea4cd29a0747bfe8d27303af6")]
public partial class MonsterAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> AttackDelay;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<MonsterController> Controller;

    private IDamagable damageable;
    private float timer;

    protected override Status OnStart()
    {
        damageable = Target.Value.GetComponent<IDamagable>();
        Controller.Value.OnIdle();
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
        CurrentDistance.Value = Vector3.Magnitude(Self.Value.transform.position - Target.Value.transform.position);
        if (timer <= 0f)
        {
            Attack();
            timer = AttackDelay.Value;
        }
        timer -= Time.deltaTime;
        return Status.Running;
    }

    private void Attack()
    {
        Controller.Value.OnAttack(Self.Value, damageable);
    }
}

