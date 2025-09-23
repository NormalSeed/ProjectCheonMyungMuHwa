using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GachaButton : MonoBehaviour
{
    [Header("SummmonCount")]
    [SerializeField] private int inputTimes;

    [Header("Image")]
    [SerializeField] private Image Image;
    [SerializeField] private Sprite Tickets;
    [SerializeField] private Sprite Jam;

    [Header("Button")]
    [SerializeField] private Button Button;


    #region Unity
    private void OnEnable()
    {
        Button.interactable= true;
        ButtonImageSetting(inputTimes);
    }
    #endregion

    #region Private
    private void ButtonImageSetting(int times)
    {
        BigCurrency currency = BigCurrency.FromBaseAmount(times);
        // 만약에 보유중인 뽑기권의 개수가 인풋보다 크다면
        if (CurrencyManager.Instance.Model.Get(CurrencyType.InvitationTicket) > currency)
        {
            Image.sprite = Tickets;
        } // 인풋보다 보유중인 용옥이 충분하면
        else if (CurrencyManager.Instance.Model.Get(CurrencyType.SpiritStone) > (currency * 100))
        {
            Image.sprite = Jam;
        }
        else  // 용옥도 모자라다면 작동 불가
        {
            Image.sprite = Jam;
            Button.interactable = false;
        }
    }
    #endregion
}