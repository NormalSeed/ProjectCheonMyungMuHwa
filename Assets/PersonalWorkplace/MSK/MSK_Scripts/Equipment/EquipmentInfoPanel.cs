using System;
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

    private EquipmentInstance instance;
    private int level;                      // 장비 레벨
    private int effectRate;                 // 증가량
    private int nextEffectRate;             // 다음 레벨 증가량
    private string charId;

    #region Unity
    private void OnEnable()
    {
        Init();
        SetButtonAddListener();
    }
    private void OnDisable()
    {
        SetButtonRemoveListener();
    }
    #endregion

    #region Init
    private void Init()
    {
        charId = HeroInfo.heroData.heroId;
        imageEquipment.SetData(instance);
        SetPanelText();
    }
    private void SetPanelText()
    {
        textEquipmentName.text = instance.template.equipmentName;
        textEquipmentEffect.text = instance.template.description;
        //textEffectRate.text =
        //textUserStone.text = $"{CurrencyManager.Instance.Model.Get(CurrencyType.)}";
        //textNeedStone.text = $"{CurrencyManager.Instance.Model.Get(CurrencyType.)}";

        textEquipLevel.text = instance.level.ToString();
        textNextLevel.text = instance.level + 1.ToString();
        //textPresentEff.text
        //textNextEff.text
        //textPresentEffectRate

        //textNextEffectRate
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

    #region OnClick Mathood
    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClickUpgrade()
    {
        // TODO : 연마석 소모체크 추가하기
        instance.level++;
        SetPanelText();
    }

    private void OnClickEquip()
    {
        var character = equipmentService.GetCharacter(charId);
        if (character == null)
        {
            Debug.LogWarning($"캐릭터 {charId}를 찾을 수 없습니다.");
            return;
        }

        var slot = character.slots[instance.equipmentType];

        // 이미 해당 슬롯에 이 장비가 장착되어 있다면 해제
        if (slot.equippedItem == instance && instance.isEquipped)
        {
            equipmentService.UnequipFromCharacter(charId, instance.equipmentType);
            Debug.Log($"장비 {instance.templateID} 해제됨");
        }
        else
        {
            // 다른 장비가 장착되어 있거나 비어 있다면 장착
            equipmentService.EquipToCharacter(charId, instance);
            Debug.Log($"장비 {instance.templateID} 장착됨");
        }
        SetPanelText();
        HeroInfo.Init();
    }
    #endregion

    #region Private
    #endregion

    #region Public
    public void GetEquipmentInstance(EquipmentInstance input)
    {
        instance = input;
    }
    #endregion
}
