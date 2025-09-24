using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GachaButton : MonoBehaviour
{
    [Header("SummmonCount")]
    [SerializeField] private int inputTimes;

    [Header("Image")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite Tickets;
    [SerializeField] private Sprite Jam;

    [Header("Button")]
    [SerializeField] private Button summonButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private CurrencyType currencyType;
    #region Unity
    private void OnEnable()
    {
        summonButton.interactable= true;
        ButtonImageSetting(inputTimes);
    }
    #endregion

    #region Private
    private void ButtonImageSetting(int times)
    {
        BigCurrency currency = BigCurrency.FromBaseAmount(times);
        // 만약에 보유중인 뽑기권의 개수가 인풋보다 크다면
        if (CurrencyManager.Instance.Model.Get(currencyType) > currency)
        {
            buttonImage.sprite = Tickets;
            buttonText.text = currency.ToString() + " 개";
        } // 인풋보다 보유중인 용옥이 충분하면
        else if (CurrencyManager.Instance.Model.Get(CurrencyType.SpiritStone) > (currency * 100))
        {
            buttonImage.sprite = Jam;
            buttonText.text = (currency*100).ToString() + " 개";
        }
        else  // 용옥도 모자라다면 작동 불가
        {
            buttonImage.sprite = Jam;
            summonButton.interactable = false;

            buttonText.text = (currency*100).ToString() + " 개";
        }
    }
    #endregion
}