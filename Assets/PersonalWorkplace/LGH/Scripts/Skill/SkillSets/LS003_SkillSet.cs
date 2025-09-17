using System.Collections;
using UnityEngine;

public class LS003_SkillSet : SkillSet
{
    [SerializeField] private GameObject skill1Effect;

    private float skill1Speed = 10f;
    private WaitForSeconds skill1Interval = new WaitForSeconds(0.5f);

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1Routine(target));
    }

    private IEnumerator Skill1Routine(Transform target)
    {
        // 공격속도 30% 버프 적용
        StatModifier atkSpeedBuff = new StatModifier(
            StatType.AtkSpeed,       
            0.3f,                    
            ModifierSource.Buff,    
            "L003_AtkSpeedBuff",
            true,                   
            5f                      
        );

        StatModifierManager.ApplyModifierWithDuration(controller.model.modelSO.CharID, atkSpeedBuff, controller.model);
        StatModifierManager.ApplyToModel(controller.model);

        yield return skill1Interval;
        skill1Effect.transform.position = controller.transform.position - offset * 2;
        L003_Skill1 effect1 = skill1Effect.GetComponent<L003_Skill1>();
        skill1Effect.SetActive(true);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 2);

        // 발사 위치
        Vector3 spawnPos = controller.transform.position;

        // 타겟 방향 계산
        Vector3 direction = (target.transform.position - spawnPos).normalized;

        // 회전 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // 풀에서 Projectile 꺼내기
        Projectile projectile = pController.SpawnProjectile("Bloodknife1", spawnPos, target);
        Debug.Log($"Projectile 생성됨: {projectile.name}, 위치: {projectile.transform.position}");


        if (projectile != null)
        {
            projectile.transform.rotation = rotation;
            projectile.Configure(controller.transform.position, target, skill1Speed, skills[0].SkillRange, skills[0], controller);
            Debug.Log("Projectile Configure 호출됨");
        }
        else
        {
            Debug.LogWarning("Projectile이 null임");
        }

        StatModifier CritDmgBuff = new StatModifier(
            StatType.CritDamage,
            0.5f,
            ModifierSource.Buff,
            "L003_CritDamageBuff",
            true,
            5f
        );

        StatModifierManager.ApplyModifierWithDuration(controller.model.modelSO.CharID, CritDmgBuff, controller.model);
        StatModifierManager.ApplyToModel(controller.model);

        IDamagable damagable = target.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(
                controller.model.ExtAtk * skills[0].ExtSkillDmg +
                controller.model.InnAtk * skills[0].InnSkillDmg);
        }
        else
        {
            Debug.Log("IDamagable이 없음");
        }

        isSkillPlaying = false;
    }
}
