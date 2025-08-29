using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private SummonResultUI resultUI;



    #region FireBase Properties
    private string _uid;               
    private DatabaseReference _dbRef;  

    private float summonNormal;             // 노말 소환확률           
    private float summonRare;               // 레어 소환확률
    private float summonUnique;             // 유니트 소환확률
    private float summonEpic;               // 에픽 소환확률
    private SummonLevel userSummonLevel;    // 소환 레벨
    private int userSummonCount;            // 유저의 소환 레벨업 수치 
    private int requireSummonCount;         // 소환 레벨업 요구량
    #endregion

    // 가중치 풀
    private Dictionary<string, float> GetWeightPool()
    {
        return new Dictionary<string, float>
        {
            { "Normal", summonNormal },
            { "Rare", summonRare },
            { "Unique", summonUnique },
            { "Epic", summonEpic }
        };
    }

    #region Unity LifeCycle

    private  async void OnEnable()
    {
        _uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        await LoadUserDataAsync();
        await LoadSummonConfigAsync();
    }
    private void OnDisable() { }
    #endregion

    #region Private

    //  소환 레벨 별 확률정보 로딩
    private async Task LoadSummonConfigAsync()
    {
        var snapshot = await _dbRef.Child("summon").Child(userSummonLevel.ToString()).GetValueAsync();

        summonNormal = Convert.ToSingle(snapshot.Child("normal").Value);
        summonRare = Convert.ToSingle(snapshot.Child("rare").Value);
        summonUnique = Convert.ToSingle(snapshot.Child("unique").Value);
        summonEpic = Convert.ToSingle(snapshot.Child("epic").Value);
        requireSummonCount = Convert.ToInt32(snapshot.Child("count").Value);
    }
    // 유저 뽑기정보 로딩
    private async Task LoadUserDataAsync()
    {
        var snapLevel = await _dbRef.Child("users").Child(_uid).Child("profile").Child("summonLevel").GetValueAsync();
        var snapCount = await _dbRef.Child("users").Child(_uid).Child("profile").Child("summonCount").GetValueAsync();
        int levelInt = snapLevel.Exists ? Convert.ToInt32(snapLevel.Value) : (int)SummonLevel.level01;
        //  가져오고 없을 경우에 설정하기
        userSummonLevel = (SummonLevel)levelInt;
        userSummonCount = snapCount.Exists ? Convert.ToInt32(snapCount.Value) : 0;
    }
    private Task SaveUserSummonLevelAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("summonLevel").SetValueAsync((int)userSummonLevel);
    }

    private Task SaveUserSummonCountAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("summonCount").SetValueAsync(userSummonCount);
    }

    private async Task<CardInfo> LoadCardInfoByRarity(string rarity)
    {
        string addressKey = rarity switch
        {
            "Normal" => "C001CardInfo",
            "Rare" => "C002CardInfo",
            "Unique" => "C003CardInfo",
            "Epic" => "C004CardInfo",
        };

        var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<CardInfo>(addressKey);

        await handle.Task;
        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogError($"[{nameof(LoadCardInfoByRarity)}] {addressKey} 로드 실패");
        return null;
    }
    public async void Summon(int times)
    {
        if (userSummonCount >= requireSummonCount)
        {
            if (userSummonLevel < SummonLevel.level10)
                userSummonLevel = (SummonLevel)((int)userSummonLevel + 1);
            userSummonCount = 0;
            await SaveUserSummonLevelAsync();
            await LoadSummonConfigAsync();
        }

        var results = new List<CardInfo>();
        for (int i = 0; i < times; i++)
        {
            string rarityKey = DrawOne();
            CardInfo info = await LoadCardInfoByRarity(rarityKey);
            results.Add(info);
            userSummonCount++;

            if (userSummonCount >= requireSummonCount)
            {
                await LevelUpAsync();
            }
        }

        await _dbRef.Child("users").Child(_uid).Child("profile").Child("summonCount").SetValueAsync(userSummonCount);
        resultUI.ShowSummonResult(results);
    }

    private async Task LevelUpAsync()
    {
        if (userSummonLevel < SummonLevel.level10)
        {
            userSummonLevel = (SummonLevel)((int)userSummonLevel + 1);
            userSummonCount = 0;

            await SaveUserSummonLevelAsync();
            await LoadSummonConfigAsync();
        }
    }

    private string DrawOne()
    {
        var weightPool = GetWeightPool();
        float sum = weightPool.Values.Sum();
        float random = UnityEngine.Random.Range(0f, sum);

        foreach (var keyValue in weightPool)
        {
            if (random <= keyValue.Value) return keyValue.Key;
            random -= keyValue.Value;
        }
        return weightPool.Keys.Last();
    }
    #endregion
}
