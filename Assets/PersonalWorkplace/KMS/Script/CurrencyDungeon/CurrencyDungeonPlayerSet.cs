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
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].transform.position = point.points[i].transform.position;
                }
            }
        }
    }

    private void CreateCards()
    {
        foreach ((string id, CardInfo info) data in playerData.currentPlayerDataList)
        {
            GameObject go = Instantiate(card);
            HeroInfoSetting infoSetting = go.GetComponent<HeroInfoSetting>();
            infoSetting.HeroID = data.id;
            infoSetting.chardata = data.info;
            cards.Add(go);
        }
    }

    public void SpawnPlayer()
    {
        foreach (GameObject card in cards)
        {
            PartyManager.Instance.AddMember(card.GetComponent<HeroInfoSetting>().chardata);
            // 기존 코드             PartyManager.Instance.AddMember(card);
        }
    }
}
