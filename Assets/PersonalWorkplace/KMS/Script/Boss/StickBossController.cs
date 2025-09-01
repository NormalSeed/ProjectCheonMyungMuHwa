using UnityEngine;

public class StickBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(RealAttackRoutine(target));
    }
}
