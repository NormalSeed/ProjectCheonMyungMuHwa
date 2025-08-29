using UnityEngine;
using System.Collections;

public class PunchBossController : BossController
{
    protected override void InitComponent()
    {
        base.InitComponent();
        RealAttackDelay = new WaitForSeconds(0.15f);
    }
    public override void OnAttack(GameObject me, IDamagable target)
    {
        Spum.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(RealAttackRoutine(target));
    }

    private IEnumerator RealAttackRoutine(IDamagable target)
    {
        yield return RealAttackDelay;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            go.GetComponent<IDamagable>().TakeDamage(Model.BaseModel.finalAttackPower);
            DamageText text = PoolManager.Instance.DamagePool.GetItem(go.transform.position);
            text.SetText(Model.BaseModel.finalAttackPower);
        }
    }
}
