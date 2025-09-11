using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

#region Serializable
[Serializable]
public class HeroSaveList
{
    public List<HeroSaveData> heroes = new();
}

[Serializable]
public class HeroSaveData
{
    public string heroId;
    public string json; // HeroData를 그대로 JSON으로 저장
}
#endregion


public class HeroDataManager : IStartable
{
    public static HeroDataManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }

    public Dictionary<string, HeroData> ownedHeroes = new();
    public List<HeroData> allTemplates = new();

    private string _uid;
    private DatabaseReference _dbRef;

    private readonly string savePath = Path.Combine(Application.persistentDataPath, "hero_cache.json");

    private Dictionary<string, object> ConvertHeroToDict(HeroData hero)
    {
        return new Dictionary<string, object>
        {
            { "hasHero", hero.hasHero },
            { "heroPiece", hero.heroPiece },
            { "level", hero.level },
            { "rarity", hero.rarity },
            { "stage", hero.stage },
            { "cardInfo", JsonUtility.ToJson(hero.cardInfo) }
        };
    }
    public int GetPieceAmountByRarity(HeroRarity rarity)
    {
        return rarity switch
        {
            HeroRarity.Normal => 1,
            HeroRarity.Rare => 3,
            HeroRarity.Unique => 5,
            HeroRarity.Legend => 10,
            _ => 0
        };
    }
    public void AddNewHero(CardInfo card)
    {
        var heroData = new HeroData
        {
            hasHero = true,
            heroPiece = 0,
            stage = 1,
            rarity = card.rarity.ToString(),
            heroId = card.HeroID,
            cardInfo = card
        };

        ownedHeroes[card.HeroID] = heroData;
    }

    public HeroDataManager(List<HeroData> values)
    {
        this.allTemplates = values;
    }


    #region Unity
    public async void Start()
    {
        Instance = this;
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        LoadHeroDataFromCache();
        await LoadHeroDataFromFirebase();
        SaveHeroDataToCache();

        IsInitialized = true;
    }
    #endregion

    #region Private
    private async Task LoadHeroDataFromFirebase()
    {
        var heroRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");
        var snapshot = await heroRef.GetValueAsync();

        ownedHeroes.Clear();

        foreach (var child in snapshot.Children)
        {
            string heroId = child.Key;
            string json = child.GetRawJsonValue();

            HeroData hero = JsonUtility.FromJson<HeroData>(json);
            ownedHeroes[heroId] = hero;
        }

        Debug.Log($"[HeroDataManager] 서버에서 영웅 {ownedHeroes.Count}명 로딩 완료");
    }
    #endregion


    public void LoadHeroDataFromCache()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("영웅 캐시 파일이 존재하지 않습니다.");
            return;
        }

        string jsonText = File.ReadAllText(savePath);
        var saveList = JsonUtility.FromJson<HeroSaveList>(jsonText);

        foreach (var data in saveList.heroes)
        {
            HeroData hero = JsonUtility.FromJson<HeroData>(data.json);
            ownedHeroes[data.heroId] = hero;
        }
    }

    public void SaveHeroData(string heroId)
    {
        SaveHeroDataToCache();
        SaveHeroDataToFirebase(heroId);
    }

    public void SaveHeroDataToCache()
    {
        var saveList = new HeroSaveList();

        foreach (var kvp in ownedHeroes)
        {
            string json = JsonUtility.ToJson(kvp.Value);
            saveList.heroes.Add(new HeroSaveData
            {
                heroId = kvp.Key,
                json = json
            });
        }

        string jsonText = JsonUtility.ToJson(saveList, true);
        File.WriteAllText(savePath, jsonText);
    }

    public void SaveHeroDataToFirebase(string heroId)
    {
        if (string.IsNullOrEmpty(_uid))
            return;

        if (!ownedHeroes.TryGetValue(heroId, out var hero))
            return;

        var heroRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(heroId);
        var heroDict = ConvertHeroToDict(hero);
        heroRef.SetValueAsync(heroDict).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log($"[HeroDataManager] '{heroId}' Firebase 저장 완료");
        });
    }

    public void SaveAllHeroDataToFirebase()
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        var heroRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");

        Dictionary<string, object> heroData = new();
        foreach (var kvp in ownedHeroes)
        {
            heroData[kvp.Key] = ConvertHeroToDict(kvp.Value);
        }
        heroRef.SetValueAsync(heroData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[HeroDataManager] 전체 영웅 Firebase 저장 완료");
        });
    }
    public void AddHeroPiece(string heroId, int amount)
    {
        if (ownedHeroes.TryGetValue(heroId, out var hero))
        {
            hero.heroPiece += amount;
        }
    }
    public void ApplyGachaResults(List<CardInfo> results)
    {
        foreach (var card in results)
        {
            if (!ownedHeroes.ContainsKey(card.HeroID))
                AddNewHero(card);
            else
                AddHeroPiece(card.HeroID, GetPieceAmountByRarity(card.rarity));
        }
    }
}