using UnityEngine;
using System.Collections;

public class StickOrcController : MonsterController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 0);
        attackCo = StartCoroutine(RealAttackRoutine(target));
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound("Stick_Attack");
        if (target != null)
        {
            target.TakeDamage((float)Model.BaseModel.finalAttackPower);
        }
        attackCo = null;

    }
}
