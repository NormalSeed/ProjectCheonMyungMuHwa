using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoUI : UIBase
{
    [Header("Hero Info SO")]
    [SerializeField] private CardInfo chardata;  // 캐릭터 정보 SO
 // [SerializeField] private          SkillInfo; // 스킬 정보 SO

    [Header("Button")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button stageUPButton;

    [Header("UI Object")]
    [SerializeField] private TextMeshProUGUI name;      // 이름
    [SerializeField] private TextMeshProUGUI power;     // 전투력
    [SerializeField] private TextMeshProUGUI outPow;    // 외공
    [SerializeField] private TextMeshProUGUI inPow;     // 내공
    [SerializeField] private TextMeshProUGUI health;    // 체력

    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot; // 레어도
    [SerializeField] private Transform characterRoot;      // 이미지
    [SerializeField] private Transform stageRoot;          // 돌파
    [SerializeField] private Transform badgeRoot;          // 팩션,소속
    [SerializeField] private Transform SkillRoot;          // 스킬정보
    
    [Header("Panel")]
    [SerializeField] private GameObject heroInfoPanel;

    private string heroID;              // 캐릭터 ID
    private int heroStage;              // 돌파정보
    private HeroRarity rarity;          // 레어도
    private HeroRelationship faction;  // 진영

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
    private void Init()
    {  
        /*
          heroID = chardata.HeroID;
          heroStage = chardata.HeroStage;
          rarity = chardata.rarity;
          faction = chardata.relationship;
          
          ... SO에서 정보 받아오기
        */

        exitButton.onClick.AddListener(OnClickExit);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        stageUPButton.onClick.AddListener(OnClickStageUP);
    }
    #region OnClick
    private void OnClickExit()
    {
        SetHide();
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
}
