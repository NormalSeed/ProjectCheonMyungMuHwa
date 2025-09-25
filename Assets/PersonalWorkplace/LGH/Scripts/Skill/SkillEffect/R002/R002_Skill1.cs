using System.Threading;
using UnityEngine;

public class R002_Skill1 : SkillEffect
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();
            MonsterController mController = collision.GetComponent<MonsterController>();
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
                controller.damageDealt += damage;
                controller.synergyUI.UpdateDamageUI();
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }
}
