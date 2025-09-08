using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class DungeonSelectButton : MonoBehaviour
{
    [SerializeField] CurrencyDungeonType type;
    [SerializeField] Button button;

    public CurrencyDungeonType Type => type;
    public Button Button => button;

    public void Register(UnityAction<CurrencyDungeonType> act)
    {
        button.onClick.AddListener(() => act.Invoke(type));
    }

    void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
