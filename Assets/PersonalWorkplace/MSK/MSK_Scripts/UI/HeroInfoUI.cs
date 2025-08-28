using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HeroInfoUI : UIBase
{
    private string modelPath = "LGH/PlayerModels/";

    #region SerializeField
    [Header("Hero Info SO")]
    [SerializeField] private CardInfo chardata;         // 캐릭터 카드 SO
    [SerializeField] private PlayerModelSO modelInfo;   // 캐릭터 스텟 SO
    [SerializeField] private SkillSet skillInfo;        // 캐릭터 스킬 SO

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

    [Header("Panel")]
    [SerializeField] private GameObject heroInfoPanel;     // 자신의 오브젝트 정보
    #endregion

    #region CardInfo SO Properties
    // CardInfo SO 에서 받아오는 정보들
    private string heroID;              // 캐릭터 ID
    private int heroStage;              // 돌파정보
    private HeroRarity rarity;          // 레어도
    private HeroFaction faction;        // 진영
    #endregion

    #region PlayerModel SO Properties
    private string heroName;        // 이름정보
    private int heroLevel;          // 레벨정보
    private double HealthPoint;     // 체력정보
    private double FinalPower;      // 종합전투
    private double InnAtkPoint;     // 내공????
    private double ExtAtkPoint;     // 외곻????
    #endregion

    #region Goods Properties
    private int requireGold = 1000;              // 필요 골드
    private int requirePiece;                    // 필요 영웅 조각
    private int ownerPiece;                      // 보유중인 영웅 조각
    #endregion

    #region Unity LiftCycle
    private void OnEnable()
    {
        Init();
    }
    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        stageUPButton.onClick.RemoveListener(OnClickStageUP);
    }
    #endregion

    #region Init
    private void Init()
    {
        // CardInfo 정보
        CardInfInit();
        // PlayerModelSO 정보
        ModelInfoInit();
        // 버튼 리스너 추가
        ButtonAddListener();
        // 텍스트정보 추가
        InfoTextSetting();
        // 전투력 계산 코드
        CountingHeroPower();
        // HeroInfoSetting 에 있는 함수 그대로
        SetCharacter();         // 이미지세팅
        SetStage();             // 돌파세팅
        SetBadge();             // 진영(소속) 세팅

        Debug.Log("HeroInfoUI Init 실행됨");
    }
    private void CardInfInit()
    {
        heroID = chardata.HeroID;
        heroStage = chardata.HeroStage;
        rarity = chardata.rarity;
        faction = chardata.faction;

        modelInfo = Resources.Load<PlayerModelSO>(modelPath + heroID + "_model");
    }
    private async void ModelInfoInit()
    {
        heroName = modelInfo.CharName;
        heroLevel = await CurrencyManager.Instance.LoadCharatorInfoFromFireBase(heroID);
        Debug.Log($"[ModelInfoInit] : 불러온 영웅 레벨 {heroLevel}");
        HealthPoint = modelInfo.HealthPoint;
        ExtAtkPoint = modelInfo.ExtAtkPoint;
        InnAtkPoint = modelInfo.InnAtkPoint;
    }
    private void ButtonAddListener()
    {
        exitButton.onClick.AddListener(OnClickExit);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        stageUPButton.onClick.AddListener(OnClickStageUP);
    }
    private void InfoTextSetting()
    {
        charName.text = heroName;
        level.text = heroLevel.ToString();
        power.text = FinalPower.ToString();

        // 인권님 수치 단위 변환 코드 참고가 필요함
        inPow.text = InnAtkPoint.ToString();
        outPow.text = ExtAtkPoint.ToString();
        health.text = HealthPoint.ToString();
    }
    private void SetCharacter()
    {
        Addressables.LoadAssetAsync<Sprite>(heroID + "_sprite").Completed += task =>
        {
            if (task.Status == AsyncOperationStatus.Succeeded)
            {
                characterRoot.sprite = task.Result;
            }
        };
    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(faction.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetStage()
    {
        foreach (Transform stage in stageRoot)
        {
            Transform red = stage.Find("Stage_Red");
            Transform gray = stage.Find("Stage_gray");

            if (red != null) red.gameObject.SetActive(false);
            if (gray != null) gray.gameObject.SetActive(true);
        }

        for (int i = 1; i <= heroStage; i++)
        {
            Transform stage = stageRoot.Find("Stage" + i);
            if (stage != null)
            {
                Transform red = stage.Find("Stage_Red");
                Transform gray = stage.Find("Stage_gray");

                if (red != null) red.gameObject.SetActive(true);
                if (gray != null) gray.gameObject.SetActive(false);
            }
        }
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
        HeroStageUpgrade();
    }
    #endregion

    #region Private

    /// <summary>
    /// 영웅을 돌파하는 코드입니다.
    /// </summary>
    private void HeroStageUpgrade()
    {
       // CurrencyManager.Instance.SaveHeroPieceToFireBase(heroID, ownerPiece);
    }

    /// <summary>
    /// 영웅 강화하는 코드입니다.
    /// </summary>
    private void HeroLevelUpgrade()
    {
        bool result = CurrencyManager.Instance.TrySpend(CurrencyType.Gold, new BigCurrency(requireGold, 0));

        if (result)
        {
            heroLevel++;
            level.text = heroLevel.ToString();
            CurrencyManager.Instance.SaveCharatorInfoToFireBase(heroID, heroLevel);
            Debug.Log("[HeroLevelUpgrade] : 골드 소모");
        }
        else
            Debug.Log("[HeroLevelUpgrade] :골드 부족함");
    }

    /// <summary>
    /// 전투력을 계산하는 코드입니다.
    /// </summary>
    /// <returns></returns>
    private void CountingHeroPower()
    {
        FinalPower = 0;
    }
    /// <summary>
    /// 레벨업 시 필요한 골드 계산용 코드입니다.
    /// </summary>
    /// <param name="level"></param>
    private void RequireLevelUpGold(int level)
    {   
        requireGold = heroLevel * 1000;                 // 임시 계산식
    }
    /// <summary>
    /// 캐릭터 돌파 시 필요한 영웅조각 계산용의 코드입니다.
    /// </summary>
    /// <param name="piece"></param>
    private void RequireRankUpPiece(int piece)
    {
        requirePiece = heroStage * (5 - (int)rarity);   // 임시 계산식
    }
    /// <summary>
    /// 정보 동기화용 코드입니다.
    /// </summary>
    /// <param name="Info"></param>
    private void OnInfoValueChange()
    {

    }
    #endregion

    #region Public

    /// <summary>
    /// CardInfo SO 세팅용 코드입니다.
    /// </summary>
    /// <param name="Info"></param>
    public void HeroSOInfoSetting(CardInfo Info)
    {
        chardata = Info;
        Debug.Log($"SO {Info}로 세팅됨");
    }
    #endregion
}


/*
TODO : 영웅 정보 UI 작업 예정 목록
    나가기 버튼 이미지 교체하기
    골드 부족 시 버튼 상호작용 불가능 추가
    전투력 계산 식 추가
    소유 골드 정보 가져오기
    소모 골드 정보 가져오기
    소유 카드 조각 가져오기
    돌파용 카드조각 개수 가져오기
    임시 작성한 영웅 레벨업, 돌파에 필요한 재화 계산식 수정하기
 */
