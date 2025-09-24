using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaButton : MonoBehaviour
{
    [Header("SummmonCount")]
    [SerializeField] public int inputTimes;

    [Header("Image")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite ticketSprite;
    [SerializeField] private Sprite spiritStoneSprite;

    [Header("Button")]
    [SerializeField] private Button summonButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private CurrencyType currencyType;
    #region Unity
    private void OnEnable()
    {
        ButtonImageSetting(inputTimes);
    }
    #endregion

    #region public
    public void ButtonImageSetting(int times)
    {
        BigCurrency ticketCost = BigCurrency.FromBaseAmount(times);
        BigCurrency spiritStoneCost = BigCurrency.FromBaseAmount(times * 100);

        // 만약에 보유중인 뽑기권의 개수가 인풋보다 크다면

        var model = CurrencyManager.Instance.Model;

        // 이미지 설정
        // 상호작용 여부
        // 텍스트 설정

        // 만약에 보유중인 뽑기권의 개수가 인풋보다 크다면
        if (model.Get(currencyType) > ticketCost)
        {
            buttonImage.sprite = ticketSprite;
            summonButton.interactable = true;
            buttonText.text = $"{ticketCost} 개";
        }// 인풋보다 보유중인 용옥이 충분하면
        else if (model.Get(CurrencyType.SpiritStone) > spiritStoneCost)
        {
            buttonImage.sprite = spiritStoneSprite;
            summonButton.interactable = true;
            buttonText.text = $"{spiritStoneCost} 개";
        }
        else // 용옥도 모자라다면 작동 불가
        {
            buttonImage.sprite = spiritStoneSprite;
            summonButton.interactable = false;
            buttonText.text = $"<color=red>{spiritStoneCost} 개</color>";
        }
    }
    #endregion
}