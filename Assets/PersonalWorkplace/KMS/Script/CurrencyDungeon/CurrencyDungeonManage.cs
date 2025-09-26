using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;

public class CurrencyDungeonManage : MonoBehaviour
{
    [SerializeField] CurrencyDugeonBossSpawner bossSpawner;
    [SerializeField] CurrencyDungeonPlayerSet playerSet;
    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;
    private CurrencyDungeonClearData clearData;

    [SerializeField] CurrencyDungeonTimer timer => barUI.Timer;

    [SerializeField] StageBarUI barUI;


    void Awake()
    {
        bossSpawner.InitBoss(DungeonClear);
        playerSet.InitPlayer();

        timer.OnTimeOver += DungeonFail;
        playerSet.SpawnPlayer();
    }

    void Start()
    {
        barUI.SetCurrencyDungeon(sceneData.type);
        bossSpawner.SpawnBoss();
        
    }

    private void DungeonClear()
    {
        timer.Stop();
        SetToFirebase();
        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        PopupManager.Instance.ShowStageClearPopup();
        BigCurrency reward = new BigCurrency(sceneData.data.Reward);
        ItemData rewardItem = null;
        sceneData.MainUiToOpen = UIType.Dungeon;
        if (sceneData.type == CurrencyDungeonType.Gold)
        {
            rewardItem = new ItemData(11002, "", "", "GoldImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.GoldChallengeTicket,
            new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.GoldChallengeTicket).Value - 1));
        }
        else if (sceneData.type == CurrencyDungeonType.Honbaeg)
        {
            rewardItem = new ItemData(11003, "", "", "SoulImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.SoulChallengeTicket,
            new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value - 1));
        }
        else if (sceneData.type == CurrencyDungeonType.Spirit)
        {
            rewardItem = new ItemData(11004, "", "", "SpiritImage", true, ItemType.Currency);
            CurrencyManager.Instance.Set(CurrencyType.SpiritStoneChallengeTicket,
            new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value - 1));
        }
        yield return new WaitForSeconds(4f);
        PopupManager.Instance.ShowRewardPopup(new List<ItemData>() { rewardItem }, new List<BigCurrency>() { reward });
        yield return new WaitForSeconds(3f);
        FadeCanvas.Instance.FadeOutAndLoadScene("DEMO_GameScene", 1.5f);


    }

    private void DungeonFail()
    {
        bossSpawner.Bosscon.gameObject.SetActive(false);
        DungeonFailUI ui = PopupManager.Instance.ShowDungeonFailPopup();
        ui.SetActionToButton(0, () =>
        {
            sceneData.MainUiToOpen = UIType.Hero;
            ui.SetHide();
            ui.LoadScene("Demo_GameScene");
        });
        ui.SetActionToButton(1, () =>
        {
            sceneData.MainUiToOpen = UIType.Dungeon;
            ui.SetHide();
            ui.LoadScene("Demo_GameScene");
        });
        ui.SetActionToButton(2, () =>
        {
            sceneData.MainUiToOpen = UIType.Upgrade;
            ui.SetHide();
            ui.LoadScene("Demo_GameScene");
        });
        ui.SetActionToButton(3, () =>
        {
            sceneData.MainUiToOpen = UIType.Summon;
            ui.SetHide();
            ui.LoadScene("Demo_GameScene");
        });
        ui.SetActionToScreen(() =>
        {
            sceneData.MainUiToOpen = UIType.Dungeon;
            ui.SetHide();
            ui.LoadScene("Demo_GameScene");
        });
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            DungeonClear();
        }

    }
    private async void SetToFirebase()
    {
        //GiveCurrency();
        string json;
        string _uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        clearData = sceneData.clearData;
        DatabaseReference _dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(_uid).Child("currencyDungeon");

        switch (sceneData.type)
        {
            case CurrencyDungeonType.Gold: clearData.goldClearLevel++; break;
            case CurrencyDungeonType.Honbaeg: clearData.HonbaegClearLevel++; break;
            case CurrencyDungeonType.Spirit: clearData.SpiritClearLevel++; break;
        }

        json = JsonUtility.ToJson(clearData);
        await _dbRef.SetRawJsonValueAsync(json);
    }
}
