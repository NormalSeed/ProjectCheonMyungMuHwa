using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DungeonSelectButton : MonoBehaviour
{
    [SerializeField] CurrencyDungeonType type;
    [SerializeField] Button button;
    [SerializeField] TMP_Text countText;

    public CurrencyDungeonType Type => type;
    public Button Button => button;

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
