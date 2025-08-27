using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class MSKDataManager : IStartable
{
    public string userId { get; private set; }
    public DatabaseReference userDataRef { get; private set; }

    private PartyManager partyManager;

    public MSKDataManager(PartyManager _partyManager)
    {
        partyManager = _partyManager;
    }

    public void Start() 
    {
        Init();
    }

    private void Init()
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        userDataRef = FirebaseDatabase.DefaultInstance.RootReference;
        partyManager.partySet += SavePartyData;
    }

    private void SavePartyData(Dictionary<string, CardInfo> partyInfo)
    {
        string json = JsonUtility.ToJson(partyInfo);
        userDataRef.Child("users").Child(userId).Child("Hero").Child("partyInfo").SetRawJsonValueAsync(json);
    }
}
