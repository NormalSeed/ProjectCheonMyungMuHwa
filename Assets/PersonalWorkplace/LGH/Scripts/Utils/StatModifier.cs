using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ModifierSource
{
    Equipment,
    Synergy,
    Buff,
    Debuff,
    Passive
}

public class StatModifier
{   
    public StatType statType;       //어떤 능력치에 영향을 주는지 결정하는 필드
    public double value;            // 증감값
    public ModifierSource source;   //Modifier의 출처(장비, 시너지 등)
    public string originID;         // 장비 ID, 시너지 이름 등
    public bool isPercent;          // 비율 기반 처리 여부
    public float duration;

    /// <summary>
    /// StatModifier 생성자
    /// </summary>
    /// <param name="statType">능력치 종류</param>
    /// <param name="value">절댓값이면 그대로, 비율값이면 1%당 0.01</param>
    /// <param name="source">장비, 시너지 등 Modifier 출처</param>
    /// <param name="originID">장비 ID, 시너지 이름 등 구별 가능한 값</param>
    /// <param name="isPercent">비율 기반 처리 여부</param>
    /// <param name="duration">버프 지속시간</param>
    public StatModifier(StatType statType, double value, ModifierSource source, string originID = "", bool isPercent = false, float duration = 0f)
    {
        this.statType = statType;
        this.value = value;
        this.source = source;
        this.originID = originID;
        this.isPercent = isPercent;
        this.duration = duration;
    }
}

public static class StatModifierManager
{
    private static Dictionary<string, List<StatModifier>> modifierCache = new();    // charID를 키로 해서 각 캐릭터의 Modifier 리스트를 저장. 캐릭터별로 어떤 Modifier가 적용됐는지 추적 가능

    /// <summary>
    /// Modifier 중복 방지를 위한 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="originID"></param>
    /// <returns></returns>
    public static bool HasModifier(string charID, string originID)
    {
        return modifierCache.ContainsKey(charID) &&
               modifierCache[charID].Any(m => m.originID == originID);
    }

    /// <summary>
    /// charID를 기반으로 해당 캐릭터에 Stat Modifier를 추가하는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="modifier"></param>
    public static void ApplyModifier(string charID, StatModifier modifier)
    {
        if (!modifierCache.ContainsKey(charID))
            modifierCache[charID] = new List<StatModifier>();

        var existing = modifierCache[charID]
            .FirstOrDefault(m => m.originID == modifier.originID && m.statType == modifier.statType);

        if (existing != null)
        {
            // 수치가 다르면 덮어쓰기
            if (existing.value != modifier.value || existing.isPercent != modifier.isPercent)
            {
                modifierCache[charID].Remove(existing);
                modifierCache[charID].Add(modifier);
            }
            // 수치가 같으면 무시
            return;
        }

        modifierCache[charID].Add(modifier);
    }

    /// <summary>
    /// 지속시간이 있는 Modifier를 적용시키기 위한 메서드 일정 시간이 지나면 제거됨
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="modifier"></param>
    /// <param name="context"></param>
    public static void ApplyModifierWithDuration(string charID, StatModifier modifier, MonoBehaviour context)
    {
        ApplyModifier(charID, modifier);

        if (modifier.duration > 0)
        {
            context.StartCoroutine(RemoveAfterDuration(charID, modifier.originID, modifier.duration));
        }
    }

    private static IEnumerator RemoveAfterDuration(string charID, string originID, double duration)
    {
        yield return new WaitForSeconds((float)duration);
        RemoveModifiersByOrigin(charID, originID);
    }

    /// <summary>
    /// charID를 기반으로 해당 캐릭터에서 originID를 가진 Modifier를 찾아 해제하는 로직
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="originID"></param>
    public static void RemoveModifiersByOrigin(string charID, string originID)
    {
        if (!modifierCache.ContainsKey(charID))
            return;

        modifierCache[charID].RemoveAll(m => m.originID == originID);
    }


    /// <summary>
    /// charID를 기반으로 해당 캐릭터에게서 특정 출처(source)의 Modifier를 모두 제거하는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="source"></param>
    public static void RemoveModifiers(string charID, ModifierSource source)
    {
        if (!modifierCache.ContainsKey(charID))
            return;

        modifierCache[charID].RemoveAll(m => m.source == source);
    }

    /// <summary>
    /// charID를 기반으로 해당 캐릭터의 해당 statType에 대해 Modifier들의 총합을 계산하는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="statType"></param>
    /// <param name="baseValue"></param>
    /// <returns></returns>
    public static double GetTotalModifier(string charID, StatType statType, double baseValue)
    {
        if (!modifierCache.ContainsKey(charID))
            return 0;

        double total = 0;

        foreach (var modifier in modifierCache[charID].Where(m => m.statType == statType))
        {
            if (modifier.isPercent)
                total += baseValue * modifier.value;
            else
                total += modifier.value;
        }

        return total;
    }

    /// <summary>
    /// Player ModleSO의 기본 능력치에 Modifier를 더해서 Player Model 안의 최종 능력치를 설정하는 메서드
    /// </summary>
    /// <param name="model"></param>
    public static void ApplyToModel(PlayerModel model)
    {
        string charID = model.modelSO.CharID;

        model.Health = model.modelSO.HealthPoint + GetTotalModifier(charID, StatType.Health, model.modelSO.HealthPoint);
        model.ExtAtk = model.modelSO.ExtAtkPoint + GetTotalModifier(charID, StatType.Attack, model.modelSO.ExtAtkPoint) + GetTotalModifier(charID, StatType.ExtAtk, model.modelSO.ExtAtkPoint);
        model.InnAtk = model.modelSO.InnAtkPoint + GetTotalModifier(charID, StatType.Attack, model.modelSO.InnAtkPoint) + GetTotalModifier(charID, StatType.InnAtk, model.modelSO.InnAtkPoint);
        model.Def = model.modelSO.DefPoint + GetTotalModifier(charID, StatType.Defense, model.modelSO.DefPoint);
        model.CritRate = (float)(model.modelSO.CritRate + GetTotalModifier(charID, StatType.CritRate, model.modelSO.CritRate));
        model.CritDamage = (float)(model.modelSO.CritDamage + GetTotalModifier(charID, StatType.CritDamage, model.modelSO.CritDamage));
        model.bossDamageBonus = (float)(model.bossDamageBonus + GetTotalModifier(charID, StatType.BDamage, 1));
        model.normalDamageBonus = (float)(model.normalDamageBonus + GetTotalModifier(charID, StatType.NDamage, 1));
        model.skillDamageBonus = (float)(model.skillDamageBonus + GetTotalModifier(charID, StatType.SkillDamage, 1));
    }
}