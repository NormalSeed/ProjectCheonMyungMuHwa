using UnityEngine;

public class HonBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(RealAttackRoutine(target));
    }
}
