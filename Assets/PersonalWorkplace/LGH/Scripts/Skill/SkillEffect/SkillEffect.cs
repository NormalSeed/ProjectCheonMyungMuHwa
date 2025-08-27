using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    protected PlayerController controller;
    protected PlayerSkillSO skill1Data;
    protected PlayerSkillSO skill2Data;

    protected float duration;

    protected Transform target;

    protected virtual void Start()
    {
        controller = GetComponentInParent<PlayerController>();
        skill1Data = controller.skillSet.GetComponent<SkillSet>().skills[0];
        skill2Data = controller.skillSet.GetComponent<SkillSet>().skills[1];
    }
}
