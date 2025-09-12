using System.Collections;
using UnityEngine;

public class US003_SkillSet : SkillSet
{
    [SerializeField] private GameObject skill1Effect;
    [SerializeField] private GameObject skill2Effect;

    public float skill1Speed;
    private WaitForSeconds skill1interval = new WaitForSeconds(1f);
    private WaitForSeconds skill2interval = new WaitForSeconds(0.5f);

    private void Awake()
    {
        SkillSetID = "US003";

        skill1Speed = 2.5f;
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill1Routine());
    }

    private IEnumerator Skill1Routine()
    {
        // skills[0].SkillRange 내에 있는 Monster 태그를 갖고 있는 모든 오브젝트 중 거리가 가장 먼 것을 새로운 타겟(newTarget)으로 설정하고
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skills[0].SkillRange);
        Transform newTarget = null;
        float maxDistance = 0f;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    newTarget = hit.transform;
                }
            }
        }
        // 해당 타겟에 skill1Effect(Sprite를 담은 GameObject임)를 붙여서 타겟 표시
        skill1Effect.transform.position = newTarget.position;
        skill1Effect.SetActive(true);

        // 발사 위치
        Vector3 spawnPos = transform.position;

        // 타겟 방향 계산
        Vector3 direction = (newTarget.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        yield return skill1interval;

        Projectile projectile = pController.SpawnProjectile("BulletS1", spawnPos, newTarget);
        if (projectile != null)
        {
            projectile.transform.rotation = rotation;
            projectile.Configure(controller.transform.position + offset, newTarget, skill1Speed, 15f, skills[0]);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }

        skill1Effect.SetActive(false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        float radius = skills[1].SkillRange;
        float angleThreshold = 30f; // 60도 부채꼴이므로 ±30도

        Vector2 origin = transform.position;
        Vector2 targetDir = (target.position - transform.position).normalized;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                Vector2 toEnemy = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector2.Angle(targetDir, toEnemy);

                if (angleToTarget <= angleThreshold)
                {
                    IDamagable damagable = hit.GetComponent<IDamagable>();
                    if (damagable != null)
                    {
                        float damage = controller.model.ExtAtk * skills[1].ExtSkillDmg +
                                       controller.model.InnAtk * skills[1].InnSkillDmg;
                        damagable.TakeDamage(damage);
                    }
                }
            }
        }

        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        skill2Effect.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        skill2Effect.transform.position = transform.position + direction * 0.7f;

        skill2Effect.SetActive(true);
        yield return skill2interval;
        skill2Effect.SetActive(false);
        isSkillPlaying = false;
    }
}
