using Firebase.Auth;
using Firebase.Database;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GachaManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private SummonResultUI resultUI;


    private Dictionary<string, List<string>> rarityHeroList = new();

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

    // 영웅 조각 반환 매칭용
    private int GetPieceAmountByRarity(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Normal: return 1;
            case HeroRarity.Rare: return 3;
            case HeroRarity.Epic: return 5;
            case HeroRarity.Unique: return 10;
            default: return 0;
        }
    }
    #region Unity LifeCycle

    private async void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        await LoadUserDataAsync();
        await LoadSummonConfigAsync();
        await LoadHeroListAsync();
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
        var snapProfile = await CurrencyManager.Instance.LoadUserProfileAsync();
        userSummonLevel = snapProfile.SummonLevel;
        userSummonCount = snapProfile.SummonCount;
    }
    private Task SaveUserSummonLevelAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("summonLevel").SetValueAsync((int)userSummonLevel);
    }

    private Task SaveUserSummonCountAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("summonCount").SetValueAsync(userSummonCount);
    }

    // 레어도에서 확률 나눠가지기
    private string DrawHeroByRarity(string rarity)
    {
        var list = rarityHeroList[rarity];
        int index = UnityEngine.Random.Range(0, list.Count);
        string selected = list[index];
        return selected;
    }


    // 레어도 풀 별 영웅 리스트 불러오기
    private async Task LoadHeroListAsync()
    {
        var snapshot = await _dbRef.Child("summon").Child("heroList").GetValueAsync();
        rarityHeroList.Clear();

        foreach (var rarity in new[] { "normal", "rare", "unique", "epic" })
        {
            var raritySnap = snapshot.Child(rarity);
            var heroList = new List<string>();

            foreach (var child in raritySnap.Children)
            {
                heroList.Add(child.Key);
            }
            rarityHeroList[rarity] = heroList;
        }
    }

    // 카드 정보 가져오기
    private async Task<CardInfo> LoadCardInfoByID(string heroID)
    {
        string addressKey = $"{heroID}CardInfo"; // 예: "C101CardInfo"
        var handle = Addressables.LoadAssetAsync<CardInfo>(addressKey);

        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        return null;
    }


    // 실제 소환 코드
    public async Task Summon(int times)
    {
        userSummonCount += times;
        if (userSummonCount >= requireSummonCount)
        {
            await LevelUpAsync();
        }

        var tasks = new List<Task<CardInfo>>();
        for (int i = 0; i < times; i++)
        {
            string rarityKey = DrawOne();
            string heroID = DrawHeroByRarity(rarityKey);
            tasks.Add(LoadCardInfoByID(heroID));
        }
        var results = (await Task.WhenAll(tasks)).ToList();

        var groupedResults = results.GroupBy(card => card.HeroID)
            .Select(group => new{
                HeroID = group.Key,
                Count = group.Count(),
                CardInfo = group.First()
            });
        foreach (var group in groupedResults)
        {
            await ProcessGachaResultAsync(group.CardInfo, group.Count);
        }
        await SaveUserSummonCountAsync();
        resultUI.ShowSummonResult(results);
        // await SaveGachaHistoryAsync(results); //소환기록 추가
    }

    //  소환레벨 업
    private async Task LevelUpAsync()
    {
        // 1~9까지만 처리
        if (userSummonLevel < SummonLevel.level10)
        {
            userSummonLevel = (SummonLevel)((int)userSummonLevel + 1);
            userSummonCount = 0;

            await SaveUserSummonLevelAsync();
            await LoadSummonConfigAsync();
        }
    }

    //  소환 확률 결정
    private string DrawOne()
    {
        var weightPool = GetWeightPool();
        float sum = weightPool.Values.Sum();
        float random = UnityEngine.Random.Range(0f, sum);

        foreach (var keyValue in weightPool)
        {
            if (random <= keyValue.Value) return keyValue.Key.ToLower();
            random -= keyValue.Value;
        }
        return weightPool.Keys.Last().ToLower();

    }

    //  소환기록 저장하기
    private async Task SaveGachaHistoryAsync(List<CardInfo> results)
    {
        var historyRef = _dbRef.Child("users").Child(_uid).Child("summonHistory");

        foreach (var card in results)
        {
            string key = historyRef.Push().Key;

            var data = new Dictionary<string, object>
        {
            { "timestamp", ServerValue.Timestamp },
            { "heroID", card.HeroID },

        };

            await historyRef.Child(key).SetValueAsync(data);
            // 기록 삭제
            // await historyRef.RemoveValueAsync();
        }
    }

    // 영웅정보를 업로드하는 코드
    private async Task ProcessGachaResultAsync(CardInfo card, int count)
    {
        var charRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(card.HeroID);
        var snapshot = await charRef.GetValueAsync();

        bool hasHero = snapshot.Child("hasHero").Exists && Convert.ToBoolean(snapshot.Child("hasHero").Value);
        int currentPiece = snapshot.Child("heroPiece").Exists ? Convert.ToInt32(snapshot.Child("heroPiece").Value) : 0;

        // 영웅 최초 획득 처리
        if (!hasHero)
        {
            var newHeroData = new Dictionary<string, object>
            {
                { "hasHero", true },
                { "heroPiece", 0 },
                { "stage", 1 },
                { "rarity", card.rarity.ToString() }
            };
            await charRef.UpdateChildrenAsync(newHeroData);
        }
        int addedPiece = GetPieceAmountByRarity(card.rarity) * count;
        int newPieceAmount = currentPiece + addedPiece;

        await charRef.Child("heroPiece").SetValueAsync(newPieceAmount);
        HeroDataManager.Instance?.UpdateHeroPiece(card.HeroID, newPieceAmount);
    }
    #endregion
}
/*
    TODO : 뽑기 해야할 일 목록
        영웅 뽑기 확률, 리스트 풀 수정하기
 */