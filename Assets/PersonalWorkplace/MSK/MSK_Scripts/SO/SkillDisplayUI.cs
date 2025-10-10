using UnityEngine;
using UnityEngine.UI;

public class SkillDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image iconImage;
    public Text nameText;
    public Text triggerText;
    public Text damageText;
    public Text effectText;
    public Text tagText;
    public Image tagBackground;

    [Header("Skill Data")]
    public SkillDisplayDataSO skillData;

    void Start()
    {
        if (skillData != null)
            ApplySkillDisplay(skillData);
    }

    public void ApplySkillDisplay(SkillDisplayDataSO data)
    {
        nameText.text = data.skillName;
        iconImage.sprite = data.icon;
        triggerText.text = data.triggerText;
        //damageText.text = data.damageText;
        //effectText.text = data.effectText;

        tagText.text = GetTagLabel(data.tag);
        //tagBackground.color = data.tagColor;
    }

    private string GetTagLabel(SkillTag tag)
    {
        switch (tag)
        {
            case SkillTag.Single: return "단일";
            case SkillTag.Area: return "범위";
            case SkillTag.Projectile: return "투사체";
            case SkillTag.Summon: return "소환";
            case SkillTag.Heal: return "회복";
            case SkillTag.Buff: return "버프";
            case SkillTag.Debuff: return "디버프";
            case SkillTag.Shield: return "보호막";
            default: return "없음";
        }
    }
}
