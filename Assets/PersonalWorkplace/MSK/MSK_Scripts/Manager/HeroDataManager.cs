using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Linq;
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
    [Inject] private EquipmentManager equipmentManager;
    // 외부 참조용
    public static HeroDataManager Instance { get; private set; }
    // 초기화 여부
    public bool IsInitialized { get; private set; }

    public Dictionary<string, HeroData> ownedHeroes = new();
    public List<HeroData> heroTemplates = new();

    private string _uid;
    private DatabaseReference _dbRef;

    private readonly string savePath = Path.Combine(Application.persistentDataPath, "hero_cache.json");

    private Dictionary<string, object> ConvertHeroToDict(HeroData hero)
    {
        return new Dictionary<string, object>
    {
        { "heroName", hero.heroName },
        { "hasHero", hero.hasHero },
        { "heroPiece", hero.heroPiece },
        { "level", hero.level },
        { "stage", hero.stage },
        { "rarity", hero.rarity },
        { "heroId", hero.heroId },
        { "cardInfo", JsonUtility.ToJson(hero.cardInfo) },
        { "PlayerModelSO", hero.PlayerModelSO != null ? JsonUtility.ToJson(hero.PlayerModelSO) : null },
        { "weapone", hero.weapone },
        { "armor", hero.armor },
        { "boots", hero.boots },
        { "gloves", hero.gloves }
    };
    }

    // 뽑기 레어도 조각 변환 필드
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

    // 새 영웅 추가 코드
    public void AddNewHero(CardInfo card)
    {
        var template = heroTemplates.Find(t => t.heroId == card.HeroID);
        var modelSO = template != null ? template.PlayerModelSO : null;
        var heroName = modelSO != null ? modelSO.CharName : "Unknown";

        var heroData = new HeroData
        {
            heroName = heroName,
            hasHero = true,
            heroPiece = 0,
            level = 1,
            stage = 1,
            rarity = card.rarity.ToString(),
            heroId = card.HeroID,
            cardInfo = card,
            PlayerModelSO = modelSO,
            weapone = string.Empty,
            armor = string.Empty,
            boots = string.Empty,
            gloves = string.Empty
        };

        ownedHeroes[card.HeroID] = heroData;
    }


    public HeroDataManager(List<HeroTemplateSO> templates)
    {
        heroTemplates = new List<HeroData>();

        foreach (var template in templates)
        {
            var hero = new HeroData
            {
                heroId = template.heroId,
                heroName = template.heroName,
                rarity = template.rarity.ToString(),
                hasHero = false,
                heroPiece = 0,
                level = 1,
                stage = 1,
                cardInfo = template.cardInfo,
                PlayerModelSO = template.modelSO,
                weapone = "",
                armor = "",
                boots = "",
                gloves = ""
            };
            heroTemplates.Add(hero);
        }

        Debug.Log($"[HeroDataManager] 템플릿 {heroTemplates.Count}개 변환 완료");
    }



    #region Unity
    public void Start()
    {
        Instance = this;
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;
        Debug.Log("[HeroDataMagaer] Start 실행");
        WaitForHeroModelsAndInit();
    }

    private async void WaitForHeroModelsAndInit()
    {
        // HeroModels 초기화 대기
        while (HeroModels.Instance == null || !HeroModels.Instance.IsInitialized)
        {
            await Task.Delay(100);
        }

        Debug.Log("[HeroDataManager] HeroModels 초기화 완료 확인");

        LoadHeroDataFromCache();
        await LoadHeroDataFromFirebase();

        RefreshAllCombatPower();
        SaveHeroDataToCache();

        IsInitialized = true;
        Debug.Log("[HeroDataManager] 초기화 완료 → IsInitialized = true");
    }
    #endregion

    #region Private
    // 최초 로딩함수
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

        foreach (var hero in ownedHeroes.Values)
        {
            if (hero.PlayerModelSO == null)
            {
                var template = heroTemplates.Find(t => t.heroId == hero.heroId);
                if (template != null)
                {
                    hero.PlayerModelSO = template.PlayerModelSO;
                    hero.cardInfo = new CardInfo
                    {
                        HeroID = template.cardInfo.HeroID,
                        HeroName = template.cardInfo.HeroName,
                        // 필요한 필드 복사
                    };
                }
                else
                {
                    Debug.LogWarning($"[LoadHeroDataFromFirebase] template이 null입니다: heroId={hero.heroId}");
                    hero.cardInfo = new CardInfo
                    {
                        HeroID = hero.heroId,
                        HeroName = "Unknown"
                        // 기본값으로라도 생성
                    };
                }

                foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
                {
                    if (hero.cardInfo == null)
                    {
                        Debug.LogError($"[LoadHeroDataFromFirebase] hero.cardInfo가 null입니다: heroId={hero.heroId}");
                        continue;
                    }

                    var equip = equipmentManager.allEquipments
                        .FirstOrDefault(e => e.charID == hero.cardInfo.HeroID && e.equipmentType == type);

                    if (equip != null)
                    {
                        ApplyHeorStats(equip, hero.cardInfo.HeroID);
                    }
                    StatModifierManager.ApplyToCard(hero.cardInfo);
                }
                Debug.Log($"[LoadHeroDataFromFirebase] {hero.heroName} 전투력 적용");
            }
        }

        Debug.Log($"[HeroDataManager] 서버에서 영웅 {ownedHeroes.Count}명 로딩 완료");
    }

    #endregion

    // 로딩함수
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
    // 영웅 저장 함수
    public void SaveHeroData(string heroId)
    {
        SaveHeroDataToCache();
        SaveHeroDataToFirebase(heroId);
    }

    //  모든 영웅 저장
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

    // 특정 영웅 저장
    public void SaveHeroDataToCache(string heroId)
    {
        if (!ownedHeroes.TryGetValue(heroId, out var hero)) return;

        HeroSaveList saveList = new();
        saveList.heroes.Add(new HeroSaveData
        {
            heroId = heroId,
            json = JsonUtility.ToJson(hero)
        });

        string jsonText = JsonUtility.ToJson(saveList, true);
        File.WriteAllText(savePath, jsonText);
    }

    // DB에 저장
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

    // 모두 DB에 저장
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

    // 조각 반환
    public void AddHeroPiece(string heroId, int amount)
    {
        if (ownedHeroes.TryGetValue(heroId, out var hero))
        {
            hero.heroPiece += amount;
        }
    }

    // 뽑기 결과반환
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
    public float CalculateCombatPower(HeroData hero)
    {
        var model = hero.PlayerModelSO; 
        return 2.0f * (
            (model.InnAtkPoint + model.ExtAtkPoint) *
            (1 + model.CritRate * (model.CritDamage - 1)) + 
            1.4f * model.DefPoint + 0.1f * model.HealthPoint
       );
    }
    private void RefreshAllCombatPower()
    {
        foreach (var hero in ownedHeroes.Values)
        {
            float power = CalculateCombatPower(hero);
            hero.cardInfo.combatPower = power;
        }
    }

    public void ApplyHeorStats(EquipmentInstance instance, string charId)
    {
        Debug.LogWarning($" [ApplyHeorStats] : {instance.statType}");
        var value = instance.GetStat();
        string originID = instance.instanceID;

        switch (instance.statType)
        {
            case StatType.Attack:
                StatModifierManager.ApplyModifier(charId,
                    new StatModifier(StatType.Attack, value / 100f, ModifierSource.Equipment, originID, true));
                StatModifierManager.ApplyModifier(charId,
                    new StatModifier(StatType.Attack, value / 100f, ModifierSource.Equipment, originID, true));
                break;
            case StatType.Defense:
                StatModifierManager.ApplyModifier(charId,
                    new StatModifier(StatType.Defense, value / 100f, ModifierSource.Equipment, originID, true));
                break;
            case StatType.CritRate:
                StatModifierManager.ApplyModifier(charId,
                    new StatModifier(StatType.CritRate, value / 100f, ModifierSource.Equipment, originID));
                break;
            case StatType.CritDamage:
                StatModifierManager.ApplyModifier(charId,
                    new StatModifier(StatType.CritDamage, value / 100f, ModifierSource.Equipment, originID));
                break;

            default:
                Debug.LogWarning($"알 수 없는 StatType: {instance.statType}");
                break;
        }
    }
    public EquipmentInstance GetEquipment(string charID, EquipmentType type)
    {
        Debug.Log("[GetEquipment] 진입");

        var instance = equipmentManager.allEquipments
            .FirstOrDefault(e => e.charID == charID && e.equipmentType == type);

        if (instance == null)
        {
            Debug.LogWarning($"[GetEquipment] 장비 없음: {charID}, {type}");
        }

        return instance;
    }

}