using UnityEngine;

public class MyungHwa_Void : SkillEffect
{
    public Collider2D coll;

    public void ActivateCollider()
    {
        coll.enabled = true;
    }

    public void UnactivateCollider()
    {
        coll.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Monster 태그를 갖고 있는 오브젝트와 충돌했을 때 데미지를 줌
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(
                controller.model.modelSO.ExtAtkPoint * skill1Data.ExtSkillDmg +
                controller.model.modelSO.InnAtkPoint * skill1Data.InnSkillDmg);
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }
}
