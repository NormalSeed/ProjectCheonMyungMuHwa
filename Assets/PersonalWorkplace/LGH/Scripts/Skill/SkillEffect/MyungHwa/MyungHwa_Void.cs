using System.Collections;
using UnityEngine;

public class MyungHwa_Void : SkillEffect
{
    public Collider2D coll;
    private Transform originalParent;

    protected override void Awake()
    {
        base.Awake();
        coll = GetComponent<Collider2D>();
        duration = 1f;
    }

    private void OnEnable()
    {
        StartCoroutine(AutoDisable());
    }

    public void SetParent(Transform parent)
    {
        originalParent = parent;
    }

    public void ActivateCollider()
    {
        coll.enabled = true;
    }

    public void UnactivateCollider()
    {
        coll.enabled = false;
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(duration);
        transform.SetParent(originalParent);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Monster 태그를 갖고 있는 오브젝트와 충돌했을 때 데미지를 줌
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
    }
}
