using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LevelSelectPanel : MonoBehaviour
{
    [SerializeField] DungeonLevelCard[] allCards;

    [SerializeField] Scrollbar scrollbar;

    [SerializeField] CurrencyDungeonDict dict;

    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;

    [SerializeField] CurrencyDungeonPlayerDataSO playerData;

    [SerializeField] TMP_Text countText;
    [SerializeField] UnityEngine.UI.Image ticketImage;

    public CurrencyDungeonClearData ClearData { get; set; }
    private int ticketCount;


    public void Setting(CurrencyDungeonType type)
    {
        int clearVal = 0;
        int countVal = dict.DungeonCounts[type];
        ticketImage.sprite = dict.TicketSprites[type];
        switch (type)
        {
            case CurrencyDungeonType.Gold:
                ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.GoldChallengeTicket).Value;
                clearVal = ClearData.goldClearLevel;
                break;
            case CurrencyDungeonType.Honbaeg:
                ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value;
                clearVal = ClearData.HonbaegClearLevel;
                break;
            case CurrencyDungeonType.Spirit:
                ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value;
                clearVal = ClearData.SpiritClearLevel;
                break;
        }
        countText.text = $"{ticketCount} / 3";
        for (int i = 0; i < allCards.Length; i++)
        {
            int j = i + 1;
            if (j <= countVal)
            {
                allCards[i].gameObject.SetActive(true);
                allCards[i].SetValues(dict.DungeonTables[type].Table[i]);
                allCards[i].SetSprite(dict.DungeonSprites[type]);
                allCards[i].SetType(type);
                if (j <= clearVal)
                {
                    allCards[i].SetStageCleared(ClearedDungeon);

                }
                else if (j == clearVal + 1)
                {
                    allCards[i].SetStageAvailable(SetSceneData);
                }
                else
                {
                    allCards[i].SetStageLocked();
                }

            }
            else
            {
                allCards[i].gameObject.SetActive(false);
            }
        }
        scrollbar.value = (float)clearVal / countVal;
    }

    public void SetSceneData(CurrencyDungeonData data, CurrencyDungeonType type)
    {
        sceneData.data = data;
        sceneData.type = type;
        sceneData.clearData = this.ClearData;
        GetCurrentPlayerDatas();
    }

    public void GetCurrentPlayerDatas()
    {
        playerData.currentPlayerDataList.Clear();
        foreach (GameObject member in PartyManager.Instance.partyMembers)
        {
            HeroInfoSetting info = member.GetComponent<HeroInfoSetting>();
            string id = info.HeroID;
            CardInfo card = info.chardata;
            playerData.currentPlayerDataList.Add((id, card));
        }
    }

    public void ClearedDungeon(CurrencyDungeonData data, CurrencyDungeonType type)
    {
        if (ticketCount < 1)
        {
            Debug.Log("티켓 부족");
            return;
        }
        BigCurrency reward = new BigCurrency(data.Reward);
        ticketCount--;
        BigCurrency subtract = new BigCurrency(ticketCount);
        if (type == CurrencyDungeonType.Gold)
        {
            CurrencyManager.Instance.Add(CurrencyType.Gold, reward);
            CurrencyManager.Instance.Set(CurrencyType.GoldChallengeTicket, subtract);
        }
        else if (type == CurrencyDungeonType.Honbaeg)
        {
            CurrencyManager.Instance.Add(CurrencyType.Soul, reward);
            CurrencyManager.Instance.Set(CurrencyType.SoulChallengeTicket, subtract);
        }
        else if (type == CurrencyDungeonType.Spirit)
        {

            CurrencyManager.Instance.Add(CurrencyType.SpiritStone, reward);
            CurrencyManager.Instance.Set(CurrencyType.SpiritStoneChallengeTicket, subtract);

        }
        Debug.Log($"<color=yellow>{data.Name} 클리어 {data.Reward}개 획득</color>");
        countText.text = $"{ticketCount} / 3";
    }
}
