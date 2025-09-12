using System;
using System.Linq;
using TMPro;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentInfoPanel : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI textEquipmentName;       //장비 이름
    [SerializeField] private TextMeshProUGUI textEquipmentEffect;     //장비 효과

    [SerializeField] private TextMeshProUGUI textEffectRate;          //효과 수치;
    [SerializeField] private TextMeshProUGUI textUserStone;           //보유중인 연마석
    [SerializeField] private TextMeshProUGUI textNeedStone;           //소모 예상 연마석
    [SerializeField] private TextMeshProUGUI textEquipLevel;          //장비 레벨
    [SerializeField] private TextMeshProUGUI textNextLevel;           //다음 장비 레벨

    [SerializeField] private TextMeshProUGUI textPresentEff;          //현재 장비 효과
    [SerializeField] private TextMeshProUGUI textNextEff;             //다음 레벨 효과
    [SerializeField] private TextMeshProUGUI textPresentEffectRate;   //현재 장비 효과 수치
    [SerializeField] private TextMeshProUGUI textNextEffectRate;      //다음 레벨 효과 수치
    [SerializeField] private TextMeshProUGUI textEquip;               //장비/ 해제 텍스트

    [Header("EquipmentCardDisplay")]
    [SerializeField] private EquipmentCardDisplay imageEquipment;     // 장비 이미지
    
    [Header("Button")]
    [SerializeField] private Button exitButton;                   //나가기 버튼
    [SerializeField] private Button equipButton;                  //장비하기/ 해제하기 버튼
    [SerializeField] private Button upgradeButton;                //장비 강화 버튼

    [Header("Instance")]
    [SerializeField] private HeroInfoUI HeroInfo;

    private EquipmentInstance instance;     // 판넬의 장비
    private EquipmentInstance oldinstance;  // 장착중인 장비
    private int level;                      // 장비 레벨
    private int effectRate;                 // 증가량
    private int nextEffectRate;             // 다음 레벨 증가량
    private string charId;                  // 영웅 ID
    private string instanceID;              // 장비 ID

    #region Unity
    private void OnEnable()
    {
        SetButtonAddListener();
    }
    private void OnDisable()
    {
        SetButtonRemoveListener();
    }
    #endregion

    #region Init
    public void Init()
    {
        imageEquipment.SetData(instance);
        GetCharID(HeroInfo.heroData.PlayerModelSO.CharID);
        SetInstanceIDFromHeroData();

        SetPanelText();
    }
    private void SetInstanceIDFromHeroData()
    {
        var type = instance.equipmentType;
        var heroData = HeroInfo.heroData;

        instanceID = type switch
        {
            EquipmentType.Weapon => heroData.weapone,
            EquipmentType.Armor => heroData.armor,
            EquipmentType.Gloves => heroData.gloves,
            EquipmentType.Boots => heroData.boots,
            _ => null
        };
    }
    // 판넬의 텍스트 설정
    private void SetPanelText()
    {
        textEquipmentName.text = instance.template.equipmentName;           // 이름
        textEquipmentEffect.text = instance.template.description;           // 설명
        textEffectRate.text = instance.GetStat().ToString() + "%";                // 장비 효과
        textPresentEffectRate.text = instance.GetStat().ToString() + "%";         // 장비 효과
        textNextEffectRate.text = instance.GetNextLevelStat().ToString() + "%";   // 다음 레벨 효과
        // 상승 능력치
        //textPresentEff.text = 
        //textNextEff.text =
        // 연마석
        //textUserStone.text = $"{CurrencyManager.Instance.Model.Get(CurrencyType.)}";
        //textNeedStone.text = $"{CurrencyManager.Instance.Model.Get(CurrencyType.)}";

        textEquipLevel.text = "현재 단계" + instance.level.ToString();
        textNextLevel.text = "다음 단계" + (instance.level + 1).ToString();   
     
        if (instance.isEquipped)
        {
            if (instance.charID == HeroInfo.heroData.heroId)
            {
                textEquip.text = "해제하기";
            }
            else
            {
                textEquip.text = "교체하기";
            }
        }
        else
        {
            textEquip.text = "장비하기";
        }
    }

    private void SetButtonAddListener()
    {
        exitButton.onClick.AddListener(OnClickExit);
        equipButton.onClick.AddListener(OnClickEquip);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
    }
    private void SetButtonRemoveListener()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
        equipButton.onClick.RemoveListener(OnClickEquip);
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
    }
    #endregion
    // 판넬 종료
    #region OnClick Mathood
    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }
    // 장비 업그레이드
    private void OnClickUpgrade()
    {
        // TODO : 연마석 소모체크 추가하기
        instance.level++;
        SetPanelText();
    }
    // 장비를 장착
    private void OnClickEquip()
    {
        var heroData = HeroInfo.heroData;
        var type = instance.equipmentType;

        // 현재 슬롯에 장착된 장비 ID 가져오기
        SetInstanceIDFromHeroData();
        var currentEquippedId = instanceID;

        // 현재 슬롯에 장착된 장비 인스턴스 가져오기
        EquipmentInstance oldInstance = null;
        if (!string.IsNullOrEmpty(currentEquippedId))
        {
            oldInstance = equipmentManager.allEquipments
                .FirstOrDefault(e => e.instanceID == currentEquippedId);
        }

        // 1. 해제: 현재 장비가 장착되어 있고, 슬롯에 자신이 들어있을 경우
        if (currentEquippedId == instance.instanceID && instance.isEquipped)
        {
            SetHeroEquipmentSlot(null);
            instance.isEquipped = false;
            instance.charID = null;
            equipmentService.UnequipFromUnactivatedCharacter(charId, oldInstance);
            Debug.Log($"[OnClickEquip] 장비 {instance.instanceID} 해제됨");
        }
        else
        {
            // 2. 교체: 슬롯에 다른 장비가 이미 장착되어 있을 경우
            if (oldInstance != null && oldInstance != instance)
            {
                oldInstance.isEquipped = false;
                oldInstance.charID = null;
                equipmentService.UnequipFromUnactivatedCharacter(charId, oldInstance);
                Debug.Log($"[OnClickEquip] 기존 장비 {oldInstance.instanceID} 해제됨");
            }

            // 3. 장착: 새 장비를 슬롯에 등록
            SetHeroEquipmentSlot(instance.instanceID);
            instance.isEquipped = true;
            instance.charID = heroData.heroId;
            equipmentService.EquipToUnactivatedCharacter(charId, instance);
            Debug.Log($"[OnClickEquip] 장비 {instance.instanceID} 장착됨");
        }

        // 저장 및 UI 갱신
        if (!string.IsNullOrEmpty(heroData.heroId))
        {
            HeroDataManager.Instance.SaveHeroData(heroData.heroId);
        }
        else
        {
            Debug.LogError("[OnClickEquip] heroData.heroId가 null입니다. 저장 실패");
        }

        SetPanelText();
        HeroInfo.Init();
    }



    #endregion

    #region Private
    // 아이템 ID에 받아와 장비 타입 확인
    private void SetHeroEquipmentSlot(string itemId)
    {
        var type = instance.equipmentType;
        var heroData = HeroInfo.heroData;

        switch (type)
        {
            case EquipmentType.Weapon: heroData.weapone = itemId; break;
            case EquipmentType.Armor: heroData.armor = itemId; break;
            case EquipmentType.Gloves: heroData.gloves = itemId; break;
            case EquipmentType.Boots: heroData.boots = itemId; break;
        }
    }
        
    #endregion

    #region Public
    // 장비 받아오기
    public void GetEquipmentInstance(EquipmentInstance input)
    {
        instance = input;
    }
    // 장착한 캐릭터 ID 받아오기
    public void GetCharID(string input)
    {
        charId = input;
        var type = instance.equipmentType;

        string oldInstanceID = null;

        switch (type)
        {
            case EquipmentType.Weapon:
                oldInstanceID = HeroInfo.weaponID;
                break;
            case EquipmentType.Armor:
                oldInstanceID = HeroInfo.armorID;
                break;
            case EquipmentType.Boots:
                oldInstanceID = HeroInfo.bootsID;
                break;
            case EquipmentType.Gloves:
                oldInstanceID = HeroInfo.glovesID;
                break;
        }

        Debug.Log($"[GetCharID] : 캐릭터 {charId} 설정됨, 기존 장비 ID: {oldInstanceID}");
    }

    #endregion
}
