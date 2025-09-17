using UnityEngine;
using System.Collections;

public class SpiritBossController : BossController
{
    protected override IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        AudioManager.Instance.PlaySound(attackSound);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            ParticleManager.Instance.GetParticle("SpiritBoss_Effect", go.transform.position, scale: 1);
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = DamageTextManager.Instance.Get(go.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
        }
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlaySound(attackSound);
        foreach (GameObject go in players)
        {
            if (!go.activeSelf) continue;
            ParticleManager.Instance.GetParticle("SpiritBoss_Effect2", go.transform.position, scale: 1);
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = DamageTextManager.Instance.Get(go.transform.position);
            text.SetText(BigCurrency.FromBaseAmount(Model.BaseModel.finalAttackPower).ToString());
        }
    }
}
