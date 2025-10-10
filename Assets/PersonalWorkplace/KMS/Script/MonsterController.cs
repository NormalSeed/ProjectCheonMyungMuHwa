using System;
using System.Collections;
using Unity.Behavior;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using VContainer;

public abstract class MonsterController : MonoBehaviour, IDamagable, IPooled<MonsterController>
{
    public bool IsInvulnerable { get; set; }
    public Action<IPooled<MonsterController>> OnLifeEnded { get; set; }
    public SPUM_Prefabs Spum;
    public MonsterModel Model;
    protected BehaviorGraphAgent treeAgent;
    public NavMeshAgent NavAgent;
    protected WaitForSeconds hurtWfs;
    protected Coroutine hurtCo;

    protected Coroutine attackCo;
    protected Coroutine deathCo;

    [SerializeField] protected Image healthBar;

    [SerializeField] protected float attackDelay;

    protected WaitForSeconds RealAttackDelay;

    public System.Action onDeath;

    public bool isAttackedByNormalAttack = false;

    [SerializeField] protected bool DoNotDropItem;

    [SerializeField] protected float damageOffset;

    private Vector2 damagePos => transform.position + new Vector3(0, damageOffset, 0);

    public bool IsDead => Model.CurHealth.Value <= 0;

    protected StageBarUI barUI;
    [Inject]
    public void Construct(StageBarUI ui)
    {
        barUI = ui;
    }
    void Awake()
    {
        InitComponent();
    }
    void OnEnable()
    {
        if (Model.BaseModel != null)
        {
            Model.InitSprite();
            PlaySpawnEffect();
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

    public virtual void PlaySpawnEffect()
    {
        int door = Model.Door;
        if (door == 0)
        {
            ParticleManager.Instance.GetParticle("M_34_Recall", transform.position);
        }
        if (door <= 25)
        {
            ParticleManager.Instance.GetParticle("M_12_Recall", transform.position);
        }
        else if (door <= 50)
        {
            ParticleManager.Instance.GetParticle("M_12_Recall", transform.position);
        }
        else if (door <= 75)
        {
            ParticleManager.Instance.GetParticle("M_34_Recall", transform.position);
        }
        else
        {
            ParticleManager.Instance.GetParticle("M_34_Recall", transform.position);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (IsInvulnerable) return;
        if (!gameObject.activeSelf) return;
        OnTakeDamage(amount);
        SetHealthBar();
    }
    protected virtual void OnTakeDamage(double amount)
    {
        Model.CurHealth.Value -= amount;
        DamageText text = DamageTextManager.Instance.Get(damagePos);
        text.SetText(BigCurrency.FromBaseAmount(amount).ToString(), new Color(0.38f,1,0.784f));
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
        onDeath?.Invoke();
        InGameManager.Instance.monsterDeathStack.Value--;
        barUI.TargetMonsterFill?.AddValue(1f / 12);
        if (attackCo != null) StopCoroutine(attackCo);
        deathCo = StartCoroutine(DeathRoutine());
        AudioManager.Instance.PlaySound("Monster_Dead");
        QuestManager.Instance.UpdateQuest("Monster", 1);
        QuestManager.Instance.ReportEvent(QuestTargetType.Monster, 1);
        PlayerProfileManager.Instance?.AddExp(Model.BaseModel.Exp);
    }
    public abstract void OnAttack(GameObject me, IDamagable target);
    protected abstract IEnumerator RealAttackRoutine(IDamagable target);



    protected virtual IEnumerator HurtEffectRoutine()
    {
        Model.SetSpriteColor(Color.black);
        yield return hurtWfs;
        Model.SetSpriteColor(Color.white);
        hurtCo = null;
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
        if (DoNotDropItem) return;
        int stage = Model.BaseModel.CurrentDoor;
        int door = (stage + 2) / 3;
        if (door >= 2)
        {
            DroppedItem i1 = PoolManager.Instance.ItemPool.GetItem(transform.position);
            i1.Init(DroppedItemType.Gold, Model.BaseModel.GoldQuant);
            i1.Shot();
            if (UnityEngine.Random.Range(1, 10) <= 1)
            {
                DroppedItem i4 = PoolManager.Instance.ItemPool.GetItem(transform.position);
                i4.Init(DroppedItemType.NormalChest, 1);
                i4.Shot();
            }
        }
        if (door >= 11)
        {
            DroppedItem i2 = PoolManager.Instance.ItemPool.GetItem(transform.position);
            i2.Init(DroppedItemType.Honbaeg, Model.BaseModel.SpiritBackQuant);
            i2.Shot();
        }
        if (door >= 151)
        {
            DroppedItem i3 = PoolManager.Instance.ItemPool.GetItem(transform.position);
            i3.Init(DroppedItemType.SpiritStone, Model.BaseModel.SoulStoneQuant);
            i3.Shot();
        }


    }

    protected virtual void SetHealthBar()
    {
        double val = Model.CurHealth.Value / Model.BaseModel.finalMaxHealth;
        float v = (float)val;
        float res = Mathf.Max(0, v);
        healthBar.fillAmount = res;
    }


    void OnDisable()
    {
        Model.SetSpriteColor(Color.white);
        if (attackCo != null) StopCoroutine(attackCo);
        if (hurtCo != null) StopCoroutine(hurtCo);
        if (deathCo != null) StopCoroutine(deathCo);
    }
}
