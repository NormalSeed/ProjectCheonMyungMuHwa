using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;

public abstract class MonsterController : MonoBehaviour, IDamagable, IPooled<MonsterController>
{

    public Action<IPooled<MonsterController>> OnLifeEnded { get; set; }
    public SPUM_Prefabs Spum;
    public MonsterModel Model;
    protected BehaviorGraphAgent treeAgent;
    public NavMeshAgent NavAgent;
    protected WaitForSeconds hurtWfs;
    protected Coroutine hurtCo;

    protected WaitForSeconds RealAttackDelay;

    public bool IsDead => Model.CurHealth.Value <= 0;
    void Awake()
    {
        InitComponent();
    }
    void OnEnable()
    {
        if (Model.BaseModel != null)
        {
            SetValue();
        }
    }
    protected virtual void InitComponent()
    {
        hurtWfs = new WaitForSeconds(0.15f);
        Spum = GetComponent<SPUM_Prefabs>();
        treeAgent = GetComponent<BehaviorGraphAgent>();
        Model = GetComponent<MonsterModel>();
        Spum.OverrideControllerInit();
        NavAgent = GetComponent<NavMeshAgent>();
        Model.CurHealth = new ObservableProperty<double>(10f);
        NavAgent.updateRotation = false;
        NavAgent.updateUpAxis = false;
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
        QuestManager.Instance.UpdateQuest("Monster", 1);
    }
    public abstract void OnAttack(GameObject me, IDamagable target);



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
}
