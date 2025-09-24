using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonHeroUI : UIBase
{
    #region SerializeField
    [Header("Buttons")]
    [SerializeField] private Button summonButton;           // 단챠
    [SerializeField] private Button summon10thButton;       // 10챠
    [SerializeField] private Button summon50thTimesButton;  // 50챠
    [SerializeField] private Button summonInfo;             // 확률정보
    [SerializeField] private Button summonResult;

    [Header("ButtonSet")]
    [SerializeField] private List<GachaButton> summonButtons;      // 버튼 스크립트   

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI summonLevelText;   // 소환레벨 텍스트
    [SerializeField] private TextMeshProUGUI button1Text;       // 단챠 버튼 텍스트
    [SerializeField] private TextMeshProUGUI button10Text;      // 10챠 버튼 텍스트
    [SerializeField] private TextMeshProUGUI button50Text;      // 50챠 버튼 텍스트

    [Header("Panel")]
    [SerializeField] private SummonResultUI summonResultUI;    // 소환 결과창
    [SerializeField] private GachaManager gachaManager;     // 가챠 메니저
    [SerializeField] private GachaRateInfoUI summonInfoPanel;    // 소환확률 정보창

    [Header("Slider")]
    [SerializeField] private Slider summonSlider;           // 소환래벨 슬라이더

    #endregion

    #region Properties
    private BigCurrency currency;
    private int summonCount;
    private int requireCount;
    private int inputTimes;
    private SummonLevel userSummonLevel;
    #endregion

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();

    }

    private void OnDisable()
    {
        summonButton.onClick.RemoveListener(OnClickSummon);
        summon10thButton.onClick.RemoveListener(OnClickSummon10th);
        summon50thTimesButton.onClick.RemoveListener(OnClickSummon50th);
        summonInfo.onClick.RemoveListener(OnClickShowInfo);
    }

    private void Init()
    {
        ButtonInit();
        SummonLevelChange();
    }

    private void ButtonInit()
    {
        summonInfo.onClick.AddListener(OnClickShowInfo);
        summonButton.onClick.AddListener(OnClickSummon);
        summon10thButton.onClick.AddListener(OnClickSummon10th);
        summon50thTimesButton.onClick.AddListener(OnClickSummon50th);
    }
    #endregion

    #region Button OnClick
    private void OnClickSummon()
    {
        inputTimes = 1;
        currency = BigCurrency.FromBaseAmount(inputTimes);
        if (!CurrencyManager.Instance.TrySpend(CurrencyType.SummonTicket, currency))
            return;
        SummonHeros(inputTimes);
        InterActButtons(false);
        QuestManager.Instance.ReportEvent(QuestTargetType.Gacha1, inputTimes);
    }
    private void OnClickSummon10th()
    {
        inputTimes = 10;
        currency = BigCurrency.FromBaseAmount(inputTimes);
        if (!CurrencyManager.Instance.TrySpend(CurrencyType.SummonTicket, currency))
            return;
        SummonHeros(inputTimes);
        InterActButtons(false);
        QuestManager.Instance.ReportEvent(QuestTargetType.Gacha1, inputTimes);
    }
    private void OnClickSummon50th()
    {
        inputTimes = 50;
        currency = BigCurrency.FromBaseAmount(inputTimes);
        if (!CurrencyManager.Instance.TrySpend(CurrencyType.SummonTicket, currency))
            return;

        SummonHeros(inputTimes);
        InterActButtons(false);
        QuestManager.Instance.ReportEvent(QuestTargetType.Gacha1, inputTimes);
    }
    private void OnClickShowInfo()
    {
        summonInfoPanel.gameObject.SetActive(true);
        summonInfoPanel.SetupCategory(1);
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
        var profile = await CurrencyManager.Instance.LoadUserProfileAsync();
        summonCount = profile.SummonCount;
        userSummonLevel = profile.SummonLevel;
        requireCount = await CurrencyManager.Instance.LoadRequireCountFromFireBase(userSummonLevel.ToString());

        summonLevelText.text = "영웅 뽑기 레벨 " + ((int)userSummonLevel).ToString();
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
    #endregion

    #region 
    public void HandleGachaCompleted()
    {
        SummonLevelChange();
        InterActButtons(true);
        foreach (var gachaButton in summonButtons)
        {
            if (gachaButton != null)
            {
                gachaButton.ButtonImageSetting(gachaButton.inputTimes);
            }
        }
    }
    #endregion
}

