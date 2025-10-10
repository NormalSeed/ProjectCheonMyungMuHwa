using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SynergyUI : MonoBehaviour
{
    [Header("시너지")]
    [SerializeField] private GameObject synergy;
    public List<SynergySlot> slots; // Inspector에서 3개 연결
    public List<SynergySkill> synergySkills;
    private SynergySkill currentSkill;
    public List<Sprite> synergySkillIcons;
    public GameObject synergyExplainUI;
    public Button explainButton;
    public Button synergySkillButton;
    public Image synergySkillShadow;
    public TextMeshProUGUI synergySkillRemainTime;

    [SerializeField] private Button synergyButton;

    [Header("데미지")]
    [SerializeField] private GameObject damageDealt;
    [SerializeField] private List<PlayerController> heros;
    [SerializeField] private List<GameObject> players;
    [SerializeField] private List<TextMeshProUGUI> damages;
    [SerializeField] private List<Image> playerIcons;
    [SerializeField] private List<Image> damageImage;
    [SerializeField] private List<Image> guageBackImages;
    [SerializeField] private List<Image> guages;

    [SerializeField] private Button damageDealtButton;

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

        synergyButton.onClick.AddListener(OnClickSynergyButton);
        damageDealtButton.onClick.AddListener(OnClickDamageDealtButton);

        UpdateDamageUI();
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

    private void OnClickSynergyButton()
    {
        synergy.SetActive(true);
        damageDealt.SetActive(false);
        UpdateSynergyUI(PartyManager.Instance.activeSynergies);
    }

    private void OnClickDamageDealtButton()
    {
        damageDealt.SetActive(true);
        synergy.SetActive(false);
        UpdateDamageUI();
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

    public void UpdateDamageUI()
    {
        // 활성화된 PlayerController만 있는 리스트
        var activeHeros = heros.Where(h => h.gameObject.activeInHierarchy).ToList();
        // 총 가한 데미지(모든 캐릭터가 가한 데미지의 총합)
        float totalDamage = activeHeros.Sum(h => h.damageDealt);

        for (int i = 0; i < activeHeros.Count && i < playerIcons.Count; i++)
        {
            players[i].SetActive(true);

            var hero = activeHeros[i];

            // 스프라이트 갱신(스프라이트는 전용 스프라이트로 변경해야함)
            var sprite = HeroSprites.Instance.GetCharaterFaceSprite(hero.charID.Value);
            playerIcons[i].color = Color.white;
            playerIcons[i].sprite = sprite;

            // 데미지 텍스트 갱신
            damages[i].text = BigCurrency.FromBaseAmount((double)hero.damageDealt).ToString();

            // 데미지 비율 이미지 갱신
            float ratio = totalDamage > 0f ? hero.damageDealt / totalDamage : 0f;
            damageImage[i].fillAmount = ratio;
            guageBackImages[i].color = Color.white;
            guages[i].color = Color.white;
            damageImage[i].color = Color.white;
        }

        for (int i = activeHeros.Count; i < players.Count; i++)
        {
            playerIcons[i].color = new Color(0, 0, 0, 0);
            guageBackImages[i].color = new Color(0, 0, 0, 0);
            guages[i].color = new Color(0, 0, 0, 0);
            damages[i].text = "";
            damageImage[i].color = new Color(0, 0, 0, 0);
        }
    }
}
