using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using UnityEngine.UI;

public enum MonsterAnimationState
{
    IDLE = -10,
    MOVE = -9,
    SPAWN = 1,
    ATTACK,
    HURT,
    DEATH
}

public abstract class BossController : MonsterController
{
    private bool isInvulnerable;
    public System.Action OnSpawnAmimEnd;

    [Inject]
    public void Construct(Image bossbar)
    {
        healthBar = bossbar;
    }
    protected override void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
        healthBar.fillAmount = 1;
        healthBar.transform.parent.gameObject.SetActive(true);
        treeAgent.SetVariableValue<float>("AttackDelay", Model.BaseModel.AttackDelay);
        treeAgent.SetVariableValue<BossController>("Controller", this);
        treeAgent.Restart();
    }
    public void OnSpawn()
    {
        isInvulnerable = true;
        StartCoroutine(SpawnRoutine());
    }
    public override void OnDeath()
    {
        //InGameManager.Instance.SetNextStage();
        InGameManager.Instance.monsterDeathStack.Value--;
        healthBar.transform.parent.gameObject.SetActive(false);
        AudioManager.Instance.PlaySound("Monster_Dead");
        StartCoroutine(DeathRoutine());
    }
    private IEnumerator SpawnRoutine()
    {
        ParticleSystem part = ParticleManager.Instance.GetParticle("CFXR2 Firewall A", transform.position);
        for (int i = 0; i < 10; i++)
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
        ParticleManager.Instance.ReleaseParticle(part, "CFXR2 Firewall A");
        OnSpawnAmimEnd?.Invoke();
        isInvulnerable = false;
    }
    protected override void OnTakeDamage(double amount)
    {
        if (isInvulnerable) return;
        base.OnTakeDamage(amount);
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem(go.transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }
    }

    //private void SetAnimation(MonsterAnimationState state)
    //{
    //    if (((int)state) < 0)
    //    {
    //        animator.SetBool(currentLoopState.ToString(), false);
    //        animator.SetBool(state.ToString(), true);
    //        currentLoopState = state;
    //    }
    //    else
    //    {
    //        StartCoroutine(SetAnimationRoutine(state.ToString()));
    //    }
    //}
    //
    //private IEnumerator SetAnimationRoutine(string state)
    //{
    //    animator.SetBool(state, true);
    //    yield return new WaitForSeconds(0.25f);
    //    animator.SetBool(state, false);
    //}

}
