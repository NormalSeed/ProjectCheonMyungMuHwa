using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoUI : UIBase
{
    [Header("Hero Info SO")]
    [SerializeField] private CardInfo chardata;         // 캐릭터 카드 SO
    [SerializeField] private PlayerModelSO ModelInfo;   // 캐릭터 스텟 SO
    [SerializeField] private SkillSet SkillInfo;        // 캐릭터 스킬 SO

    [Header("Button")]
    [SerializeField] private Button exitButton;         // 나가기 버튼
    [SerializeField] private Button upgradeButton;      // 레벨업 버튼
    [SerializeField] private Button stageUPButton;      // 승급 버튼

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI charName;  // 이름
    [SerializeField] private TextMeshProUGUI level;     // 레벨
    [SerializeField] private TextMeshProUGUI power;     // 전투력
    [SerializeField] private TextMeshProUGUI outPow;    // 외공
    [SerializeField] private TextMeshProUGUI inPow;     // 내공
    [SerializeField] private TextMeshProUGUI health;    // 체력
    [SerializeField] private TextMeshProUGUI heroPiece; // 영웅조각
    [SerializeField] private TextMeshProUGUI exp;       // 성장재화

    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot; // 레어도
    [SerializeField] private Transform characterRoot;      // 이미지
    [SerializeField] private Transform stageRoot;          // 돌파
    [SerializeField] private Transform badgeRoot;          // 팩션,소속
    [SerializeField] private Transform SkillRoot;          // 스킬정보

    [Header("Panel")]
    [SerializeField] private GameObject heroInfoPanel;     // 자신의 오브젝트 정보

    private string heroID;              // 캐릭터 ID
    private int heroStage;              // 돌파정보
    private HeroRarity rarity;          // 레어도
    private HeroFaction faction;        // 진영

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

        heroID = chardata.HeroID;
        heroStage = chardata.HeroStage;
        rarity = chardata.rarity;
        faction = chardata.faction;

        exitButton.onClick.AddListener(OnClickExit);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        stageUPButton.onClick.AddListener(OnClickStageUP);
        Debug.Log("HeroInfoUI Init 실행됨");
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

    }

    /// <summary>
    /// 영웅 강화하는 코드입니다.
    /// </summary>
    private void HeroLevelUpgrade()
    {
       bool result = CurrencyManager.Instance.TrySpend(CurrencyType.Gold, new BigCurrency(1000, 0));

        if (result)
        {
            Debug.Log("[HeroLevelUpgrade] : 골드 소모");
        }
        else
            Debug.Log("[HeroLevelUpgrade] :골드 부족함");
    }

    /// <summary>
    /// 전투력을 계산하는 코드입니다.
    /// </summary>
    /// <returns></returns>
    private int CountingHeroPower()
    {
        int power = 0;
        return power;
    }
    #endregion

    #region Public

    public void HeroSOInfoSetting(CardInfo Info)
    {
        chardata = Info;
        Debug.Log($"SO {Info}로 세팅됨");
    }
    #endregion
}
