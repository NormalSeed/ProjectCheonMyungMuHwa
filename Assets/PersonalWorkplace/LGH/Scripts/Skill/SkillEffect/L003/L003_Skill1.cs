using UnityEngine;
using UnityEngine.AI;

public class L003_Skill1 : SkillEffect
{
    private NavMeshAgent agent;
    private SPUM_Prefabs spumController;
    private bool isSpumInitialized = false;

    [SerializeField] private GameObject hiddenWeapon;
    private float atkSpeed;
    private float atkInterval;

    private void OnEnable()
    {
        duration = 5f;
        agent = GetComponent<NavMeshAgent>();
        if (!isSpumInitialized)
        {
            spumController = GetComponent<SPUM_Prefabs>();
            spumController.OverrideControllerInit();
            isSpumInitialized = true;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        target = FindNearestEnemy();
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            target = FindNearestEnemy();
            if (target == null) 
            {
                gameObject.SetActive(false);
                return;
            } 
        }

        if (atkSpeed == 0 || atkSpeed != controller.model.AttackSpeed)
        {
            atkSpeed = controller.model.AttackSpeed;
            atkInterval = 1 / atkSpeed;
        }

        atkInterval -= Time.deltaTime;
        if (atkInterval <= 0 && target != null && target.gameObject.activeInHierarchy)
        {
            L003_Skill1_Attack();
        }

        if (Vector3.Distance(transform.position, controller.transform.position) > 1f)
        {
            agent.SetDestination(controller.transform.position);
        }
        else
        {
            agent.ResetPath();
        }

        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private Transform FindNearestEnemy()
    {
        float minDistance = Mathf.Infinity;
        Transform nearest = null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void L003_Skill1_Attack()
    {
        spumController.PlayAnimation(PlayerState.ATTACK, 2);
        L003_ShadowWeapon weapon = hiddenWeapon.GetComponent<L003_ShadowWeapon>();
        
        hiddenWeapon.transform.position = transform.position;
        hiddenWeapon.SetActive(true);
        weapon.SetTarget(target);

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(
                skill1Data.ExtSkillDmg * controller.model.ExtAtk +
                skill1Data.InnSkillDmg * controller.model.InnAtk);
        }

        atkInterval = 1 / atkSpeed;
    }
}
