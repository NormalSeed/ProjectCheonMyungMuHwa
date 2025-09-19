using UnityEngine;

public class JeokRang_BlazingSlash : SkillEffect
{
    protected override void Awake()
    {
        base.Awake();
        duration = 5f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Monster 태그를 갖고 있는 오브젝트와 충돌했을 때 데미지를 줌
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = target.GetComponent<IDamagable>();
            MonsterController mController = target.GetComponent<MonsterController>();
            float rawDamage = (float)(
                    skillSet.skills[1].ExtSkillDmg * controller.model.ExtAtk +
                    skillSet.skills[1].InnSkillDmg * controller.model.InnAtk -
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

                DamageText text = DamageTextManager.Instance.Get(mController.transform.position);
                text.SetText(BigCurrency.FromBaseAmount(damage).ToString());
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }
}
