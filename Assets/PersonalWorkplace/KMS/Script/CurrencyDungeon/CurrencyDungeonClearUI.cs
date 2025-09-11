using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class CurrencyDungeonClearUI : UIBase
{

    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;
    private CurrencyDungeonClearData clearData;

    public override void SetShow()
    {
        GiveCurrency();
        SetToFirebase();
    }

    private async void SetToFirebase()
    {
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
        BigCurrency cur = new BigCurrency(sceneData.data.Reward);
        switch (sceneData.type)
        {
            case CurrencyDungeonType.Gold: CurrencyManager.Instance.Add(CurrencyType.Gold, cur); break;
            case CurrencyDungeonType.Honbaeg: CurrencyManager.Instance.Add(CurrencyType.Soul, cur); break;
            case CurrencyDungeonType.Spirit: CurrencyManager.Instance.Add(CurrencyType.SpiritStone, cur); break;
        }
    }
}
