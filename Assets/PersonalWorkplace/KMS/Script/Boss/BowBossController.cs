using UnityEngine;

public class BowBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 2);
        StartCoroutine(RealAttackRoutine(target));
    }
}
