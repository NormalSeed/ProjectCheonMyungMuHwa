using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdShopCard : MonoBehaviour
{
    [SerializeField] float amount;
    [SerializeField] Button btn;
    public UnityEvent OnClick => btn.onClick;

    public void GiveCurrencyAndCloseButton()
    {
        CurrencyManager.Instance.Add(CurrencyType.Jewel, new BigCurrency(amount));
        btn.gameObject.SetActive(false);
    }



}
