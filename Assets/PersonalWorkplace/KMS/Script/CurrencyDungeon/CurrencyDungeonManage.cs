using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class CurrencyDungeonManage : MonoBehaviour
{
    [SerializeField] CurrencyDugeonBossSpawner bossSpawner;
    [SerializeField] CurrencyDungeonPlayerSet playerSet;
    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;
    private CurrencyDungeonClearData clearData;

    [SerializeField] CurrencyDungeonTimer timer;

    [SerializeField] UIBase failUI;
    [SerializeField] UIBase clearUI;


    void Awake()
    {
        bossSpawner.InitBoss(DungeonClear);
        playerSet.InitPlayer();

        timer.OnTimeOver += DungeonFail;
    }

    void Start()
    {
        bossSpawner.SpawnBoss();
        playerSet.SpawnPlayer();
    }

    private void DungeonClear()
    {
        timer.Stop();
        SetToFirebase();
        string cur = "";
        switch (sceneData.type)
        {
            case CurrencyDungeonType.Gold: cur = "금화"; break;
            case CurrencyDungeonType.Honbaeg: cur = "혼백"; break;
            case CurrencyDungeonType.Spirit: cur = "영석"; break;
        }
        CurrencyDungeonPopup.Instance.SetText($"{cur}  {sceneData.data.Reward}개");
        CurrencyDungeonPopup.Instance.OnTouch.AddListener(() =>
        {
            CurrencyDungeonPopup.Instance.Close();
            clearUI.SetShow();
            CurrencyDungeonPopup.Instance.OnTouch.RemoveAllListeners();
        });
    }

    private void DungeonFail()
    {
        bossSpawner.SpawnedBoss.SetActive(false);
        failUI.SetShow();
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
        GiveCurrency();
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
    private void GiveCurrency()
    {
        BigCurrency reward = new BigCurrency(sceneData.data.Reward);
        switch (sceneData.type)
        {
            case CurrencyDungeonType.Gold:
                CurrencyManager.Instance.Set(CurrencyType.GoldChallengeTicket,
                new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.GoldChallengeTicket).Value - 1));
                CurrencyManager.Instance.Add(CurrencyType.Gold, reward); break;
            case CurrencyDungeonType.Honbaeg:
                CurrencyManager.Instance.Set(CurrencyType.SoulChallengeTicket,
                new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.SoulChallengeTicket).Value - 1));
                CurrencyManager.Instance.Add(CurrencyType.Soul, reward); break;
            case CurrencyDungeonType.Spirit:
                CurrencyManager.Instance.Set(CurrencyType.SpiritStoneChallengeTicket,
                new BigCurrency(CurrencyManager.Instance.Get(CurrencyType.SpiritStoneChallengeTicket).Value - 1));
                CurrencyManager.Instance.Add(CurrencyType.SpiritStone, reward); break;
        }
    }

}
