using System.Collections.Generic;
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


    // 데이터 베이스에서 불러올 값들 (몇단계까지 클리어 했는지)
    private int goldclear = 5;
    private int honbaegclear = 6;
    private int spiritclear = 12;


    public void Setting(CurrencyDungeonType type)
    {
        int clearVal = 0;
        int countVal = dict.DungeonCounts[type];
        switch (type)
        {
            case CurrencyDungeonType.Gold:
                clearVal = goldclear;
                break;
            case CurrencyDungeonType.Honbaeg:
                clearVal = honbaegclear;
                break;
            case CurrencyDungeonType.Spirit:
                clearVal = spiritclear;
                break;
        }
        for (int i = 0; i < allCards.Length; i++)
        {
            int j = i + 1;
            if (j <= countVal)
            {
                allCards[i].gameObject.SetActive(true);
                allCards[i].SetValues(dict.DungeonTables[type].Table[i]);
                allCards[i].SetSprite(dict.DungeonSprites[type]);
                allCards[i].SetType(type);
                if (j < clearVal)
                {
                    allCards[i].SetStageCleared();

                }
                else if (j == clearVal)
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
}
