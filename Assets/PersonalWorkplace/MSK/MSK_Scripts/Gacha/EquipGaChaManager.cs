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
using VContainer;

public class EquipGachaManager : MonoBehaviour
{

    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

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

    private Dictionary<string, List<string>> rarityEquipMap = new(); // 레어도 별 리스트
    #endregion
    
    private Dictionary<string, EquipmentInstance> InfoCache = new(); // 캐싱용 딕셔너리

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

        // 장비 리스트 로딩
        var ListSnap = summonSnap.Child("equipment");
        rarityEquipMap.Clear();

        foreach (var rarityNode in ListSnap.Children)
        {
            rarityEquipMap[rarityNode.Key.ToLower()] = rarityNode.Children
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
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("equipsummonLevel").SetValueAsync((int)userSummonLevel);
    }

    private Task SaveUserSummonCountAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("equipsummonCount").SetValueAsync(userSummonCount);
    }

    // 카드 정보 표기
    private EquipmentInstance LoadEquipmentByCode(string templateID, RarityType rarity)
    {
        if (string.IsNullOrEmpty(templateID))
        {
            Debug.LogWarning("템플릿 ID가 비어있습니다.");
            return null;
        }

        // 캐싱된 인스턴스가 있다면 반환
        if (InfoCache.TryGetValue(templateID, out var cached))
            return cached;

        int level = 1;
        var equipment = equipmentService.AcquireEquipment(templateID, rarity, level);

        if (equipment != null)
        {
            InfoCache[templateID] = equipment;
            return equipment;
        }

        Debug.LogError($"장비 생성 실패: {templateID}");
        return null;
    }

    private string GetRandomTemplateIDByRarity(string rarityKey)
    {
        if (!rarityEquipMap.TryGetValue(rarityKey.ToLower(), out var templateList) || templateList.Count == 0)
        {
            Debug.LogWarning($"해당 레어도({rarityKey})에 템플릿 없음");
            return "";
        }

        int index = UnityEngine.Random.Range(0, templateList.Count);
        return templateList[index];
    }


    // 실제 소환 코드
    public async Task Summon(int times)
    {
        userSummonCount += times;
        if (userSummonCount >= requireSummonCount)
        {
            await LevelUpAsync();
        }

        var results = new List<EquipmentInstance>();
        for (int i = 0; i < times; i++)
        {
            string rarityKey = DrawRarity();
            RarityType rarity = Enum.Parse<RarityType>(rarityKey, true);
            string templateID = GetRandomTemplateIDByRarity(rarityKey);

            var equipment = LoadEquipmentByCode(templateID, rarity);
            if (equipment != null)
            {
                results.Add(equipment);
            }
        }

        StartCoroutine(ProcessEquipmentResultsCoroutine(results));
        await SaveUserSummonCountAsync();
        resultUI.ShowSummonResult(results);
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
    private async Task SaveEquipmentGachaHistoryAsync(List<EquipmentInstance> results)
    {
        var historyRef = _dbRef.Child("users").Child(_uid).Child("equipmentSummonHistory");

        foreach (var equip in results)
        {
            string key = historyRef.Push().Key;

            var data = new Dictionary<string, object>
        {
            { "timestamp", ServerValue.Timestamp },
            { "templateID", equip.templateID },
            { "rarity", equip.rarity.ToString() },
            { "level", equip.level }
        };

            await historyRef.Child(key).SetValueAsync(data);
        }
    }


    // 장비 정보 갱신하는 코루틴
    private IEnumerator ProcessEquipmentResultsCoroutine(List<EquipmentInstance> results)
    {
        foreach (var equip in results)
        {
            Debug.Log($"획득 장비: {equip.templateID} / {equip.rarity} / Lv.{equip.level}");
        }

        equipmentManager.SaveToJson(); // 저장 처리
        yield return null;
    }
    #endregion
}