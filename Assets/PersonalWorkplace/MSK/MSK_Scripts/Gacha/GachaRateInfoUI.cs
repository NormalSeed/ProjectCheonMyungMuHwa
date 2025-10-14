using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaRateInfoUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button exitButton;

    [Header("Text Groups")]
    [SerializeField] private List<TextMeshProUGUI> heroRateTexts;
    [SerializeField] private List<TextMeshProUGUI> equipRateTexts;
    [SerializeField] private TextMeshProUGUI summonLevelText;

    [Header("Panel")]
    [SerializeField] private GameObject herolegendPanel;
    [SerializeField] private GameObject equiplegendPanel;

    [Header("Contents")]
    [SerializeField] private GameObject heroContents;
    [SerializeField] private GameObject equipContents;

    [SerializeField] private List<HeroRateInfoSetting> heroRateCards;
    [SerializeField] private List<EquipRateSetting> equipRateSettings;

    private SummonCategory summonCategory;

    private DatabaseReference _dbRef;
    private string _uid;


    public string normalRate;           // 노말 확률
    public string rareRate;             // 레어 확률
    public string uniqueRate;           // 유니크 확률
    public string LegendaryRate;        // 전설 확률


    private class RateData
    {
        public float Normal;
        public float Rare;
        public float Unique;
        public float Epic;
    }

    private class HeroCountData
    {
        public int Normal;
        public int Rare;
        public int Unique;
        public int Epic;
    }

    public enum SummonCategory
    {
        heroList = 1,
        equipmentList = 2,
    }

    #region Unity
    private void Awake()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;
    }

    private async void OnEnable()
    {
        ShowCategoryPanels();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(index));
        }
        exitButton.onClick.AddListener(OnClickExit);
        int userLevel = 0;

        if ((int)summonCategory == 1)
            userLevel = await GetUserSummonLevelAsync("summonCount");
        else
            userLevel = await GetUserSummonLevelAsync("equipsummonLevel");

        if (userLevel == 0)
        {
            Debug.Log("소환레벨 설정 안됨");
            return;
        }

        if (Enum.IsDefined(typeof(SummonLevel), userLevel))
        {
            SummonLevel summonLevel = (SummonLevel)userLevel;
            await LoadRateDataAsync(summonLevel);
            summonLevelText.text = $"Lv.{userLevel}";

            var legendPanel = GetLegendPanel();
            if (legendPanel != null)
                legendPanel.SetActive(summonLevel != SummonLevel.level01);

            if (summonCategory == SummonCategory.heroList)
            {
                foreach (var card in heroRateCards)
                {
                    card.SetHeroText();
                }
            }
            else if (summonCategory == SummonCategory.equipmentList)
            {
                foreach (var equip in equipRateSettings)
                {
                    equip.Init();
                }
            }
        }
    }

    private void OnDisable()
    {
        foreach (var button in levelButtons)
        {
            button.onClick.RemoveAllListeners();
        }
        exitButton.onClick.RemoveListener(OnClickExit);
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Public

    public void SetupCategory(int categoryValue)
    {
        if (!Enum.IsDefined(typeof(SummonCategory), categoryValue))
        {
            Debug.LogWarning($"[GachaRateInfoUI] 잘못된 카테고리 값: {categoryValue}");
            return;
        }

        summonCategory = (SummonCategory)categoryValue;
        ShowCategoryPanels();
    }

    #endregion

    #region OnClick

    private async void OnLevelButtonClicked(int levelIndex)
    {
        SummonLevel summonLevel = (SummonLevel)(levelIndex + 1);
        await LoadRateDataAsync(summonLevel);
        summonLevelText.text = $"Lv.{(int)summonLevel}";

        var legendPanel = GetLegendPanel();
        if (legendPanel != null)
            legendPanel.SetActive(summonLevel != SummonLevel.level01);

        if (summonCategory == SummonCategory.heroList)
        {
            foreach (var card in heroRateCards)
            {
                card.SetHeroText();
            }
        }
        else if (summonCategory == SummonCategory.equipmentList)
        {
            foreach (var equip in equipRateSettings)
            {
                equip.Init();
            }
        }
    }


    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Private

    private void ShowCategoryPanels()
    {
        herolegendPanel.SetActive(summonCategory == SummonCategory.heroList);
        equiplegendPanel.SetActive(summonCategory == SummonCategory.equipmentList);

        heroContents.SetActive(summonCategory == SummonCategory.heroList);
        equipContents.SetActive(summonCategory == SummonCategory.equipmentList);
    }

    private GameObject GetLegendPanel()
    {
        return summonCategory switch
        {
            SummonCategory.heroList => herolegendPanel,
            SummonCategory.equipmentList => equiplegendPanel,
            _ => null
        };
    }

    private List<TextMeshProUGUI> GetCurrentRateTextGroup()
    {
        return summonCategory switch
        {
            SummonCategory.heroList => heroRateTexts,
            SummonCategory.equipmentList => equipRateTexts,
            _ => null
        };
    }

    private async Task<int> GetUserSummonLevelAsync(string type)
    {
        var snap = await _dbRef.Child("users").Child(_uid).Child("profile").Child($"{type}").GetValueAsync();
        if (snap == null || !snap.Exists)
        {
            Debug.LogWarning($"[GachaRateInfoUI] summonLevel 정보 없음: {_uid}");
            return 1;
        }

        return Convert.ToInt32(snap.Value);
    }

    private async Task<RateData> LoadSummonRateAsync(SummonLevel summonLevel)
    {
        string levelKey = summonLevel.ToString();
        var snap = await _dbRef.Child("summon").Child(levelKey).GetValueAsync();

        if (snap == null || !snap.Exists)
        {
            Debug.LogWarning($"[GachaRateInfoUI] summon/{levelKey} 경로 없음");
            return null;
        }

        return new RateData
        {
            Normal = Convert.ToSingle(snap.Child("normal").Value),
            Rare = Convert.ToSingle(snap.Child("rare").Value),
            Unique = Convert.ToSingle(snap.Child("unique").Value),
            Epic = Convert.ToSingle(snap.Child("epic").Value)
        };
    }

    private async Task<HeroCountData> LoadCountsByCategoryAsync()
    {
        string path = summonCategory switch
        {
            SummonCategory.heroList => "heroList",
            SummonCategory.equipmentList => "equipmentList",
            _ => "heroList"
        };

        var snap = await _dbRef.Child("summon").Child(path).GetValueAsync();

        if (snap == null || !snap.Exists)
        {
            Debug.LogWarning($"[GachaRateInfoUI] summon/{path} 경로 없음");
            return null;
        }

        return new HeroCountData
        {
            Normal = (int)snap.Child("normal").ChildrenCount,
            Rare = (int)snap.Child("rare").ChildrenCount,
            Unique = (int)snap.Child("unique").ChildrenCount,
            Epic = (int)snap.Child("epic").ChildrenCount
        };
    }

    private void UpdateRateUI(RateData rate, HeroCountData count)
    {
        var rateTexts = GetCurrentRateTextGroup();
        if (rateTexts == null || rateTexts.Count < 4)
        {
            Debug.LogWarning("[GachaRateInfoUI] 텍스트 그룹이 잘못 설정됨");
            return;
        }

        float normal, rare, unique, epic;

        if (summonCategory == SummonCategory.equipmentList)
        {
            const int fixedEquipCount = 16;
            normal = rate.Normal / fixedEquipCount;
            rare = rate.Rare / fixedEquipCount;
            unique = rate.Unique / fixedEquipCount;
            epic = rate.Epic / fixedEquipCount;
        }
        else
        {
            normal = count.Normal > 0 ? rate.Normal / count.Normal : 0f;
            rare = count.Rare > 0 ? rate.Rare / count.Rare : 0f;
            unique = count.Unique > 0 ? rate.Unique / count.Unique : 0f;
            epic = count.Epic > 0 ? rate.Epic / count.Epic : 0f;
        }

        normalRate = $"{normal * 100f:F3}%";
        rareRate = $"{rare * 100f:F3}%";
        uniqueRate = $"{epic * 100f:F3}%";
        LegendaryRate = $"{unique * 100f:F3}%";


        rateTexts[0].text = $"총합 {rate.Normal * 100f:F3}%";
        rateTexts[1].text = $"총합 {rate.Rare * 100f:F3}%";
        rateTexts[2].text = $"총합 {rate.Unique * 100f:F3}%";
        rateTexts[3].text = $"총합 {rate.Epic * 100f:F3}%";
    }

    private async Task LoadRateDataAsync(SummonLevel summonLevel)
    {
        var rate = await LoadSummonRateAsync(summonLevel);
        var count = await LoadCountsByCategoryAsync();

        if (rate == null || count == null)
            return;

        UpdateRateUI(rate, count);
    }
    #endregion
}
