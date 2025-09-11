using System.Collections;
using UnityEngine;

public class NS002_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill1Range;
    public float skill2Range;
    public WaitForSeconds skill1Duration = new WaitForSeconds(2f);

    public GameObject skill1Effect;
    public GameObject skill2Effect;

    private void Awake()
    {
        SkillSetID = "NS002";
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        // 정신집중 애니메이션 재생
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        
        skill1Effect.transform.position = this.gameObject.transform.position + offset;
        skill1Effect.SetActive(true);

        yield return skill1Duration;
        skill1Effect.SetActive(false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;

        spumC.PlayAnimation(PlayerState.ATTACK, 1);

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(
                controller.model.ExtAtk * skills[1].ExtSkillDmg +
                controller.model.InnAtk * skills[1].InnSkillDmg);
        }

        GameObject effect2 = skill2Effect;
        effect2.transform.position = target.position;
        var timed = effect2.GetComponent<NanGong_DestroyFist>();
        timed.SetParent(this.transform);
        effect2.SetActive(true);
        effect2.transform.SetParent(null);
        isSkillPlaying = false;
    }
}
