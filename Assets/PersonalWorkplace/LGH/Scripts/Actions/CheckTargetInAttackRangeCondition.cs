using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckTargetInAttackRange", story: "Compare values of [CurrentDistance] and [Self] [AttackRange]", category: "Conditions", id: "affd606350b7efabd24a21f913a37c3c")]
public partial class CheckTargetInAttackRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> AttackRange;

    private PlayerModel model;

    public override void OnStart()
    {
        model = Self.Value.GetComponent<PlayerModel>();
        AttackRange.Value = model.AtkRange;
    }

    public override bool IsTrue()
    {
        if (CurrentDistance.Value <= AttackRange.Value)
        {
            return true;
        }

        return false;
    }
}
