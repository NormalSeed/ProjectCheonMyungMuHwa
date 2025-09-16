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
        switch (type)
        {
            case CurrencyDungeonType.Gold: countText.text = $"{(int)CurrencyManager.Instance.Get(CurrencyType.GoldChallengeTicket).Value} / 3"; break;
            case CurrencyDungeonType.Honbaeg: countText.text = $"{(int)CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value} / 3"; break;
            case CurrencyDungeonType.Spirit: countText.text = $"{(int)CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value} / 3"; break;
        }
    }

    public void Register(UnityAction<CurrencyDungeonType> act)
    {
        button.onClick.AddListener(() => act.Invoke(type));
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
