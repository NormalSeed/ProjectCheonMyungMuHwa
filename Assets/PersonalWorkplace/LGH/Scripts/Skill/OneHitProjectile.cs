using UnityEngine;
using UnityEngine.InputSystem.XR;

public class OneHitProjectile : Projectile
{
    private SkillSet skillSet;

    private float duration;

    protected override void OnEnable()
    {
        base.OnEnable();
        duration = 5f;
    }

    protected override void Update()
    {
        base.Update();
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            ReturnPool();
        }
    }

    protected override void FixedUpdate()
    {
        // 타겟이 null이면 가장 가까운 몬스터를 찾아서 재지정
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
            float minDistance = Mathf.Infinity;
            Transform closest = null;

            foreach (GameObject monster in monsters)
            {
                float dist = Vector2.Distance(transform.position, monster.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = monster.transform;
                }
            }

            if (closest != null)
            {
                target = closest;

                Debug.Log($"타겟 재지정됨: {target.name}");
            }
            else
            {
                // 타겟이 없으면 투사체 종료
                ReturnPool();
                return;
            }
        }

        // 이동 처리
        // 타겟 방향 계산
        Vector2 direction = (target.position - transform.position).normalized;

        // 그 방향으로 이동
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
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
            ReturnPool();
        }
    }

    private void OnDisable()
    {
        duration = 5f;
    }
}
