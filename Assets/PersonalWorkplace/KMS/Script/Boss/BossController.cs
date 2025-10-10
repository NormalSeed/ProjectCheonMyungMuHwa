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
    public System.Action OnSpawnAmimEnd;

    [SerializeField] protected Transform selfEffectTrs;

    [SerializeField] int animationNumber;
    [SerializeField] string effectName;
    [SerializeField] float effectScale;
    [SerializeField] bool targetEffect;

    [SerializeField] protected string attackSound;

    [SerializeField] protected string spawnEffect;
    protected override void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
        treeAgent.SetVariableValue<float>("AttackDelay", Model.AttackDelay);
        treeAgent.SetVariableValue<BossController>("Controller", this);
        treeAgent.Restart();
    }
    public void OnSpawn()
    {
        barUI.SetHealthBarText(new BigCurrency(Model.CurHealth.Value), new BigCurrency(Model.BaseModel.finalMaxHealth));
        IsInvulnerable = true;
        transform.localScale = Vector3.one;
        StartCoroutine(SpawnRoutine());
    }
    public override void PlaySpawnEffect()
    {
        if (spawnEffect != "")
        {
            ParticleManager.Instance.GetParticle(spawnEffect, transform.position);
            return;
        }
        int door = Model.Door;
        if (door == 0)
        {
            ParticleManager.Instance.GetParticle("Boss_4_Recall", transform.position);
        }
        if (door <= 25)
        {
            ParticleManager.Instance.GetParticle("Boss_1_Recall", transform.position);
        }
        else if (door <= 50)
        {
            ParticleManager.Instance.GetParticle("Boss_2_Recall", transform.position);
        }
        else if (door <= 75)
        {
            ParticleManager.Instance.GetParticle("Boss_3_Recall", transform.position);
        }
        else
        {
            ParticleManager.Instance.GetParticle("Boss_4_Recall", transform.position);
        }
    }
    public override void OnDeath()
    {
        onDeath?.Invoke();
        barUI.Timer.Stop();
        if (attackCo != null) StopCoroutine(attackCo);
        InGameManager.Instance?.SetNextStage();
        if (InGameManager.Instance != null) InGameManager.Instance.monsterDeathStack.Value--;
        AudioManager.Instance.PlaySound("Monster_Dead");
        StartCoroutine(DeathRoutine());
    }

    protected override void DropItem()
    {
        if (DoNotDropItem) return;
        int door = Model.BaseModel.CurrentDoor;
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
            go.GetComponent<IDamagable>().TakeDamage((float)Model.BaseModel.finalAttackPower);
        }
        attackCo = null;
    }

    protected override void SetHealthBar()
    {
        double val = Model.CurHealth.Value / Model.BaseModel.finalMaxHealth;
        float v = (float)val;
        float res = Mathf.Max(0, v);
        barUI.SetFill(res);
        float h = Mathf.Max((float)Model.CurHealth.Value, 0f);
        barUI.SetHealthBarText(new BigCurrency(h), new BigCurrency(Model.BaseModel.finalMaxHealth));
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
