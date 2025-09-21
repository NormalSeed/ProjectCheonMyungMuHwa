using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyUI : MonoBehaviour
{
    public List<SynergySlot> slots; // Inspector에서 3개 연결
    public List<SynergySkill> synergySkills;
    private SynergySkill currentSkill;
    public List<Sprite> synergySkillIcons;
    public GameObject synergyExplainUI;
    public Button explainButton;
    public Button synergySkillButton;
    public Image synergySkillShadow;
    public TextMeshProUGUI synergySkillRemainTime;

    private bool canUseSynergySkill;
    [SerializeField] private float coolTime;

    private void Start()
    {
        foreach (var skill in synergySkills)
        {
            if (skill == null)
                Debug.LogWarning("synergySkills에 null이 있습니다.");
            else
                Debug.Log($"SynergySkill 연결됨: {skill.name} - Faction: {skill.faction}");
        }


        foreach (SynergySlot slot in slots) 
        {
            slot.gameObject.SetActive(false);
        }

        explainButton.onClick.AddListener(ToggleExplainUI);

        coolTime = 0f;
        canUseSynergySkill = true;
        synergySkillButton.onClick.AddListener(OnClickSynergySkillButton);
    }

    private void Update()
    {
        if (coolTime > 0f)
        {
            canUseSynergySkill = false;
            coolTime -= Time.deltaTime;
            synergySkillButton.interactable = false;
            synergySkillShadow.enabled = true;
            synergySkillRemainTime.enabled = true;

            int intCool = Mathf.CeilToInt(coolTime);

            synergySkillRemainTime.text = intCool.ToString();
        }
        else
        {
            canUseSynergySkill = true;
            synergySkillButton.interactable = true;
            synergySkillShadow.enabled = false;
            synergySkillRemainTime.enabled = false;
        }
    }

    public void UpdateSynergyUI(List<SynergyInfo> synergyInfos)
    {
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < synergyInfos.Count; i++)
        {
            var info = synergyInfos[i];
            slots[i].SetData(info.faction.ToString(), info.stage, info.count);
            slots[i].gameObject.SetActive(true);
        }
    }

    public void ToggleExplainUI()
    {
        synergyExplainUI.SetActive(!synergyExplainUI.activeSelf);
    }

    public void SetSynergySkillButtonState(SynergyInfo selected)
    {
        var buttonImage = synergySkillButton.GetComponent<Image>();

        if (selected == null)
        {
            synergySkillButton.interactable = false;
            buttonImage.enabled = false;
            return;
        }

        Dictionary<HeroFaction, int> iconIndexMap = new()
    {
        { HeroFaction.J, 0 },
        { HeroFaction.S, 1 },
        { HeroFaction.M, 2 }
    };

        if (!iconIndexMap.TryGetValue(selected.faction, out int iconIndex) || iconIndex >= synergySkillIcons.Count)
        {
            synergySkillButton.interactable = false;
            buttonImage.enabled = false;
            return;
        }

        buttonImage.sprite = synergySkillIcons[iconIndex];
        buttonImage.enabled = true;
        synergySkillButton.interactable = true;

        // 버튼 클릭 이벤트 연결
        var skill = synergySkills.FirstOrDefault(s => s.faction == selected.faction);
        if (skill != null)
        {
            currentSkill = synergySkills.FirstOrDefault(s => s.faction == selected.faction);
            synergySkillButton.onClick.RemoveAllListeners();
            synergySkillButton.onClick.AddListener(OnClickSynergySkillButton);

            Debug.Log($"합격 시너지 버튼 설정 완료: {selected.faction} - Stage {selected.stage}");
        }
        else
        {
            Debug.LogWarning($"해당 진영의 SynergySkill이 없습니다: {selected.faction}");
            synergySkillButton.interactable = false;
            buttonImage.enabled = false;
        }
    }

    private void OnClickSynergySkillButton()
    {
        if (!canUseSynergySkill || currentSkill == null) return;

        currentSkill.PlaySkill();
        coolTime = 60f;
    }
}
