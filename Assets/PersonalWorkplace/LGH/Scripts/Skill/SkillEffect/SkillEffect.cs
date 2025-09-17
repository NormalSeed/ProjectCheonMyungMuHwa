using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    protected SkillSet skillSet;
    protected PlayerController controller;
    public PlayerSkillSO skill1Data;
    public PlayerSkillSO skill2Data;

    protected float duration;

    protected Transform target;

    protected virtual void Awake()
    {
        skillSet = GetComponentInParent<SkillSet>();
        controller = skillSet.controller;
        skill1Data = controller.skillSet.GetComponent<SkillSet>().skills[0];
        skill2Data = controller.skillSet.GetComponent<SkillSet>().skills[1];
    }
}
