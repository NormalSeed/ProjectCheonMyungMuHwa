using System.Collections;
using UnityEngine;

public class PunchOrcController : MonsterController
{

    [SerializeField] float realAttackDelay;
    private WaitForSeconds wfs;

    protected override void InitComponent()
    {
        base.InitComponent();
        wfs = new WaitForSeconds(realAttackDelay);
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 0);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return wfs;
        AudioManager.Instance.PlaySound("Punch_Attack");
        if (target != null)
        {
            target.TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem((target as PlayerController).transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }

    }
}
