using System.Threading.Tasks;
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
    [SerializeField] private Button summonResult;

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
    [SerializeField] private GachaManager gachaManager;     // 가챠 메니저
    [SerializeField] private GameObject summonInfoPanel;    // 소환확률 정보창

    [Header("Slider")]
    [SerializeField] private Slider summonSlider;           // 소환래벨 슬라이더

    #endregion

    #region Properties
    private int summonCount;
    private int requireCount;
    private SummonLevel userSummonLevel;
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

        gachaManager.OnGachaCompleted -= HandleGachaCompleted;
    }

    private void Init()
    {
        ButtonInit();
        SummonLevelChange();
        gachaManager.OnGachaCompleted += HandleGachaCompleted;
    }

    private void ButtonInit()
    {
        summonInfo.onClick.AddListener(OnClickShowInfo);
        summonButton.onClick.AddListener(onClickSummon);
        summon10thButton.onClick.AddListener(onClickSummon10th);
        summon50thTimesButton.onClick.AddListener(onClickSummon50th);
    }
    #endregion

    #region Button OnClick
    private void onClickSummon()
    {
        SummonHeros(1);
        InterActButtons(false);
    }
    private void onClickSummon10th()
    {
        SummonHeros(10);
        InterActButtons(false);
    }
    private void onClickSummon50th()
    {
        SummonHeros(50);
        InterActButtons(false);
    }
    private void OnClickShowInfo()
    {
        summonInfoPanel.SetActive(true);
    }
    #endregion


    #region private

    private void InterActButtons(bool input)
    {
        summonButton.interactable = input;
        summon10thButton.interactable = input;
        summon50thTimesButton.interactable = input;
        summonInfo.interactable = input;
        summonResult.interactable = input;
    }
    private async Task SummonLevelChange()
    {
        summonCount = await CurrencyManager.Instance.LoadSummonCountFromFireBase();
        int levelValue = await CurrencyManager.Instance.LoadSummonLevelFromFireBase();
        userSummonLevel = (SummonLevel)levelValue;
        requireCount = await CurrencyManager.Instance.LoadRequireCountFromFireBase(userSummonLevel.ToString());

        summonLevelText.text = "영웅 뽑기 레벨 " + levelValue.ToString();
        UpdateSummonSlider();
    }
    private void UpdateSummonSlider()
    {
        summonSlider.maxValue = requireCount;
        summonSlider.value = summonCount;
    }
    private async Task SummonHeros(int times)
    {
        summonResultUI.gameObject.SetActive(true);
        await gachaManager.Summon(times);
    }

    private void HandleGachaCompleted()
    {
        SummonLevelChange();
        InterActButtons(true);
    }

    private void ChangeButtonText()
    {
        /*   TODO : 가진 재화를 확인하여 소환 타입을 설정하기   */
    }
    #endregion
}

