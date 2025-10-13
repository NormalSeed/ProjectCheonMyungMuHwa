using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image skillIcon;                  // 아이콘
    [SerializeField] private TextMeshProUGUI skillName;         // 이름
    [SerializeField] private TextMeshProUGUI skillCoolDown;     // 발동조건 / 쿨다운
    [SerializeField] private TextMeshProUGUI skillSubscription; // 스킬 설명
    [SerializeField] private GameObject skillTag1;              // 스킬 유형1
    [SerializeField] private GameObject skillTag2;              // 스킬 유형2

    [Header("Skill Data")]
    public SkillDisplayDataSO skillData;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void InitSkill(SkillDisplayDataSO data)
    {
        skillData = data;

        if (skillData == null)
        {
            Debug.LogWarning("[Skill Display] : 스킬정보가 Null입니다.");
            return;
        }

        DisplaySkill(data);
    }

    private void DisplaySkill(SkillDisplayDataSO data)
    {
        // 기본 정보 설정
        skillIcon.sprite = data.icon;
        skillName.text = data.skillName;
        skillCoolDown.text = data.triggerText;
        skillSubscription.text = data.description;

        // 태그 텍스트 설정
        SetTagText(skillTag1, data.tag1);
        SetTagText(skillTag2, data.tag2);
    }

    private void SetTagText(GameObject tagObject, SkillTag tag)
    {
        TextMeshProUGUI tagText = tagObject.GetComponentInChildren<TextMeshProUGUI>();

        if (tag == SkillTag.None)
        {
            tagObject.SetActive(false);
        }
        else
        {
            tagObject.SetActive(true);
            tagText.text = tag.ToString();
        }
    }
}
