using UnityEngine;
using System.Collections;

public class CaneBossController : BossController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 4);
        StartCoroutine(RealAttackRoutine(target));
    }
    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        ParticleManager.Instance.GetParticle("NormalBoss2_AtkEffect", selfEffectTrs.position, scale: 3);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = DamageTextManager.Instance.Get(go.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
        }
    }
}
