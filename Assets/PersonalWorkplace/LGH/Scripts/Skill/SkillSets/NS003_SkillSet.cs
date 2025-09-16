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
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1ShieldRoutine());
    }

    private IEnumerator Skill1ShieldRoutine()
    {
        skill1Effect.transform.position = this.gameObject.transform.position + offset;
        skill1Effect.SetActive(true);
        controller.ApplyShield(
            controller.model.ExtAtk * skills[0].ExtSkillDmg +
            controller.model.InnAtk * skills[0].InnSkillDmg,
            5f);
        yield return new WaitUntil(() => controller.isShieldActive == false);
        skill1Effect.SetActive(false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        skill2Effect.transform.position = target.position;
        skill2Effect.SetActive(true);

        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(
                controller.model.ExtAtk * skills[1].ExtSkillDmg +
                controller.model.InnAtk * skills[1].InnSkillDmg);
        }

        yield return new WaitForSeconds(0.4f);
        if (damagable != null)
        {
            damagable.TakeDamage(
                controller.model.ExtAtk * skills[1].ExtSkillDmg +
                controller.model.InnAtk * skills[1].InnSkillDmg);
        }

        skill2Effect.SetActive(false);
        isSkillPlaying = false;
    }
}
