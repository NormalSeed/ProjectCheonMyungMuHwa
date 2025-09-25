using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SSynergySkill : SynergySkill
{
    [SerializeField] private List<PlayerController> players;
    private float partyCombatPower;
    private WaitForSeconds effectInterval = new WaitForSeconds(0.3f);

    private void Awake()
    {
        faction = HeroFaction.S;
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

        StartCoroutine(SkillEffectRoutine());
    }

    private IEnumerator SkillEffectRoutine()
    {
        // 화면 네 구석의 월드 좌표 계산
        float z = -Camera.main.transform.position.z;
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(200f, Screen.height - 200f, z));
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 200f, 200f, z));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 200f, Screen.height - 200f, z));
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(200f, 200f, z));
        Vector3 screenCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.transform.position.z * -1));

        // 순서: 좌상단 → 우하단 → 우상단 → 좌하단
        Vector3[] positions = new Vector3[] { topLeft, bottomRight, topRight, bottomLeft };

        foreach (var pos in positions)
        {
            particleController.PlayParticle("FX_splash_hit_01_air", pos);
            yield return effectInterval;
        }

        particleController.PlayParticle("FX_splash_hit_01_floor", screenCenter);

        // Monster 태그가 붙은 모든 오브젝트에 데미지 적용
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            IDamagable damagable = monster.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(partyCombatPower);
            }
        }
    }
}
