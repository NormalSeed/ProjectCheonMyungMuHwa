using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MSynergySkill : SynergySkill
{
    [SerializeField] private List<PlayerController> players;
    private float partyCombatPower;
    private WaitForSeconds damageInterval = new WaitForSeconds(1f);
    private Vector3 effectPosition = new Vector3(0f, -4.5f, 0f);

    private void Awake()
    {
        faction = HeroFaction.M;
    }

    public override void PlaySkill()
    {
        partyCombatPower = 0f;

        //파티 전투력 계산
        foreach (var player in players)
        {
            if (player.model == null) continue;

            float combatPower = 2 * ((player.model.ExtAtk + player.model.InnAtk) * (1f + player.model.CritRate * (player.model.CritDamage - 1f)) + 1.4f * player.model.Def + 0.1f * player.model.Health);
            partyCombatPower += combatPower;
        }

        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        particleController.PlayParticle("FX_splash_portal_floor", Vector3.zero);

        for (int i = 0; i < 10; i++)
        {
            // Monster 태그가 붙은 모든 오브젝트에 데미지 적용
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
            foreach (GameObject monster in monsters)
            {
                IDamagable damagable = monster.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    particleController.PlayParticle("FX_splash_hit_02_air", monster.transform.position + effectPosition);
                    damagable.TakeDamage(partyCombatPower * 0.5f * 0.05f);
                }
            }

            yield return damageInterval;
        }
    }
}
