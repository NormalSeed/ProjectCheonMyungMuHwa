using UnityEngine;
using System.Collections;
using VContainer;

public class CainOrcController : MonsterController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 4);
        attackCo = StartCoroutine(RealAttackRoutine(target));
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound("Cane_Attack");
        if (target != null)
        {
            MonsterProjectile proj = PoolManager.Instance.MagicPool.GetItem(transform.position);
            proj.TargetPos = (target as PlayerController).transform.position;
            proj.Damage = Model.BaseModel.finalAttackPower;
            proj.Shot();
        }

    }
}
