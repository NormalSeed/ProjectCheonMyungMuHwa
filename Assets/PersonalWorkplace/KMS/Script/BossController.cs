using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterAnimationState
{
    IDLE = -10,
    MOVE = -9,
    SPAWN = 1,
    ATTACK,
    HURT,
    DEATH
}

public class BossController : MonsterController
{

    private Animator animator;
    private WaitForSeconds wfs;

    private MonsterAnimationState currentLoopState;

    public System.Action OnSpawnAmimEnd;
    protected override void InitComponent()
    {
        currentLoopState = MonsterAnimationState.IDLE;
        hurtWfs = new WaitForSeconds(0.15f);
        NavAgent = GetComponent<NavMeshAgent>();
        treeAgent = GetComponent<BehaviorGraphAgent>();
        Model = GetComponent<MonsterModel>();
        animator = GetComponentInChildren<Animator>();
        NavAgent.updateRotation = false;
        NavAgent.updateUpAxis = false;
        Model.CurHealth = new ObservableProperty<double>(10f);
        wfs = new WaitForSeconds(1f);
    }
    protected override void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
        treeAgent.SetVariableValue<float>("AttackDelay", Model.BaseModel.AttackDelay);
        treeAgent.SetVariableValue<BossController>("Controller", this);
        treeAgent.Restart();

    }
    public override void OnIdle()
    {
        SetAnimation(MonsterAnimationState.IDLE);

    }

    public void OnSpawn()
    {
        StartCoroutine(SpawnRoutine());

    }

    public override void OnDeath()
    {
        InGameManager.Instance.SetNextStage();
        StartCoroutine(DeathRoutine());
    }
    protected override IEnumerator DeathRoutine()
    {
        SetAnimation(MonsterAnimationState.DEATH);
        for (int i = 0; i < 5; i++)
        {
            DroppedItem item = PoolManager.Instance.ItemPool.GetItem(transform.position);
            item.Shot();
        }
        yield return new WaitForSeconds(2);
        treeAgent.End();
        OnLifeEnded?.Invoke(this);
    }

    [SerializeField] GameObject effect;
    private IEnumerator SpawnRoutine()
    {
        GameObject go = Instantiate(effect, transform.position, Quaternion.identity);
        for (int i = 0; i < 55; i++)
        {
            if (i % 2 == 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(go);
        OnSpawnAmimEnd?.Invoke();
    }

    private void SetAnimation(MonsterAnimationState state)
    {
        if (((int)state) < 0)
        {
            animator.SetBool(currentLoopState.ToString(), false);
            animator.SetBool(state.ToString(), true);
            currentLoopState = state;
        }
        else
        {
            StartCoroutine(SetAnimationRoutine(state.ToString()));
        }
    }

    private IEnumerator SetAnimationRoutine(string state)
    {
        animator.SetBool(state, true);
        yield return new WaitForSeconds(0.25f);
        animator.SetBool(state, false);
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        SetAnimation(MonsterAnimationState.ATTACK);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return wfs;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem(go.transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }
    }

}
