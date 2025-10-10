using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CurrencyDungeonPlayerSet : MonoBehaviour
{
    [SerializeField] CurrencyDungeonPoint[] allignPoints;

    [SerializeField] GameObject[] players;
    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;

    [SerializeField] CurrencyDungeonPlayerDataSO playerData;

    [SerializeField] GameObject card;

    private List<GameObject> cards;

    public void InitPlayer()
    {
        ActiveAllignPoint();
        CreateCards();
    }
    private void ActiveAllignPoint()
    {
        cards = new();
        CurrencyDungeonType type = sceneData.type;
        foreach (CurrencyDungeonPoint point in allignPoints)
        {
            if (type == point.Type)
            {
                point.gameObject.SetActive(true);
                Camera.main.transform.position = point.transform.position;
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].transform.position = point.points[i].transform.position;
                }
            }
        }
    }

    private void CreateCards()
    {
        foreach (CardInfo info in playerData.currentPlayerDataList)
        {
            PartyManager.Instance.AddMember(info);
        }
    }

    public void SpawnPlayer()
    {
        PartyManager.Instance.PartyInit();
    }
}
