using System.Collections;
using UnityEngine;

public class BS02_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill1Speed;
    public float skill2Speed;
    public float skill1Range;
    public float skill2Range;

    private void Awake()
    {
        SkillSetID = "BS02";
        skill1Speed = 12f;
        skill2Speed = 3.3f;
        skill1Range = skills[0].SkillRange;
        skill2Range = skills[1].SkillRange;
    }

    public override void Skill1(Transform target)
    {
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        if (target == null || pController == null) return;

        // 발사 위치
        Vector3 spawnPos = transform.position;

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("poisonarrow", spawnPos, target);
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

        IDamagable damagable = target.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(
            controller.model.ExtAtk * skills[0].ExtSkillDmg);
        }
        else
        {
            Debug.Log("IDamagable이 없음");
        }

        StartCoroutine(Skill1DamageLoop(target));
    }

    private IEnumerator Skill1DamageLoop(Transform target, float duration = 5f, float interval = 0.2f)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;

            IDamagable damagable = target.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(
                    controller.model.InnAtk * skills[0].InnSkillDmg);
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }


    public override void Skill2(Transform target)
    {
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        if (target == null || pController == null) return;

        Vector3 spawnPos = transform.position;
        Vector3 baseDir = (target.position - spawnPos).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        float[] angleOffsets = { 0f, 15f, -15f };

        foreach (float offset in angleOffsets)
        {
            float angle = baseAngle + offset;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            // 방향 벡터 계산
            Vector3 directionVec = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f).normalized;

            // 방향을 나타낼 임시 Transform 생성
            GameObject dirObj = new GameObject("TempDirection");
            dirObj.transform.position = spawnPos + directionVec; // 방향 기준 위치
            dirObj.transform.rotation = rotation;

            Projectile projectile = pController.SpawnProjectile("knife", spawnPos, target);
            if (projectile != null)
            {
                projectile.transform.rotation = rotation;

                // 방향 벡터를 기반으로 Configure
                projectile.Configure(spawnPos, dirObj.transform, skill2Speed, skill2Range, skills[1]);

                Debug.Log($"Projectile 생성됨: {projectile.name}, 각도: {angle}");
            }
            else
            {
                Debug.LogWarning("Projectile이 null임");
            }
        }
    }
}
