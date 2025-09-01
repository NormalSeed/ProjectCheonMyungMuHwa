using UnityEngine;

public class CaneBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 4);
        StartCoroutine(RealAttackRoutine(target));
    }
}
