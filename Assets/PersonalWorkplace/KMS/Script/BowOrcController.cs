using UnityEngine;
using System.Collections;
using VContainer;

public class BowOrcController : MonsterController
{

    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 2);
        attackCo = StartCoroutine(RealAttackRoutine(target));
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound("Bow_Attack");
        if (target != null)
        {
            MonsterProjectile proj = PoolManager.Instance.ArrowPool.GetItem(transform.position);
            proj.TargetPos = (target as PlayerController).transform.position;
            proj.Damage = Model.BaseModel.finalAttackPower;
            proj.Shot();
        }

    }
}
