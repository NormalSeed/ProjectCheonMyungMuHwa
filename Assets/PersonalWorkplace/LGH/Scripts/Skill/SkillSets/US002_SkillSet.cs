using System.Collections;
using UnityEngine;
using UnityEngineInternal;

public class US002_SkillSet : SkillSet
{
    [SerializeField] private GameObject skill1Effect;
    [SerializeField] private GameObject skill2Effect;
    public float skill1Duration;

    private void Awake()
    {
        SkillSetID = "US002";
        skill1Duration = 2f;
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1Routine());
    }

    private IEnumerator Skill1Routine()
    {
        Transform origin = controller.transform;
        skill1Effect.transform.position = origin.position + offset;
        skill1Effect.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        skill1Effect.SetActive(false);
        Vector2 center = transform.position;
        float radius = skills[0].SkillRange;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                IDamagable damagable = hit.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    PlayerController targetController = hit.GetComponent<PlayerController>();
                    InstantAttatchment attatchment = aController.SpawnAttatchment("Healing1", hit.transform);
                    attatchment.Configure(skill1Duration, skills[0]);
                    float healAmount = controller.model.ExtAtk * skills[0].ExtSkillDmg + controller.model.InnAtk * skills[1].InnSkillDmg;
                    targetController.Heal(healAmount); // 첫 번째 회복
                    StartCoroutine(DelayedHeal(targetController, healAmount)); // 2번째 회복
                }
            }
        }
        isSkillPlaying = false;
    }

    private IEnumerator DelayedHeal(PlayerController target, float amount)
    {
        yield return new WaitForSeconds(2f);
        target.Heal(amount); // 두 번째 회복
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        yield return new WaitForSeconds(0.7f);
        spumC.PlayAnimation(PlayerState.ATTACK, 2);
        Vector3 offset = new Vector3(0.2f, 0, 0);
        Vector3 destination;
        if (target.position.x - transform.position.x >= 0)
        {
            destination = target.position - offset;
        }
        else
        {
            destination = target.position + offset;
        }

        Transform origin = controller.transform;
        controller.transform.position = destination;

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(
                controller.model.ExtAtk * skills[0].ExtSkillDmg +
                controller.model.InnAtk * skills[1].InnSkillDmg);
        }

        if (Random.value < 0.2f)
        {
            U002_Skill2 se2 = skill2Effect.GetComponent<U002_Skill2>();
            se2.SetParent(origin.transform);
            skill2Effect.transform.SetParent(target);
            skill2Effect.SetActive(true);
        }

        yield return new WaitForSeconds(0.7f);
        controller.transform.position = origin.position;
        isSkillPlaying = false;
    }
}
