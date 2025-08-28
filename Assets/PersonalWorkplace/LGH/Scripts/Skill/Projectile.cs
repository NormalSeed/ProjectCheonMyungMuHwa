using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class Projectile : PooledObject
{
    protected PlayerController controller;
    protected PlayerSkillSO skillData;

    public float speed;
    public float range;

    protected Transform target;
    protected Vector2 startPosition;
    protected Vector2 fireDirection;
    protected bool isFired;

    protected void Start()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    protected void OnEnable()
    {
        isFired = false;
    }

    protected virtual void Update()
    {
        if (!isFired || target == null) return;

        if (Vector2.Distance(startPosition, transform.position) > range)
        {
            fireDirection = Vector2.zero;
            ReturnPool();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!isFired || target == null) return;

        transform.Translate(fireDirection * speed * Time.deltaTime, Space.World);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
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
                //ReturnPool();
            }
            else
            {
                Debug.Log("IDamagable이 없음");
            }
        }
    }

    public void Configure(Vector2 fireOrigin, Transform targetTransform, float skillSpeed, float skillRange, PlayerSkillSO skillSO)
    {
        target = targetTransform;
        startPosition = fireOrigin;
        transform.position = startPosition;
        fireDirection = ((Vector2)targetTransform.position - fireOrigin).normalized;
        speed = skillSpeed;
        range = skillRange;
        skillData = skillSO;

        isFired = true;
        Debug.Log($"FireOrigin: {fireOrigin}, Target: {targetTransform.position}, Direction: {fireDirection}");
    }
}
