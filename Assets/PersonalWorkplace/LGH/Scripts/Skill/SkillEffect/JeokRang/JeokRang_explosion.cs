using System.Collections;
using UnityEngine;

public class JeokRang_explosion : SkillEffect
{
    private Coroutine damageLoop;
    private Transform originalParent;

    protected override void Awake()
    {
        base.Awake();
        duration = 5f;
    }

    public void SetParent(Transform parent)
    {
        originalParent = parent;

    }

    private void OnEnable()
    {
        damageLoop = StartCoroutine(DamageOverTime());
        StartCoroutine(AutoDisable());
    }

    private void OnDisable()
    {
        if (damageLoop != null)
        {
            StopCoroutine(damageLoop);
            damageLoop = null;
        }
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(duration);
        transform.SetParent(originalParent);
        gameObject.SetActive(false);
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            // 범위 내 모든 몬스터 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skill2Data.SkillRange * 1.25f);
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

            yield return new WaitForSeconds(0.5f);
        }
    }

}
