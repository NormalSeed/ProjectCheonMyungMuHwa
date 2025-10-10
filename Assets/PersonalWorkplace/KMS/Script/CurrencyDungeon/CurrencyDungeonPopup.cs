using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CurrencyDungeonPopup : MonoBehaviour
{
    public static CurrencyDungeonPopup Instance;
    [SerializeField] TMP_Text currencyText;

    [SerializeField] Button touch;
    public UnityEvent OnTouch => touch.onClick;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string str)
    {
        gameObject.SetActive(true);
        currencyText.text = str;
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
