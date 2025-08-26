using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AS01_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill1Speed;
    public float skill2Speed;
    public float skill1Range;
    public float skill2Range;

    private void Awake()
    {
        SkillSetID = "AS01";
        skill1Speed = 3.3f;
        skill2Speed = 3.3f;
        skill1Range = skills[0].SkillRange;
        skill2Range = skills[1].SkillRange;
    }

    public override void Skill1(Transform target)
    {
        if (target == null || pController == null) return;

        // 발사 위치
        Vector3 spawnPos = transform.position;

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("Slashwave", spawnPos, target);
        Debug.Log($"Projectile 생성됨: {projectile.name}, 위치: {projectile.transform.position}");


        if (projectile != null)
        {
            projectile.transform.rotation = rotation;
            projectile.Configure(controller.transform.position, target, skill1Speed, skill1Range, skills[0]);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }
    }

    public override void Skill2(Transform target)
    {
        if (target == null || pController == null) return;

        // 발사 위치
        Vector3 spawnPos = transform.position;

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("powerwave", spawnPos, target);
        Debug.Log($"Projectile 생성됨: {projectile.name}, 위치: {projectile.transform.position}");


        if (projectile != null)
        {
            projectile.transform.rotation = rotation;
            projectile.Configure(controller.transform.position, target, skill1Speed, skill1Range, skills[1]);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }
    }
}
