using UnityEngine;

public class LastBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 5);
        StartCoroutine(RealAttackRoutine(target));
    }
}
