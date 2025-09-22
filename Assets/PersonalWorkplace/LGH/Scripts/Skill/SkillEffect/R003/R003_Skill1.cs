using System.Collections;
using UnityEngine;

public class R003_Skill1 : SkillEffect
{
    private Coroutine damageLoop;
    private WaitForSeconds interval = new WaitForSeconds(0.5f);

    protected override void Awake()
    {
        base.Awake();
        duration = 5f;
    }

    private void OnEnable()
    {
        damageLoop = StartCoroutine(DamageOverTime());
    }

    private void OnDisable()
    {
        if (damageLoop != null)
        {
            StopCoroutine(damageLoop);
            damageLoop = null;
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            // 범위 내 모든 몬스터 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skill1Data.SkillRange * 0.5f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Monster"))
                {
                    IDamagable damagable = hit.GetComponent<IDamagable>();
                    MonsterController mController = hit.GetComponent<MonsterController>();
                    if (damagable != null && mController != null)
                    {
                        float rawDamage = (float)(
                            skillSet.skills[0].ExtSkillDmg * controller.model.ExtAtk +
                            skillSet.skills[0].InnSkillDmg * controller.model.InnAtk -
                            mController.Model.BaseModel.finalOuterDefense -
                            mController.Model.BaseModel.finalInnerDefense);
                        float damage = Mathf.Clamp(rawDamage, 1f, float.MaxValue);

                        bool isCritical = UnityEngine.Random.value < controller.model.CritRate;
                        if (isCritical)
                        {
                            damage *= controller.model.CritDamage;
                        }

                        damagable.TakeDamage(damage);
                    }
                    else
                    {
                        Debug.Log("IDamagable이 없음");
                    }
                }
            }

            yield return interval;
        }
    }
}
