using System.Runtime.CompilerServices;
using UnityEngine;

public class Projectile : PooledObject
{
    private PlayerController controller;
    private PlayerSkillSO skillData;

    public float speed;
    public float range;

    private Transform target;
    private Vector2 startPosition;
    private bool isFired;

    private void Start()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        isFired = false;
    }

    private void Update()
    {
        if (!isFired || target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        if (Vector2.Distance(startPosition, transform.position) > range)
        {
            ReturnPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Monster 태그를 갖고 있는 오브젝트와 충돌했을 때 데미지를 주고 풀로 돌아감
        if (collision.CompareTag("Monster"))
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();
            
            if (damagable != null)
            {
                damagable.TakeDamage(
                controller.model.modelSO.ExtAtkPoint * skillData.ExtSkillDmg +
                controller.model.modelSO.InnAtkPoint * skillData.InnSkillDmg);
                ReturnPool();
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }

    public void Configure(Transform targetTransform, float skillSpeed, float skillRange, PlayerSkillSO skillSO)
    {
        target = targetTransform;
        startPosition = transform.position;
        speed = skillSpeed;
        range = skillRange;
        skillData = skillSO;

        isFired = true;
    }
}
