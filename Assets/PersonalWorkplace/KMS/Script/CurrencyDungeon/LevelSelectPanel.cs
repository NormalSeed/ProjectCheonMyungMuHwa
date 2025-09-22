using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectPanel : MonoBehaviour
{
    [SerializeField] DungeonLevelCard[] allCards;

    [SerializeField] Scrollbar scrollbar;

    [SerializeField] CurrencyDungeonDict dict;

    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;

    [SerializeField] CurrencyDungeonPlayerDataSO playerData;

    [SerializeField] TMP_Text countText;

    [SerializeField] TMP_Text titleText;
    [SerializeField] Image ticketImage;

    [SerializeField] RectTransform viewPort;
    [SerializeField] RectTransform contents;

    public CurrencyDungeonClearData ClearData { get; set; }
    private int ticketCount;

    private string currency;

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
                titleText.text = "금화던전";
                currency = "금화";
                break;
            case CurrencyDungeonType.Honbaeg:
                ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value;
                clearVal = ClearData.HonbaegClearLevel;
                titleText.text = "혼백던전";
                currency = "혼백";
                break;
            case CurrencyDungeonType.Spirit:
                ticketCount = (int)CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value;
                clearVal = ClearData.SpiritClearLevel;
                titleText.text = "영석던전";
                currency = "영석";
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
        StartCoroutine(Align(clearVal));
    }
    public IEnumerator Align(int clearVal)
    {
        yield return null;
        float canvWidth = viewPort.rect.width;
        float contWidth = contents.rect.width;
        float wDelta = contWidth - canvWidth;
        float canvCenter = canvWidth / 2;
        float targetPos = allCards[clearVal].transform.localPosition.x;
        float distance = targetPos - canvCenter;
        float scrollValue = Mathf.Clamp(distance / wDelta, 0, 1);
        scrollbar.value = scrollValue;
    }

    private void SetSceneData(CurrencyDungeonData data, CurrencyDungeonType type)
    {
        if (ticketCount < 1)
        {
            Debug.Log("티켓 부족");
            return;
        }
        sceneData.data = data;
        sceneData.type = type;
        sceneData.clearData = this.ClearData;
        GetCurrentPlayerDatas();
    }

    private void GetCurrentPlayerDatas()
    {
        /*  기존 코드입니다. 
           playerData.currentPlayerDataList.Clear();
           foreach (GameObject member in PartyManager.Instance.partyMembers)
           {
               HeroInfoSetting info = member.GetComponent<HeroInfoSetting>();
               string id = info.HeroID;
               CardInfo card = info.chardata;
               playerData.currentPlayerDataList.Add((id, card));
           }
        */

        playerData.currentPlayerDataList.Clear();
        foreach (CardInfo member in PartyManager.Instance.MembersID)
        {
            //string id = member.HeroID;
            playerData.currentPlayerDataList.Add(member);
        }
        SceneManager.LoadSceneAsync("CurrencyDungeonScene");
    }

    private void ClearedDungeon(CurrencyDungeonData data, CurrencyDungeonType type)
    {
        if (ticketCount < 1)
        {
            Debug.Log("티켓 부족");
            return;
        }
        BigCurrency reward = new BigCurrency(data.Reward);
        ticketCount--;
        BigCurrency subtract = new BigCurrency(ticketCount);
        ItemData rewardItem = null;
        if (type == CurrencyDungeonType.Gold)
        {
            rewardItem = new ItemData(11002, "", "", "GoldImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.GoldChallengeTicket, subtract);
        }
        else if (type == CurrencyDungeonType.Honbaeg)
        {
            rewardItem = new ItemData(11003, "", "", "SoulImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.SoulChallengeTicket, subtract);
        }
        else if (type == CurrencyDungeonType.Spirit)
        {
            rewardItem = new ItemData(11004, "", "", "SpiritImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.SpiritStoneChallengeTicket, subtract);

        }
        PopupManager.Instance.ShowRewardPopup(new List<ItemData>() {rewardItem}, new List<BigCurrency>() {reward});
        countText.text = $"{ticketCount} / 3";
        //CurrencyDungeonPopup.Instance.SetText($"{currency}  {data.Reward}개");
        //CurrencyDungeonPopup.Instance.OnTouch.AddListener(() =>
        //{
        //    CurrencyDungeonPopup.Instance.Close();
        //    CurrencyDungeonPopup.Instance.OnTouch.RemoveAllListeners();
        //});
    }
}
