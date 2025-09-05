using UnityEngine;
using System.Collections;

public class GoldBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 0);
        StartCoroutine(RealAttackRoutine(target));
    }
    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        ParticleManager.Instance.GetParticle("5Boss_Attack", selfEffectTrs.position, scale: 3);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem(go.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
        }
    }
}
