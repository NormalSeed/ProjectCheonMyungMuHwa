using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonUI : UIBase
{
    #region SerializeField
    [Header("Buttons")]
    [SerializeField] private Button summonButton;           // 단챠
    [SerializeField] private Button summon10thButton;       // 10챠
    [SerializeField] private Button summon50thTimesButton;  // 50챠
    [SerializeField] private Button summonInfo;             // 확률정보

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI summonLevelText;   // 소환레벨 텍스트
    [SerializeField] private TextMeshProUGUI button1Text;       // 단챠 버튼 텍스트
    [SerializeField] private TextMeshProUGUI button10Text;      // 10챠 버튼 텍스트
    [SerializeField] private TextMeshProUGUI button50Text;      // 50챠 버튼 텍스트

    [Header("Hero Image")]
    [SerializeField] private Transform Hero1;               // 영웅 이미지
    [SerializeField] private Transform Hero2;               // 영웅 이미지
    [SerializeField] private Transform Hero3;               // 영웅 이미지
    [SerializeField] private Transform Hero4;               // 영웅 이미지

    [Header("Panel")]
    [SerializeField] private SummonResultUI summonResultUI;    // 소환 결과창
    [SerializeField] private GameObject summonInfoPanel;    // 소환확률 정보창


    #endregion

    #region Properties

    #endregion

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        summonButton.onClick.RemoveListener(onClickSummon);
        summon10thButton.onClick.RemoveListener(onClickSummon10th);
        summon50thTimesButton.onClick.RemoveListener(onClickSummon50th);
        summonInfo.onClick.RemoveListener(OnClickShowInfo);
    }

    private void Init()
    {
        summonButton.onClick.AddListener(onClickSummon);
        summon10thButton.onClick.AddListener(onClickSummon10th);
        summon50thTimesButton.onClick.AddListener(onClickSummon50th);
        summonInfo.onClick.AddListener(OnClickShowInfo);
    }
    #endregion

    #region Button OnClick
    private void onClickSummon()
    {
        SummonHeros(1);
    }
    private void onClickSummon10th()
    {
        SummonHeros(10);
    }
    private void onClickSummon50th()
    {
        SummonHeros(50);
    }
    private void OnClickShowInfo()
    {
        summonInfoPanel.SetActive(true);
    }
    #endregion

    
    #region private
    private void SummonHeros(int times)
    {
        summonResultUI.gameObject.SetActive(true);
        summonResultUI.ShowSummonResult(times);
    }

    private void ChangeButtonText(TextMeshProUGUI text)
    {
        /*   TODO : 가진 재화를 확인하여 소환 타입을 설정하기   */
    }
    #endregion
}

