using UnityEngine;

public class L002_Skill2 : SkillEffect
{
    public float moveSpeed = 1.5f;
    public float damageInterval = 0.5f;
    private float damageTimer = 0f;

    private void OnEnable()
    {
        duration = 5f;
    }

    private void Update()
    {
        // 타겟 방향으로 이동
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // 데미지 타이머 갱신
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamageToMonstersInRange();
        }

        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
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
                this.gameObject.SetActive(false);
                return;
            }
        }
    }

    private void ApplyDamageToMonstersInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                IDamagable damagable = hit.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    damagable.TakeDamage(
                        skillSet.skills[1].ExtSkillDmg * controller.model.ExtAtk +
                        skillSet.skills[1].InnSkillDmg * controller.model.InnAtk);
                }
            }
        }
    }
}
