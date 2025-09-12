using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RS002_SkillSet : SkillSet
{
    public GameObject skill1Effect;

    public float skill2Speed;
    public float skill2Range;

    private void Awake()
    {
        SkillSetID = "RS002";
        skill2Speed = 5f;
        skill2Range = skills[1].SkillRange;
    }

    public override void Skill1(Transform target)
    {
        // Skill1은 일반 SkillEffect
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        // 발사 위치
        Vector3 spawnPos = transform.position;

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 위치 오프셋
        Vector3 offset = direction * 0.5f;
        Vector3 effectPos = spawnPos + offset;

        // 이펙트 배치 및 활성화
        skill1Effect.transform.position = effectPos;
        skill1Effect.transform.rotation = rotation;
        skill1Effect.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        skill1Effect.SetActive(false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        // Skill2는 Projectile, 추후에 Target 타입 전용 Projectile 추가해서 교체해야 함
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);

        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        Vector3 origin = transform.position;
        float range = skills[1].SkillRange;

        // 범위 내 몬스터 탐색
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        List<Transform> validTargets = new();

        foreach (var monsterObj in monsters)
        {
            var damagable = monsterObj.GetComponent<IDamagable>();
            if (damagable != null)
            {
                float dist = Vector2.Distance(transform.position, monsterObj.transform.position);
                if (dist <= skills[1].SkillRange)
                {
                    validTargets.Add(monsterObj.transform);
                }
            }
        }

        // 거리순 정렬 후 가장 가까운 5개 선택
        var sortedTargets = validTargets
            .OrderBy(t => Vector2.Distance(origin, t.position))
            .Take(5)
            .ToList();

        yield return new WaitForSeconds(0.7f);

        foreach (var t in sortedTargets)
        {
            FirePooledProjectile(t);
        }
        isSkillPlaying = false;
    }

    private void FirePooledProjectile(Transform target)
    {
        if (target == null || pController == null) return;

        Vector3 spawnPos = transform.position;
        Vector3 direction = (target.position - spawnPos).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("R002_arrow", spawnPos, target);
        Debug.Log($"Projectile 생성됨: {projectile?.name}, 위치: {projectile?.transform.position}");

        if (projectile != null)
        {
            projectile.transform.rotation = rotation;
            projectile.Configure(spawnPos, target, skill2Speed, skill2Range, skills[1]);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }
    }

}
