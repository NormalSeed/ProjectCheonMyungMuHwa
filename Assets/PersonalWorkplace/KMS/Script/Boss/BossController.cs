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

public class BossController : MonsterController
{
    public System.Action OnSpawnAmimEnd;

    [SerializeField] protected Transform selfEffectTrs;

    [SerializeField] int animationNumber;
    [SerializeField] string effectName;
    [SerializeField] float effectScale;
    [SerializeField] bool targetEffect;

    [SerializeField] protected string attackSound;

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
        treeAgent.SetVariableValue<float>("AttackDelay", Model.AttackDelay);
        treeAgent.SetVariableValue<BossController>("Controller", this);
        treeAgent.Restart();
    }
    public void OnSpawn()
    {
        IsInvulnerable = true;
        transform.localScale = Vector3.one;
        StartCoroutine(SpawnRoutine());
    }
    public override void PlaySpawnEffect()
    {
        int stage = Model.BaseModel.CurrentStage;
        if (stage < 100)
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
        else if (stage < 200)
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
        else if (stage < 300)
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
        else if (stage < 400)
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
        else
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
    }
    public override void OnDeath()
    {
        onDeath?.Invoke();
        if (attackCo != null) StopCoroutine(attackCo);
        InGameManager.Instance?.SetNextStage();
        if (InGameManager.Instance != null) InGameManager.Instance.monsterDeathStack.Value--;
        healthBar.transform.parent.gameObject.SetActive(false);
        AudioManager.Instance.PlaySound("Monster_Dead");
        StartCoroutine(DeathRoutine());
    }

    protected override void DropItem()
    {
        if (DoNotDropItem) return;
        int stage = Model.BaseModel.CurrentStage;
        int door = (stage + 2) / 3;
        if (door >= 2)
        {
            DroppedItem i1 = PoolManager.Instance.ItemPool.GetItem(transform.position);
            i1.Init(DroppedItemType.Gold, Model.BaseModel.GoldQuant);
            i1.Shot();
            DroppedItem i4 = PoolManager.Instance.ItemPool.GetItem(transform.position);
            i4.Init(DroppedItemType.EpicChest, 1);
            i4.Shot();
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
    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1f);

        OnSpawnAmimEnd?.Invoke();
        IsInvulnerable = false;
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, animationNumber);
        attackCo = StartCoroutine(RealAttackRoutine(target));
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound(attackSound);
        if (selfEffectTrs != null) ParticleManager.Instance.GetParticle(effectName, selfEffectTrs.position, scale: effectScale);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            if (targetEffect) ParticleManager.Instance.GetParticle(effectName, go.transform.position, scale: 1);
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = DamageTextManager.Instance.Get(go.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
        }
        attackCo = null;
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
