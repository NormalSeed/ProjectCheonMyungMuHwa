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
        if (damagable != null)
        {
            damagable.TakeDamage(
                skillSet.skills[0].ExtSkillDmg * controller.model.ExtAtk +
                skillSet.skills[0].InnSkillDmg * controller.model.InnAtk);
            Debug.Log($"{monster.name}에게 저주 데미지 적용됨");
        }
    }
}
