using UnityEngine;
using System.Collections;

public class StickOrcController : MonsterController
{
    protected override void InitComponent()
    {
        base.InitComponent();
        RealAttackDelay = new WaitForSeconds(0.15f);
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 0);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound("Stick_Attack");
        if (target != null)
        {
            target.TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem((target as PlayerController).transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }

    }
}
