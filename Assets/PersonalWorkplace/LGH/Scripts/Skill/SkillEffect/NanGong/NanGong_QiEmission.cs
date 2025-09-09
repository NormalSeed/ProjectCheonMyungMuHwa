using UnityEngine;

public class NanGong_QiEmission : SkillEffect
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(
                    controller.model.ExtAtk * skill1Data.ExtSkillDmg +
                    controller.model.InnAtk * skill1Data.InnSkillDmg);
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
                        value: 0.3,
                        source: ModifierSource.Buff,
                        originID: "QiEmissionBuff",
                        isPercent: true,
                        duration: 5f
                    );

                    StatModifierManager.ApplyModifierWithDuration(charID, buff, this);
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
