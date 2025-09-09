using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DungeonLevelCard : MonoBehaviour
{
    [SerializeField] GameObject locker;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text quantityText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] TMP_Text ButtonText;
    [SerializeField] Button startButton;
    [SerializeField] Image currencyImage;

    private CurrencyDungeonData data;

    private CurrencyDungeonType type;


    public void SetValues(CurrencyDungeonData data)
    {
        this.data = data;
        levelText.text = data.Name;
        BigCurrency reward = new BigCurrency(data.Reward);
        BigCurrency battle = new BigCurrency(data.BattlePower);
        quantityText.text = reward.ToString();
        attackText.text = battle.ToString();
    }
    public void SetType(CurrencyDungeonType type)
    {
        this.type = type;
    }
    public void SetSprite(Sprite spr)
    {
        currencyImage.sprite = spr;
    }
    public void SetStageCleared()
    {
        ButtonText.text = "소탕";
        locker.SetActive(false);
        startButton.onClick.AddListener(() => Debug.Log($"<color=yellow>{data.Name} 클리어 {data.Reward}개 획득</color>")); //{KMS_Util.DungeonTypeToName[type]}

    }
    public void SetStageAvailable()
    {
        ButtonText.text = "입장";
        locker.SetActive(false);
        startButton.onClick.AddListener(() => SceneManager.LoadSceneAsync("CurrencyDungeonScene"));

    }
    public void SetStageLocked()
    {
        ButtonText.text = "입장불가";
        locker.SetActive(true);
    }
    void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
    }
}
