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
            IDamagable damagable = collision.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(
                controller.model.modelSO.ExtAtkPoint * skill2Data.ExtSkillDmg +
                controller.model.modelSO.InnAtkPoint * skill2Data.InnSkillDmg);
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }
}
