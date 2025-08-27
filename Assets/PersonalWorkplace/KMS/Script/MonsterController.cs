using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;
public struct AttackResult
{
    public double damage;
    public bool isDead;
}
public interface IDamagable2
{
    //amount- 내 공격력
    public AttackResult TakeDamage(double amount);
}

public abstract class MonsterController : MonoBehaviour, IDamagable, IPooled<MonsterController>
{

    public SPUM_Prefabs Spum;
    public MonsterModel Model;
    protected BehaviorGraphAgent treeAgent;
    public NavMeshAgent NavAgent;
    public Action<IPooled<MonsterController>> OnLifeEnded { get; set; }
    protected WaitForSeconds hurtWfs;
    protected Coroutine hurtCo;

    public bool IsDead => Model.CurHealth.Value <= 0;
    void Awake()
    {
        InitComponent();
    }
    protected virtual void InitComponent()
    {
        hurtWfs = new WaitForSeconds(0.15f);
        Spum = GetComponent<SPUM_Prefabs>();
        NavAgent = GetComponent<NavMeshAgent>();
        treeAgent = GetComponent<BehaviorGraphAgent>();
        Model = GetComponent<MonsterModel>();
        NavAgent.updateRotation = false;
        NavAgent.updateUpAxis = false;
        Spum.OverrideControllerInit();
        Model.CurHealth = new ObservableProperty<double>(10f);

    }
    void OnEnable()
    {
        if (Model.BaseModel != null)
        {
            SetValue();
        }
    }
    protected virtual void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
        treeAgent.SetVariableValue<float>("MoveSpeed", Model.BaseModel.MoveSpeed);
        treeAgent.SetVariableValue<float>("AttackDistance", Model.BaseModel.AttackDistance);
        treeAgent.SetVariableValue<float>("AttackDistanceWithClearance", Model.BaseModel.AttackDistanceWithClearance);
        treeAgent.SetVariableValue<float>("AttackDelay", Model.BaseModel.AttackDelay);
        treeAgent.SetVariableValue<float>("CurrentDistance", float.MaxValue);
        treeAgent.SetVariableValue<MonsterController>("Controller", this);
        treeAgent.Restart();

    }

    protected virtual IEnumerator HurtEffectRoutine()
    {
        Model.SetSpriteColor(Color.black);
        yield return hurtWfs;
        Model.SetSpriteColor(Color.white);
    }
    protected virtual IEnumerator DeathRoutine()
    {
        Spum.PlayAnimation(PlayerState.DEATH, 0);
        for (int i = 0; i < 5; i++)
        {
            DroppedItem item = PoolManager.Instance.ItemPool.GetItem(transform.position);
            item.Shot();
        }
        yield return new WaitForSeconds(2);
        treeAgent.End();
        OnLifeEnded?.Invoke(this);
    }

    public void TakeDamage(double amount)
    {
        OnTakeDamage(amount);
    }
    protected virtual void OnTakeDamage(double amount)
    {
        if (IsDead) return;
        Model.CurHealth.Value -= amount;
        if (hurtCo != null)
        {
            StopCoroutine(hurtCo);
            hurtCo = null;
        }
        hurtCo = StartCoroutine(HurtEffectRoutine());

    }

    public virtual void OnIdle()
    {
        Spum.PlayAnimation(PlayerState.IDLE, 0);
    }
    public virtual void OnMove()
    {
        Spum.PlayAnimation(PlayerState.MOVE, 0);
    }

    public virtual void OnDeath()
    {
        StartCoroutine(DeathRoutine());
        AudioManager.Instance.PlaySound("Monster_Dead");
    }
    public abstract void OnAttack(GameObject me, IDamagable target);
}
