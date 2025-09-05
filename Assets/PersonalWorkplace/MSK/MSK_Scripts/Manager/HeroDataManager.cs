using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;

public class HeroDataManager : IStartable
{
    public static HeroDataManager Instance { get; private set; }

    private string _uid;
    private DatabaseReference _dbRef;
    public Dictionary<string, HeroData> ownedHeroes = new();

    void IStartable.Start()
    {
        Start();
    }
    private void Start()
    {
        Instance = this;    
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        LoadAllHeroData();
    }

    private async void LoadAllHeroData()
    {
        if (string.IsNullOrEmpty(_uid))
            return;

        var heroInfoPath = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");

        try
        {
            DataSnapshot snapshot = await heroInfoPath.GetValueAsync();
            ownedHeroes.Clear();

            foreach (var child in snapshot.Children)
            {
                string heroId = child.Key;
                string json = child.GetRawJsonValue();

                HeroData hero = JsonUtility.FromJson<HeroData>(json);
                ownedHeroes[heroId] = hero;

                await LoadSOAssetsAsync(heroId, hero);
            }

            Debug.Log($"총 {ownedHeroes.Count}명의 영웅 정보를 불러왔습니다.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase 데이터 로딩 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// CardInfo를 불러오는 코드입니다.
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="hero"></param>
    private async Task LoadCardInfoAsync(string heroId, HeroData hero)
    {
        string cardInfoKey = heroId + "CardInfo";
        var handle = Addressables.LoadAssetAsync<CardInfo>(cardInfoKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            hero.cardInfo = handle.Result;

            hero.cardInfo.HeroStage = handle.Result.HeroStage;
        }
        else
        {
            Debug.LogWarning($"CardInfo 로드 실패: {cardInfoKey}");
        }
    }
    /// <summary>
    /// PlayerModeSO를 설정하는 코드입니다.
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="hero"></param>
    /// <returns></returns>
    private async Task LoadPlayerModelSOAsync(string heroId, HeroData hero)
    {
        string modelKey = heroId + "_model";
        var handle = Addressables.LoadAssetAsync<PlayerModelSO>(modelKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            hero.PlayerModelSO = handle.Result;
            hero.PlayerModelSO.Level = handle.Result.Level;
        }
        else
        {
            Debug.LogWarning($"PlayerModelSO 로드 실패: {modelKey}");
        }
    }
    /// <summary>
    /// SKill정보를 설정하는 코드입니다.
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
    private async Task LoadSkillSOAsync(HeroData hero)
    {
        /*
        if (hero.PlayerModelSO == null)
        {
            Debug.LogWarning("PlayerModelSO가 null이므로 SkillSO를 로드할 수 없습니다.");
            return;
        }

        string skillKey = hero.PlayerModelSO.SkillSetID;
        Debug.Log($"[Addressables] 스킬 키: {skillKey}");

        var handle = Addressables.LoadAssetAsync<PlayerSkillSO>(skillKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            hero.PlayerSkillSO = handle.Result;
        }
        else
        {
            Debug.LogWarning($"PlayerSkillSO 로드 실패: {skillKey}");
        }
        */
    }
    /// <summary>
    /// 영웅정보를 설정하는 코드입니다.
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="hero"></param>
    /// <returns></returns>
    private async Task LoadSOAssetsAsync(string heroId, HeroData hero)
    {
        await LoadCardInfoAsync(heroId, hero);
        await LoadPlayerModelSOAsync(heroId, hero);
        await LoadSkillSOAsync(hero);
        Debug.Log($"[{heroId}]의 정보를 설정했습니다.");
        Debug.Log(JsonUtility.ToJson(hero, true));
    }
}