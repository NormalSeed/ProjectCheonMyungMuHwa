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
                    MonsterController mController = hit.GetComponent<MonsterController>();
                    float rawDamage = (float)(
                            skillSet.skills[0].ExtSkillDmg * controller.model.ExtAtk +
                            skillSet.skills[0].InnSkillDmg * controller.model.InnAtk -
                            mController.Model.BaseModel.finalOuterDefense -
                            mController.Model.BaseModel.finalInnerDefense);
                    float damage = Mathf.Clamp(rawDamage, 1f, float.MaxValue);
                    if (damagable != null && mController != null)
                    {
                        bool isCritical = UnityEngine.Random.value < controller.model.CritRate;
                        if (isCritical)
                        {
                            damage *= controller.model.CritDamage;
                        }

                        damagable.TakeDamage(damage);
                        controller.damageDealt += damage;
                        controller.synergyUI.UpdateDamageUI();
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
