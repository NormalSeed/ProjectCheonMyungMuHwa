using System.Collections.Generic;
using UnityEngine;

public class JSynergySkill : SynergySkill
{
    [SerializeField] private List<PlayerController> players;
    private Vector3 spearPosition = new Vector3(0f, 4.5f, 0f);

    public override void PlaySkill()
    {
        // 시전 이펙트
        particleController.PlayParticle("FX_splash_spear_floor", spearPosition);

        // 버프 이펙트 + StatModifier 적용
        foreach (var player in players)
        {
            if (player.model == null) continue;

            particleController.PlayParticle("FX_splash_power_floor", player.transform.position);

            string charID = player.model.modelSO.CharID;

            // 중복 방지
            if (StatModifierManager.HasModifier(charID, "광명진기")) continue;

            var modifier = new StatModifier(
                StatType.Attack,
                0.5f,                         // 50% 증가
                ModifierSource.Synergy,
                "광명진기",
                isPercent: true,
                duration: 10f                  // 10초 지속
            );

            StatModifierManager.ApplyModifierWithDuration(charID, modifier, player.model);
            StatModifierManager.ApplyToModel(player.model); // 즉시 반영
        }

        Debug.Log("광명진기 버프 적용 완료");
    }
}
