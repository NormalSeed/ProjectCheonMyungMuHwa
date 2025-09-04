using System;
using System.Collections;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public abstract class MonsterController : MonoBehaviour, IDamagable, IPooled<MonsterController>
{

    public Action<IPooled<MonsterController>> OnLifeEnded { get; set; }
    public SPUM_Prefabs Spum;
    public MonsterModel Model;
    protected BehaviorGraphAgent treeAgent;
    public NavMeshAgent NavAgent;
    protected WaitForSeconds hurtWfs;
    protected Coroutine hurtCo;

    protected Coroutine attackCo;

    [SerializeField] protected Image healthBar;

    [SerializeField] protected float attackDelay;

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
            InGameManager.Instance.monsterDeathStack.Value++;
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
        RealAttackDelay = new WaitForSeconds(attackDelay);
    }
    protected virtual void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
        healthBar.fillAmount = 1;
        treeAgent.SetVariableValue<float>("MoveSpeed", Model.MoveSpeed);
        treeAgent.SetVariableValue<float>("AttackDistance", Model.AttackDistance);
        treeAgent.SetVariableValue<float>("AttackDistanceWithClearance", Model.AttackDistanceWithClearance);
        treeAgent.SetVariableValue<float>("AttackDelay", Model.AttackDelay);
        treeAgent.SetVariableValue<float>("CurrentDistance", float.MaxValue);
        treeAgent.SetVariableValue<MonsterController>("Controller", this);
        treeAgent.Restart();
    }



    public void TakeDamage(double amount)
    {
        OnTakeDamage(amount);
        SetHealthBar();
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
        InGameManager.Instance.monsterDeathStack.Value--;
        if (attackCo != null) StopCoroutine(attackCo);
        StartCoroutine(DeathRoutine());
        AudioManager.Instance.PlaySound("Monster_Dead");
        //QuestManager.Instance.UpdateQuest("Monster", 1);
    }
    public abstract void OnAttack(GameObject me, IDamagable target);
    protected abstract IEnumerator RealAttackRoutine(IDamagable target);



    protected virtual IEnumerator HurtEffectRoutine()
    {
        Model.SetSpriteColor(Color.black);
        yield return hurtWfs;
        Model.SetSpriteColor(Color.white);
    }
    protected virtual IEnumerator DeathRoutine()
    {
        Spum.PlayAnimation(PlayerState.DEATH, 0);
        DropItem();
        yield return new WaitForSeconds(2);
        treeAgent.End();
        OnLifeEnded?.Invoke(this);
    }

    protected virtual void DropItem()
    {
        DroppedItem i1 = PoolManager.Instance.ItemPool.GetItem(transform.position);
        i1.Init(DroppedItemType.Gold, Model.BaseModel.GoldQuant);
        DroppedItem i2 = PoolManager.Instance.ItemPool.GetItem(transform.position);
        i2.Init(DroppedItemType.SpiritBack, Model.BaseModel.SpiritBackQuant);
        DroppedItem i3 = PoolManager.Instance.ItemPool.GetItem(transform.position);
        i3.Init(DroppedItemType.SoulStone, Model.BaseModel.SoulStoneQuant);
        i1.Shot(); i2.Shot(); i3.Shot();
        
        DroppedItem i4 = PoolManager.Instance.ItemPool.GetItem(transform.position);
        i4.Init(DroppedItemType.NormalChest, 1);
         i4.Shot();


    }

    private void SetHealthBar()
    {
        double val = Model.CurHealth.Value / Model.BaseModel.finalMaxHealth;
        float v = (float)val;
        float res = Mathf.Max(0, v);
        healthBar.fillAmount = res;
    }
}
