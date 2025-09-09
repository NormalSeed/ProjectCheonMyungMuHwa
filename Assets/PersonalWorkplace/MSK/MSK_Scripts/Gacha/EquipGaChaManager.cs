using Firebase.Auth;
using Firebase.Database;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

public class EquipGachaManager : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("UI")]
    [SerializeField] private SummonResultUI resultUI;

    #region Firebase Properties
    private string _uid;
    private DatabaseReference _dbRef;

    private float summonNormal;
    private float summonRare;
    private float summonUnique;
    private float summonEpic;

    private SummonLevel userSummonLevel;
    private int userSummonCount;
    private int requireSummonCount;

    private List<string> allTemplateIDs = new();
    #endregion

    #region Unity LifeCycle
    private async void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        await LoadSummonDataAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadSummonDataAsync()
    {
        var profile = await CurrencyManager.Instance.LoadUserProfileAsync();
        userSummonLevel = profile.SummonLevel;
        userSummonCount = profile.SummonCount;

        var summonSnap = await _dbRef.Child("summon").GetValueAsync();
        var configSnap = summonSnap.Child(userSummonLevel.ToString());

        summonNormal = Convert.ToSingle(configSnap.Child("normal").Value);
        summonRare = Convert.ToSingle(configSnap.Child("rare").Value);
        summonUnique = Convert.ToSingle(configSnap.Child("unique").Value);
        summonEpic = Convert.ToSingle(configSnap.Child("epic").Value);
        requireSummonCount = Convert.ToInt32(configSnap.Child("count").Value);

        var listSnap = summonSnap.Child("equipmentList");
        allTemplateIDs = listSnap.Children
            .Select(item => item.Key)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }
    #endregion

    #region Summon Logic
    public async Task Summon(int times)
    {
        userSummonCount += times;
        if (userSummonCount >= requireSummonCount)
        {
            await LevelUpAsync();
        }

        var results = GenerateSummonResults(times);

        StartCoroutine(ProcessEquipmentResultsCoroutine(results));
        await SaveUserSummonCountAsync();
        resultUI.ShowSummonResult(results);
    }

    private List<EquipmentInstance> GenerateSummonResults(int times)
    {
        var results = new List<EquipmentInstance>();

        for (int i = 0; i < times; i++)
        {
            RarityType rarity = GetRandomRarity();
            string templateID = GetRandomTemplateID();

            var template = equipmentManager.allTemplates.Find(t => t.templateID == templateID);
            if (template == null) continue;

            int level = 1;
            var equipment = equipmentService.AcquireEquipment(templateID, rarity, level);
            results.Add(equipment);
        }

        return results;
    }

    private RarityType GetRandomRarity()
    {
        var pool = new Dictionary<RarityType, float>
        {
            { RarityType.Normal, summonNormal },
            { RarityType.Rare, summonRare },
            { RarityType.Unique, summonUnique },
            { RarityType.Epic, summonEpic }
        };

        float total = pool.Values.Sum();
        float rand = UnityEngine.Random.Range(0f, total);

        foreach (var kvp in pool)
        {
            if (rand <= kvp.Value) return kvp.Key;
            rand -= kvp.Value;
        }

        return pool.Keys.Last();
    }

    private string GetRandomTemplateID()
    {
        if (allTemplateIDs.Count == 0)
        {
            Debug.LogWarning("템플릿 리스트가 비어있습니다.");
            return "";
        }

        int index = UnityEngine.Random.Range(0, allTemplateIDs.Count);
        return allTemplateIDs[index];
    }

    private async Task LevelUpAsync()
    {
        if (userSummonLevel < SummonLevel.level10)
        {
            userSummonLevel = (SummonLevel)((int)userSummonLevel + 1);
            userSummonCount = 0;

            await SaveUserSummonLevelAsync();
            await LoadSummonDataAsync();
        }
    }

    private Task SaveUserSummonLevelAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("equipsummonLevel")
            .SetValueAsync((int)userSummonLevel);
    }

    private Task SaveUserSummonCountAsync()
    {
        return _dbRef.Child("users").Child(_uid).Child("profile").Child("equipsummonCount")
            .SetValueAsync(userSummonCount);
    }
    #endregion

    #region Result Handling
    private IEnumerator ProcessEquipmentResultsCoroutine(List<EquipmentInstance> results)
    {
        foreach (var equip in results)
        {
            Debug.Log($"획득 장비: {equip.templateID} / {equip.rarity} / Lv.{equip.level}");
        }

        equipmentManager.SaveToJson();
        yield return null;
    }
    #endregion
}
