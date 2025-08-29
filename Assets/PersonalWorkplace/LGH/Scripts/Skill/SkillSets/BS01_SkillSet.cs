using System.Collections;
using UnityEngine;

public class BS01_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill2Speed;
    public float skill2Duration;
    public float skill1Range;
    public float skill2Range;

    public GameObject effect;

    private void Awake()
    {
        SkillSetID = "BS01";
        skill2Speed = 2.5f;
        skill2Duration = 5f;
        skill1Range = skills[0].SkillRange;
        skill2Range = skills[1].SkillRange;
    }

    public override void Skill1(Transform target)
    {
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        effect.transform.position = target.position;
        effect.SetActive(true);
        effect.transform.SetParent(null);

        yield return new WaitForSeconds(1f);

        // 이펙트 비활성화
        effect.SetActive(false);
        effect.transform.SetParent(this.transform);
    }

    public override void Skill2(Transform target)
    {
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        if (target == null || pController == null) return;

        // 반지름 5짜리 원 안의 랜덤 위치 계산
        Vector2 randomOffset = Random.insideUnitCircle * 5f;
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("Yinyang2", spawnPos, target);
        Debug.Log($"Projectile 생성됨: {projectile.name}, 위치: {projectile.transform.position}");


        if (projectile != null)
        {
            projectile.Configure(controller.transform.position, target, skill2Speed, skill2Range, skills[1]);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }
    }
}
