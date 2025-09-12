using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class CurrencyDungeonClearUI : UIBase
{

    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;
    private CurrencyDungeonClearData clearData;

    public override void SetShow()
    {
        SetToFirebase();
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

        gameObject.SetActive(true);
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
