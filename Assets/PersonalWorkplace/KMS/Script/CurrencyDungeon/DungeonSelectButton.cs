using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DungeonSelectButton : MonoBehaviour
{
    [SerializeField] CurrencyDungeonType type;
    [SerializeField] Button button;

    [SerializeField] Button adButton;
    [SerializeField] AdDataSO adData;
    [SerializeField] TMP_Text countText;

    [SerializeField] AssetReferenceSprite background;

    [SerializeField] Image backgroundImage;

    public CurrencyDungeonType Type => type;
    public Button Button => button;

    void Awake()
    {
        LoadBackgroundAsync();

    }

    private async void LoadBackgroundAsync()
    {
        AsyncOperationHandle<Sprite> handle = background.LoadAssetAsync<Sprite>();
        Sprite spr = await handle.Task;
        backgroundImage.sprite = spr;
    }

    void OnEnable()
    {
        int ticketCount = 0;
        switch (type)
        {
            case CurrencyDungeonType.Gold: ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.GoldChallengeTicket).Value; break;
            case CurrencyDungeonType.Honbaeg: ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value; break;
            case CurrencyDungeonType.Spirit: ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value; break;
        }
        countText.text = $"{ticketCount} / 3";
        ActiveAdButton(ticketCount);
    }

    public void Register(UnityAction<CurrencyDungeonType> act)
    {
        button.onClick.AddListener(() => act.Invoke(type));
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
        adButton.onClick.RemoveAllListeners();
    }
    private void ActiveAdButton(int ticketCount)
    {
        adButton.onClick.RemoveAllListeners();
        if (ticketCount > 0)
        {
            adButton.gameObject.SetActive(false);
            return;
        }
        int amount = 3;
        adButton.gameObject.SetActive(true);
        adButton.onClick.AddListener(() => adData.ShowRewardAD(() =>
        {
            BigCurrency reward = new BigCurrency(amount);
            switch (type)
            {
                case CurrencyDungeonType.Gold: CurrencyManager.Instance.Set(CurrencyType.GoldChallengeTicket, reward); break;
                case CurrencyDungeonType.Honbaeg: CurrencyManager.Instance.Set(CurrencyType.SoulChallengeTicket, reward); break;
                case CurrencyDungeonType.Spirit: CurrencyManager.Instance.Set(CurrencyType.SpiritStoneChallengeTicket, reward); break;
            }
            countText.text = $"{amount} / 3";
            adButton.gameObject.SetActive(false);
        }));
    }
}
