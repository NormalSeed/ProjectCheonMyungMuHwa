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

public class EquipGachaManager : MonoBehaviour
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

    private Dictionary<string, List<string>> rarityHeroMap = new(); // 레어도 별 영웅 리스트
    #endregion
    
    private Dictionary<string, CardInfo> cardInfoCache = new(); // 캐싱용 딕셔너리
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

    private async void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        await LoadUserDataAsync();
        await LoadSummonConfigAsync();
    }
    private void OnDisable() { }
    #endregion

    #region Private

    //  소환 레벨 별 확률정보 로딩
    private async Task LoadSummonConfigAsync()
    {
        var summonSnap = await _dbRef.Child("summon").GetValueAsync();

        // 확률 및 요구 수치 로딩
        var configSnap = summonSnap.Child(userSummonLevel.ToString());
        summonNormal = Convert.ToSingle(configSnap.Child("normal").Value);
        summonRare = Convert.ToSingle(configSnap.Child("rare").Value);
        summonUnique = Convert.ToSingle(configSnap.Child("unique").Value);
        summonEpic = Convert.ToSingle(configSnap.Child("epic").Value);
        requireSummonCount = Convert.ToInt32(configSnap.Child("count").Value);

        // 영웅 리스트 로딩
        var heroListSnap = summonSnap.Child("heroList");
        rarityHeroMap.Clear();

        foreach (var rarityNode in heroListSnap.Children)
        {
            rarityHeroMap[rarityNode.Key.ToLower()] = rarityNode.Children
                .Select(hero => hero.Key)
                .ToList();
        }
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

    // 카드 정보 표기
    private async Task<CardInfo> LoadCardInfoByCode(string heroCode)
    {
        if (cardInfoCache.TryGetValue(heroCode, out var cached))
            return cached;

        string addressKey = $"{heroCode}CardInfo";
        var handle = Addressables.LoadAssetAsync<CardInfo>(addressKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            cardInfoCache[heroCode] = handle.Result;
            return handle.Result;
        }

        Debug.LogError($"[{nameof(LoadCardInfoByCode)}] {addressKey} 로드 실패");
        return null;
    }

    private async Task<CardInfo> LoadCardInfoByRarity(string rarity)
    {
        string key = rarity.ToLower();
        if (!rarityHeroMap.TryGetValue(key, out var heroList) || heroList.Count == 0)
        {
            Debug.LogError($"[{nameof(LoadCardInfoByRarity)}] {rarity}에 해당하는 영웅 리스트 없음");
            return null;
        }

        string selectedCode = heroList[UnityEngine.Random.Range(0, heroList.Count)];
        return await LoadCardInfoByCode(selectedCode);
    }


    // 실제 소환 코드
    public async Task Summon(int times)
    {
        userSummonCount += times;
        if (userSummonCount >= requireSummonCount)
        {
            await LevelUpAsync();
        }

        var results = new List<CardInfo>();
        for (int i = 0; i < times; i++)
        {
            string rarityKey = DrawRarity();
            CardInfo info = await LoadCardInfoByRarity(rarityKey);
            results.Add(info);
        }

        StartCoroutine(ProcessResultsCoroutine(results));
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
    private string DrawRarity()
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
    // 영웅 조각 정보를 업로드하는 코루틴
    private IEnumerator ProcessResultsCoroutine(List<CardInfo> results)
    {
        // HeroDataManager에 결과 반영
        HeroDataManager.Instance.ApplyGachaResults(results);

        // 한번에 저장
        HeroDataManager.Instance.SaveHeroDataToCache();
        HeroDataManager.Instance.SaveAllHeroDataToFirebase();

        yield return null;
        Debug.Log("영웅 뽑기 결과 처리 완료"); 
    }
    #endregion
}
/*
    TODO : 뽑기 해야할 일 목록
        영웅 뽑기 확률, 리스트 풀 수정하기
 */