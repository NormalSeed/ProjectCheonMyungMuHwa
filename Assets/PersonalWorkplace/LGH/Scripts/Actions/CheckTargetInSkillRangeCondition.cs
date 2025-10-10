using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckTargetInSkillRange", story: "Compare values of [CurrentDistance] and [Self] [SkillRanges]", category: "Conditions", id: "0b18ff640f2785615681cb1f292f6036")]
public partial class CheckTargetInSkillRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> SkillRanges;

    private PlayerController controller;
    private SkillSet skillSet;

    public override void OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        skillSet = controller.skillSet.GetComponent<SkillSet>();
    }

    public override bool IsTrue()
    {
        if (controller == null && Self.Value != null)
        {
            controller = Self.Value.GetComponent<PlayerController>();
        }

        if (skillSet == null && controller != null && controller.skillSet != null)
        {
            skillSet = controller.skillSet.GetComponent<SkillSet>();
        }

        if (skillSet == null || skillSet.skills.Count < 2)
            return false;


        if (controller.isSkill1Ready)
        {
            SkillRanges.Value = skillSet.skills[0].SkillRange;
        }
        else if (controller.isSkill2Ready)
        {
            SkillRanges.Value = skillSet.skills[1].SkillRange;
        }
        else
        {
            SkillRanges.Value = 0f; // 사거리가 없으면 무효 처리
        }

        return CurrentDistance.Value <= SkillRanges.Value;
    }
}
