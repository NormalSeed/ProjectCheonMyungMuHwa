using UnityEngine;
using System.Collections;

public class StickOrcController : MonsterController
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
        if (target != null)
        {
            target.TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem((target as TestPlayer).transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }

    }
}
