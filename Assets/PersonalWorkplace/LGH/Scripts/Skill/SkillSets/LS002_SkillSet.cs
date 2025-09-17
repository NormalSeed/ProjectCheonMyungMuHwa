using System.Collections;
using UnityEngine;

public class LS002_SkillSet : SkillSet
{
    [SerializeField] private GameObject skill1Effect;
    [SerializeField] private GameObject skill2Effect;

    public WaitForSeconds skill1Interval = new WaitForSeconds(0.5f);

    private void Awake()
    {
        SkillSetID = "LS002";
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 5);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        yield return skill1Interval;
        skill1Effect.transform.position = target.position;
        skill1Effect.SetActive(true);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 5);
        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        yield return skill1Interval;

        L002_Skill2 effect2 = skill2Effect.GetComponent<L002_Skill2>();

        skill2Effect.transform.position = controller.transform.position;
        skill2Effect.SetActive(true);
        isSkillPlaying = false;
    }
}
