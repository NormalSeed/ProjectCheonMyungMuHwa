using System;
using System.Collections;
using NUnit.Framework.Internal;
using Unity.Behavior;
using UnityEditor.Animations;
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
    public override void OnAttack(GameObject me, IDamagable target)
    {
        SetAnimation(MonsterAnimationState.ATTACK);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return wfs;
        if (target != null)
        {
            target.TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem((target as PlayerController).transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }

    }
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
        wfs = new WaitForSeconds(0.7f);
    }

    protected override void SetValue()
    {
        base.SetValue();
    
  }
    public override void OnIdle()
    {
        SetAnimation(MonsterAnimationState.IDLE);

    }
    public override void OnMove()
    {
        SetAnimation(MonsterAnimationState.MOVE);

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


}
