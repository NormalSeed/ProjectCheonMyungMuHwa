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

    public event Action OnGachaCompleted;

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

    // 카드 정보 표기
    private async Task<CardInfo> LoadCardInfoByRarity(string rarity)
    {
        string addressKey = rarity switch
        {
            "Normal" => "C001CardInfo",
            "Rare" => "C002CardInfo",
            "Unique" => "C003CardInfo",
            "Epic" => "C004CardInfo",
        };

        var handle = Addressables.LoadAssetAsync<CardInfo>(addressKey);

        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogError($"[{nameof(LoadCardInfoByRarity)}] {addressKey} 로드 실패");
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

        var results = new List<CardInfo>();
        for (int i = 0; i < times; i++)
        {
            string rarityKey = DrawOne();
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
    private async Task ProcessGachaResultAsync(CardInfo card)
    {
        var charRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(card.HeroID);

        // 보유 여부 확인
        var hasHeroSnap = await charRef.Child("hasHero").GetValueAsync();
        bool hasHero = hasHeroSnap.Exists && Convert.ToBoolean(hasHeroSnap.Value);

        if (!hasHero)
        {
            Debug.Log("영웅 최초 처리");
            // 캐릭터 최초 획득 시 정보 등록
            var newHeroData = new Dictionary<string, object>
            {
                { "hasHero", true },
                { "heroPiece", 0 }, // 조각은 0부터 시작
                { "stage", 1 },
                { "rarity", card.rarity.ToString() }
            };

            await charRef.UpdateChildrenAsync(newHeroData);
        }
        else
        {
            // 중복 획득: 조각 추가
            var pieceSnap = await charRef.Child("heroPiece").GetValueAsync();
            int currentPiece = pieceSnap.Exists ? Convert.ToInt32(pieceSnap.Value) : 0;
            int addedPiece = GetPieceAmountByRarity(card.rarity); // 레어도에 따라 조각 수 결정

            Debug.Log($"[조각 추가] {card.HeroID} → 기존: {currentPiece}, 추가: {addedPiece}, 최종: {currentPiece + addedPiece}");
           
            await charRef.Child("heroPiece").SetValueAsync(currentPiece + addedPiece);
        }
    }
    // 영웅 조각 정보를 업로드하는 코루틴
    private IEnumerator ProcessResultsCoroutine(List<CardInfo> results)
    {
        foreach (var info in results)
        {
            var task = ProcessGachaResultAsync(info);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                Debug.LogError($"ProcessGachaResultAsync 실패: {task.Exception}");
        }
        Debug.Log("모든 결과 처리 완료");
        OnGachaCompleted?.Invoke();
    }
    #endregion
}
