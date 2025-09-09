using System.Collections;
using UnityEngine;

public class NS002_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill1Range;
    public float skill2Range;
    public WaitForSeconds skill1Duration = new WaitForSeconds(1f);

    public GameObject skill1Effect;
    public GameObject skill2Effect;

    private void Awake()
    {
        SkillSetID = "NS002";
    }

    public override void Skill1(Transform target)
    {
        // 정신집중 애니메이션 재생
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        skill1Effect.transform.position = this.gameObject.transform.position;
        skill1Effect.SetActive(true);

        yield return skill1Duration;
        skill1Effect.SetActive(false);
    }

    public override void Skill2(Transform target)
    {

    }
}
