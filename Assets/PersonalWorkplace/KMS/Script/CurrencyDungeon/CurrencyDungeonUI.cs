using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public enum CurrencyDungeonType { Gold, Honbaeg, Spirit }

public class CurrencyDungeonUI : UIBase
{
    [SerializeField] DungeonSelectPanel dungeonPanel;
    [SerializeField] LevelSelectPanel levelPanel;

    [SerializeField] CurrencyDungeonSceneLoadDataSO data;
    CurrencyDungeonClearData clearData;

    public override void SetShow()
    {
        Task task = LoadFromFirebase();
    }

    public void OpenDungeonPanel()
    {
        levelPanel.gameObject.SetActive(false);
        dungeonPanel.gameObject.SetActive(true);
        dungeonPanel.RegisteButtons(OpenLevelPanel);

    }
    public void OpenLevelPanel(CurrencyDungeonType type)
    {
        levelPanel.gameObject.SetActive(true);
        dungeonPanel.gameObject.SetActive(false);
        levelPanel.ClearData = this.clearData;
        levelPanel.Setting(type);
    }

    public async Task LoadFromFirebase()
    {
        string json;
        string _uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference _dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(_uid).Child("currencyDungeon");
        var snapshot = await _dbRef.GetValueAsync();
        if (!snapshot.Exists)
        {
            clearData = new CurrencyDungeonClearData() { goldClearLevel = 0, HonbaegClearLevel = 0, SpiritClearLevel = 0 };
            json = JsonUtility.ToJson(clearData);
            await _dbRef.SetRawJsonValueAsync(json);
        }
        json = snapshot.GetRawJsonValue();
        clearData = JsonUtility.FromJson<CurrencyDungeonClearData>(json);
        gameObject.SetActive(true);
        if (data.BackToMain)
        {
            OpenLevelPanel(data.type);
            data.BackToMain = false;
        }
        else
        {
            OpenDungeonPanel();
        }
    }
}

public struct CurrencyDungeonClearData
{
    public int goldClearLevel;
    public int HonbaegClearLevel;
    public int SpiritClearLevel;
}
