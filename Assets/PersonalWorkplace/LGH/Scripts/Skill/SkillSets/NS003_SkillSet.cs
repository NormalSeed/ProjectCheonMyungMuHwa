using System.Collections;
using UnityEngine;

public class NS003_SkillSet : SkillSet
{
    public GameObject skill1Effect;
    public GameObject skill2Effect;

    private void Awake()
    {
        SkillSetID = "NS003";
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        StartCoroutine(Skill1ShieldRoutine());
    }

    private IEnumerator Skill1ShieldRoutine()
    {
        controller.ApplyShield(
            controller.model.ExtAtk * skills[0].ExtSkillDmg +
            controller.model.InnAtk * skills[0].InnSkillDmg,
            5f);
        yield return new WaitUntil(() => controller.isShieldActive == false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {

    }
}
