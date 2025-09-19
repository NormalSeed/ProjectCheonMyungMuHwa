using System.Collections.Generic;
using UnityEngine;

public class L002_Skill1 : SkillEffect
{
    private HashSet<MonsterController> monstersInRange = new HashSet<MonsterController>();

    private void OnEnable()
    {
        duration = 5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null)
            {
                monstersInRange.Add(monster);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null)
            {
                monstersInRange.Remove(monster);
            }
        }
    }

    private void Update()
    {
        foreach (var monster in monstersInRange)
        {
            if (monster != null && monster.isAttackedByNormalAttack)
            {
                ApplyCurseDamage(monster);
                monster.isAttackedByNormalAttack = false; // 한 번만 적용
            }
        }

        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            this.gameObject.SetActive(false);
        }

    }

    private void ApplyCurseDamage(MonsterController monster)
    {
        IDamagable damagable = monster.GetComponent<IDamagable>();
        MonsterController mController = monster.GetComponent<MonsterController>();
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

            DamageText text = DamageTextManager.Instance.Get(mController.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(damage).ToString());
        }
    }
}
