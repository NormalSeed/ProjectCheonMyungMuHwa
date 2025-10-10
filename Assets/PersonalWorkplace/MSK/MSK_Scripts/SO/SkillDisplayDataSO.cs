using UnityEngine;


[CreateAssetMenu(fileName = "SkillDisplay", menuName = "ScriptableObjects/SkillDisplay")]
public class SkillDisplayDataSO : ScriptableObject
{
    public string skillName;              // 스킬 이름
    [TextArea] public string description; // 설명 텍스트
    public Sprite icon;                   // 아이콘 이미지

    public string triggerText;            // 발동 조건 설명
    //public string damageText;             // 피해 설명
    //public string effectText;             // 추가 효과 설명

    public SkillTag tag;                  // 범위, 단일, 투사체, 소환 등
    //public Color tagColor;                // 태그 색상
}
