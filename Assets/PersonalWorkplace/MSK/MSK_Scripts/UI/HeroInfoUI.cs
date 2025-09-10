using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class HeroInfoUI : UIBase
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

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
    #endregion

    #region SO Properties
    private HeroData heroData;
    private BigCurrency HealthPoint = new();     // 체력정보
    private BigCurrency FinalPower = new();      // 종합전투
    private BigCurrency InnAtkPoint = new();     // 내공????
    private BigCurrency ExtAtkPoint = new();     // 외곻????
    #endregion

    #region Goods Properties
    private BigCurrency requireGold = new();              // 필요 골드
    private int requirePiece;                    // 필요 영웅 조각
    private int ownerPiece;                      // 보유중인 영웅 조각
    #endregion

    #region Equip
    private Dictionary<string, EquipmentInstance> equips = new();

    #endregion
    #region FireBase
    private string _uid;
    private DatabaseReference _dbRef;
    #endregion

    #region Unity LiftCycle
    private void OnEnable()  { }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        stageUPButton.onClick.RemoveListener(OnClickStageUP);
    }
    #endregion

    #region Init 
    private async void Init()
    {
        ButtonAddListener();                 // 버튼 리스너 연결
        PrepareHeroStats();                 // 능력치 및 조각 계산
        InfoTextSetting();                  // 텍스트 UI 세팅
        await SetCharacter(heroData.PlayerModelSO.SpriteKey); // 이미지 로딩
        SetStage();                         // 돌파 단계 표시
        SetBadge();                         // 진영 표시
        SetUpgradeInteractable(upgradeButton); // 레벨업 버튼 활성화 여부
        SetRankUpInteractable(stageUPButton);  // 돌파 버튼 활성화 여부
        SetEquipment();                        // 장착 중 장비 설정
    }
    private void SetEquipment()
    {
        Debug.Log("[SetEquipment] : 불러오는 중 Weapon");
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Weapon);
        Debug.Log("[SetEquipment] : 불러오는 중Armor");
        //armor.GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Armor);
        Debug.Log("[SetEquipment] : 불러오는 중 Gloves");
        GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Gloves);
        Debug.Log("[SetEquipment] : 불러오는 중Boots");
        //boots.GetEquipment(heroData.PlayerModelSO.CharID, EquipmentType.Boots);
    }

    private void PrepareHeroStats()
    {
        HealthPoint = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.HealthPoint);
        ExtAtkPoint = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.ExtAtkPoint);
        InnAtkPoint = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.InnAtkPoint);
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
        health.text = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.HealthPoint).ToString();
        outPow.text = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.ExtAtkPoint).ToString();
        inPow.text = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.InnAtkPoint).ToString();
        power.text = CountingHeroPower();
        exp.text = $"{requireGold} / {CurrencyManager.Instance.Model.Get(CurrencyType.Gold)}";
        heroPiece.text = heroData.stage >= 5 ? "돌파 불가능" : $"{requirePiece} / {ownerPiece}";
    }

    private async Task SetCharacter(string spriteKey)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            characterRoot.sprite = handle.Result;
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
        Debug.Log("정보 UI 비활성화 입력됨");
        heroInfoPanel.gameObject.SetActive(false);
    }
    private void OnClickUpgrade()
    {
        HeroLevelUpgrade();
    }
    private void OnClickStageUP()
    {
        HeroRankUpPiece();
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
            level.text = heroData.PlayerModelSO.Level.ToString();
            requireGold = BigCurrency.FromBaseAmount(heroData.PlayerModelSO.Level * 500);
            exp.text = $"{requireGold} / {CurrencyManager.Instance.Model.Get(CurrencyType.Gold)}";

            CurrencyManager.Instance.SaveCharacterInfoToFireBase(heroData.cardInfo.HeroID, heroData.PlayerModelSO.Level);

            // GameEvents 안의 이벤트 호출
            GameEvents.HeroLevelChanged(heroData.PlayerModelSO.Level);

            SetUpgradeInteractable(upgradeButton);
        }
    }

    /// <summary>
    /// 전투력을 계산하는 코드입니다.
    /// </summary>
    /// <returns></returns>
    private string CountingHeroPower()
    {
        double power = heroData.PlayerModelSO.ExtAtkPoint *
                       heroData.PlayerModelSO.HealthPoint *
                       heroData.PlayerModelSO.InnAtkPoint * 0.7;

        return BigCurrency.FromBaseAmount(power).ToString();
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
        // 5회 아상 돌파방지 코드
        if (heroData.stage >= 5) return;

        ownerPiece -= requirePiece;
        heroData.stage++;
        heroData.heroPiece = ownerPiece;
        //  돌파저장
        CurrencyManager.Instance.SaveHeroStageToFireBase(heroData.cardInfo.HeroID, heroData.stage);
        CurrencyManager.Instance.SavePieceToFireBase(heroData.cardInfo.HeroID, ownerPiece);

        requirePiece = heroData.stage * (5 - (int)heroData.cardInfo.rarity);
        heroPiece.text = heroData.stage >= 5 ? "돌파 불가능" : $"{requirePiece} / {ownerPiece}";

        SetStage();
        SetRankUpInteractable(stageUPButton);
    }
    #endregion

    #region Public
    public void GetEquipment(string charID, EquipmentType type)
    {
        EquipmentInstance EquipmentInstance;

        Debug.Log("[GetEquipment] 진입 ");
        foreach (var istance in equipmentManager.allEquipments)
        {
            if (istance.charID == charID && istance.equipmentType == type)
            {
                switch (type)
                {
                    case (EquipmentType.Weapon):
                        {
                            weapone.HeroEquipSet(istance);
                            break;
                        }
                    case (EquipmentType.Armor):
                        {
                            armor.HeroEquipSet(istance);
                            break;
                        }
                    case (EquipmentType.Boots):
                        {
                            boots.HeroEquipSet(istance);
                            break;
                        }
                    case (EquipmentType.Gloves):
                        {
                            gloves.HeroEquipSet(istance);
                            break;
                        }
                }
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
