using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class HeroInfoUI : UIBase
{
    [Inject] private readonly EquipmentManager equipmentManager;

    #region SerializeField
    [Header("Button")]
    [SerializeField] private Button exitButton;         // 나가기 버튼
    [SerializeField] private Button upgradeButton;      // 레벨업 버튼
    [SerializeField] private Button stageUPButton;      // 승급 버튼

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI charName;  // 이름
    [SerializeField] private TextMeshProUGUI level;     // 레벨
    [SerializeField] private TextMeshProUGUI power;     // 종합전투력
    [SerializeField] private TextMeshProUGUI outPow;    // 외공
    [SerializeField] private TextMeshProUGUI inPow;     // 내공
    [SerializeField] private TextMeshProUGUI health;    // 체력
    [SerializeField] private TextMeshProUGUI heroPiece; // 영웅조각
    [SerializeField] private TextMeshProUGUI exp;       // 성장재화

    [Header("Root References")]
    [SerializeField] private Image characterRoot;          // 이미지
    [SerializeField] private Transform stageRoot;          // 돌파
    [SerializeField] private Transform badgeRoot;          // 팩션,소속
    [SerializeField] private Transform SkillRoot;          // 스킬정보

    [Header("Equipment")]
    [SerializeField] private InfoEquipButton weapone;      // 무기
    [SerializeField] private InfoEquipButton armor;        // 방어구
    [SerializeField] private InfoEquipButton boots;        // 부츠
    [SerializeField] private InfoEquipButton gloves;       // 글러브

    [Header("Panel")]
    [SerializeField] private GameObject heroInfoPanel;     // 자신의 오브젝트 정보
    [SerializeField] private GameObject equipPanel;
    [SerializeField] private HeroUI heroUI;
    #endregion

    #region SO Properties
    public HeroData heroData;
    private BigCurrency HealthPoint = new();     // 체력정보
    private BigCurrency InnAtkPoint = new();     // 내공
    private BigCurrency ExtAtkPoint = new();     // 외곻
    #endregion

    #region Goods Properties
    private BigCurrency requireGold = new();              // 필요 골드
    private int requirePiece;                    // 필요 영웅 조각
    private int ownerPiece;                      // 보유중인 영웅 조각
    #endregion

    #region Equip
    public string weaponID;
    public string armorID;
    public string bootsID;
    public string glovesID;
    #endregion

    #region Unity LiftCycle

    private void OnDisable()
    {
        equipPanel.SetActive(false);
        exitButton.onClick.RemoveListener(OnClickExit);
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        stageUPButton.onClick.RemoveListener(OnClickStageUP);
    }
    #endregion

    #region Init 
    public void Init()
    {
        ButtonAddListener();
        RefreshHeroUI();
    }
    private void SetEquipment()
    {
        SetEquipmentSettings();
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Weapon);
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Armor);
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Gloves);
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Boots);
    }
    private void SetEquipmentSettings()
    {
        EquipClass equipClass = (EquipClass)Enum.Parse(typeof(EquipClass), heroData.PlayerModelSO.Role);
        weapone.EquipmentSettingLoad(equipClass);
        armor.EquipmentSettingLoad(equipClass);
        boots.EquipmentSettingLoad(equipClass);
        gloves.EquipmentSettingLoad(equipClass);
    }
    private void PrepareHeroStats()
    {
        HealthPoint = BigCurrency.FromBaseAmount(heroData.cardInfo.HealthPoint);
        ExtAtkPoint = BigCurrency.FromBaseAmount(heroData.cardInfo.ExtAtkPoint);
        InnAtkPoint = BigCurrency.FromBaseAmount(heroData.cardInfo.InnAtkPoint);
        requireGold = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.Level * 500);

        ownerPiece = heroData.heroPiece;
        requirePiece = heroData.stage * (5 - (int)heroData.cardInfo.rarity);
    }
    private void ButtonAddListener()
    {
        exitButton.onClick.AddListener(OnClickExit);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        stageUPButton.onClick.AddListener(OnClickStageUP);
    }
    private void InfoTextSetting()
    {
        charName.text = heroData.PlayerModelSO.CharName;
        level.text = heroData.PlayerModelSO.Level.ToString();
        health.text = HealthPoint.ToString();
        outPow.text = ExtAtkPoint.ToString();
        inPow.text = InnAtkPoint.ToString();
        power.text = CountingHeroPower();
        exp.text = $"{requireGold} / {CurrencyManager.Instance.Model.Get(CurrencyType.Gold)}";
        heroPiece.text = heroData.stage >= 5 ? "돌파 불가능" : $"{requirePiece} / {ownerPiece}";
    }
    private void SetCharacter()
    {
        characterRoot.sprite = HeroSprites.Instance.GetCharacterSprite(heroData.heroId);
    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(heroData.cardInfo.faction.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetStage()
    {
        foreach (Transform stage in stageRoot)
        {
            stage.Find("Red")?.gameObject.SetActive(false);
            stage.Find("Gray")?.gameObject.SetActive(true);
        }

        for (int i = 1; i <= heroData.stage; i++)
        {
            Transform stage = stageRoot.Find("Stage" + i);
            if (stage != null)
            {
                stage.Find("Red")?.gameObject.SetActive(true);
                stage.Find("Gray")?.gameObject.SetActive(false);
            }
        }
    }
    // 버튼 설정여부
    private void SetUpgradeInteractable(Button btn)
    {
        btn.interactable = CurrencyManager.Instance.Model.Get(CurrencyType.Gold) >= requireGold;
    }
    private void SetRankUpInteractable(Button btn)
    {
        btn.interactable = ownerPiece >= requirePiece;
    }
    #endregion

    #region OnClick
    private void OnClickExit()
    {
        heroInfoPanel.gameObject.SetActive(false);
    }
    private void OnClickUpgrade()
    {
        HeroLevelUpgrade();
        RefreshCombatPower();
        RefreshHeroUI();
    }
    private void OnClickStageUP()
    {
        HeroRankUpPiece();
        RefreshCombatPower();
        RefreshHeroUI();
    }
    #endregion

    #region Private
    /// <summary>
    /// 영웅 강화하는 코드입니다.
    /// </summary>
    private void HeroLevelUpgrade()
    {
        if (CurrencyManager.Instance.TrySpend(CurrencyType.Gold, requireGold))
        {
            heroData.PlayerModelSO.Level++;
            GameEvents.HeroLevelChanged(heroData.PlayerModelSO.Level);
            requireGold = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.Level * 500);
            RequireLevelUpGold(heroData.PlayerModelSO.Level);
            CurrencyManager.Instance.SaveCharacterInfoToFireBase(heroData.cardInfo.HeroID, heroData.PlayerModelSO.Level);
            RefreshHeroUI();
        }
    }

    /// <summary>
    /// 전투력을 계산하는 코드입니다.
    /// </summary>
    /// <returns></returns>
    private string CountingHeroPower()
    {
        return BigCurrency.FromBaseAmount(heroData.cardInfo.combatPower).ToString();
    }

    /// <summary>
    /// 레벨업 시 필요한 골드 계산용 코드입니다.
    /// </summary>
    /// <param name="level"></param>
    private string RequireLevelUpGold(int level)
    {
        string myGold = CurrencyManager.Instance.Model.Get(CurrencyType.Gold).ToString();
        string reqGold = requireGold.ToString();
        string result = reqGold + " / " + myGold;
        return result;                 // 임시 계산식 level * 500
    }

    /// <summary>
    /// 캐릭터 돌파 코드입니다.
    /// </summary>
    /// <param name="piece"></param>
    private void HeroRankUpPiece()
    {
        if (heroData.stage >= 5) return;

        ownerPiece -= requirePiece;
        heroData.stage++;
        heroData.heroPiece = ownerPiece;
        GameEvents.HeroLevelChanged(heroData.PlayerModelSO.Level);
        heroUI.RefreshAllCards();
        CurrencyManager.Instance.SaveHeroStageToFireBase(heroData.cardInfo.HeroID, heroData.stage);
        CurrencyManager.Instance.SavePieceToFireBase(heroData.cardInfo.HeroID, ownerPiece);
        requirePiece = heroData.stage + (5 - (int)heroData.cardInfo.rarity) * (heroData.stage);
        RefreshHeroUI();
    }

    /// <summary>
    /// 전투력 갱신용 코드
    /// </summary>
    private void RefreshCombatPower()
    {
        power.text = BigCurrency.FromBaseAmount(heroData.cardInfo.combatPower).ToString();
    }
    #endregion

    #region Public

    /// <summary>
    /// 전체 UI를 새로고침하는 통합 메서드입니다.
    /// </summary>
    public async void RefreshHeroUI()
    {
        StatModifierManager.ApplyToCard(heroData.cardInfo);
        PrepareHeroStats();                          // 능력치 및 조각 계산
        InfoTextSetting();                           // 텍스트 UI 세팅
        SetUpgradeInteractable(upgradeButton);       // 레벨업 버튼 활성화 여부
        SetRankUpInteractable(stageUPButton);        // 돌파 버튼 활성화 여부
        RefreshCombatPower();                        // 전투력 갱신
        SetStage();                                  // 돌파 단계 표시
        SetBadge();                                  // 진영 표시
        SetEquipment();                              // 장비 설정
        SetCharacter(); // 캐릭터 이미지 로딩
    }

    /// <summary>
    /// 장착한 장비를 설정하는 코드입니다.
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="type"></param>
    public void GetEquipment(string charID, EquipmentType type)
    {
        Debug.Log("[GetEquipment] 진입");

        var equipButtons = new Dictionary<EquipmentType, InfoEquipButton>
    {
        { EquipmentType.Weapon, weapone },
        { EquipmentType.Armor, armor },
        { EquipmentType.Boots, boots },
        { EquipmentType.Gloves, gloves }
    };

        if (!equipButtons.TryGetValue(type, out var button))
        {
            Debug.LogWarning($"[GetEquipment] 버튼 매핑 실패: {type}");
            return;
        }

        var instance = equipmentManager.allEquipments
            .FirstOrDefault(e => e.charID == charID && e.equipmentType == type);
        
        if (instance != null)
        {
            button.HeroEquipSet(instance);
            switch (type)
            {
                case EquipmentType.Weapon:
                    weaponID = instance.instanceID;
                    break;
                case EquipmentType.Armor:
                    armorID = instance.instanceID;
                    break;
                case EquipmentType.Boots:
                    bootsID = instance.instanceID;
                    break;
                case EquipmentType.Gloves:
                    glovesID = instance.instanceID;
                    break;
            }
        }
        else
        {
            button.EquipReset();

            switch (type)
            {
                case EquipmentType.Weapon:
                    weaponID = null;
                    break;
                case EquipmentType.Armor:
                    armorID = null;
                    break;
                case EquipmentType.Boots:
                    bootsID = null;
                    break;
                case EquipmentType.Gloves:
                    glovesID = null;
                    break;
            }
        }
    }


    public void SetHeroData(HeroData data)
    {
        heroData = data;
        Init();
    }
    #endregion
}

/*
TODO : 영웅 정보 UI 작업 예정 목록
    골드 부족 시 버튼 상호작용 불가능 추가
    임시 작성한 영웅 레벨업, 돌파에 필요한 재화, 종합 전투력 계산식 수정하기
    영웅 돌파 표시 수정
 */
