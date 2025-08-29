using UnityEngine;
using System.Collections;
using VContainer;

public class CainOrcController : MonsterController
{

    protected override void InitComponent()
    {
        base.InitComponent();
        RealAttackDelay = new WaitForSeconds(0.15f);
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 4);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
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
