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

public abstract class BossController : MonsterController
{
    private bool isInvulnerable;
    public System.Action OnSpawnAmimEnd;
    protected override void SetValue()
    {
        Model.CurHealth.Value = Model.BaseModel.finalMaxHealth;
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
        InGameManager.Instance.SetNextStage();
        AudioManager.Instance.PlaySound("Monster_Dead");
        StartCoroutine(DeathRoutine());
    }
    private IEnumerator SpawnRoutine()
    {
        ParticleSystem part = ParticleManager.Instance.GetParticle("CFXR2 Firewall A", transform.position);
        for (int i = 0; i < 20; i++)
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
