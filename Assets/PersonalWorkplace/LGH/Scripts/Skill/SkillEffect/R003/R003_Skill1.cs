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
                    if (damagable != null)
                    {
                        damagable.TakeDamage(
                            controller.model.ExtAtk * skill2Data.ExtSkillDmg +
                            controller.model.InnAtk * skill2Data.InnSkillDmg);
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
