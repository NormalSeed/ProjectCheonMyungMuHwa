using System.Collections;
using UnityEngine;

public class PunchOrcController : MonsterController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 0);
        attackCo = StartCoroutine(RealAttackRoutine(target));
    }

    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound("Punch_Attack");
        if (target != null)
        {
            target.TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = DamageTextManager.Instance.Get((target as PlayerController).transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
            
        }

    }
}
