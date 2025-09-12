using UnityEngine;

public class R002_Skill1 : SkillEffect
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
    }
}
