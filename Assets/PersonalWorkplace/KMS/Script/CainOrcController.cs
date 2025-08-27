using UnityEngine;
using System.Collections;
using VContainer;

public class CainOrcController : MonsterController
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
        Spum.PlayAnimation(PlayerState.ATTACK, 4);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return wfs;
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
