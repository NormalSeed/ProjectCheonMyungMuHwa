using System.Collections;
using UnityEngine;

public class NanGong_QiEmission : SkillEffect
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();
            MonsterController mController = collision.GetComponent<MonsterController>();
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
        else if (collision.CompareTag("Player"))
        {
            PlayerModel model = collision.GetComponent<PlayerModel>();

            if (model != null)
            {
                string charID = model.modelSO.CharID;

                // 중복 방지: 이미 버프가 있다면 적용하지 않음
                if (!StatModifierManager.HasModifier(charID, "QiEmissionBuff"))
                {
                    var buff = new StatModifier(
                        statType: StatType.InnAtk,
                        value: 0.3f,
                        source: ModifierSource.Buff,
                        originID: "QiEmissionBuff",
                        isPercent: true,
                        duration: 5f
                    );

                    StatModifierManager.ApplyModifierWithDuration(charID, buff, model);
                    StatModifierManager.ApplyToModel(model);

                    Debug.Log($"<color=green>{model.name}에게 5초간 InnAtk +30% QiEmission 버프 적용됨</color>");
                }
                else
                {
                    Debug.Log($"<color=yellow>{model.name}은 이미 QiEmission 버프를 받고 있음</color>");
                }
            }
        }
    }
}
