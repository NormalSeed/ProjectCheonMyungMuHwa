using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System.Collections.Generic;

public class GachaRateInfoUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button exitButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI nRateText;
    [SerializeField] private TextMeshProUGUI rRateText;
    [SerializeField] private TextMeshProUGUI uRateText;
    [SerializeField] private TextMeshProUGUI lRateText;
    [SerializeField] private TextMeshProUGUI summonLevelText;

    [Header("Panel")]
    [SerializeField] private GameObject herolegendPanel;
    [SerializeField] private GameObject equiplegendPanel;
    [SerializeField] private GameObject petlegendPanel;

    [Header("Contents")]
    [SerializeField] private GameObject heroContents;
    [SerializeField] private GameObject equipContents;
    [SerializeField] private GameObject petContents;

    [Header("Category")]
    [SerializeField] private SummonCategory summonCategory;

    private Dictionary<SummonLevel, RateData> _rateCache = new();

    private DatabaseReference _dbRef;
    private string _uid;

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
        heroList,
        equipmentList,
        PetList
    }

    #region Unity

    private async void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        ShowCategoryPanels();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(index));
        }
        exitButton.onClick.AddListener(OnClickExit);

        int userLevel = await GetUserSummonLevelAsync();
        if (Enum.IsDefined(typeof(SummonLevel), userLevel))
        {
            SummonLevel summonLevel = (SummonLevel)userLevel;
            await LoadRateDataAsync(summonLevel);
            summonLevelText.text = $"Lv.{userLevel}";

            var legendPanel = GetLegendPanel();
            if (legendPanel != null)
                legendPanel.SetActive(summonLevel != SummonLevel.level01);
        }
    }

    private void OnDisable()
    {
        foreach (var button in levelButtons)
        {
            button.onClick.RemoveAllListeners();
        }
        exitButton.onClick.RemoveListener(OnClickExit);
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
        petlegendPanel.SetActive(summonCategory == SummonCategory.PetList);

        heroContents.SetActive(summonCategory == SummonCategory.heroList);
        equipContents.SetActive(summonCategory == SummonCategory.equipmentList);
        petContents.SetActive(summonCategory == SummonCategory.PetList);
    }

    private GameObject GetLegendPanel()
    {
        return summonCategory switch
        {
            SummonCategory.heroList => herolegendPanel,
            SummonCategory.equipmentList => equiplegendPanel,
            SummonCategory.PetList => petlegendPanel,
            _ => null
        };
    }

    private async Task<int> GetUserSummonLevelAsync()
    {
        var snap = await _dbRef.Child("users").Child(_uid).Child("profile").Child("summonLevel").GetValueAsync();
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
            SummonCategory.PetList => "petList",
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
        float normal = count.Normal > 0 ? rate.Normal / count.Normal : 0f;
        float rare = count.Rare > 0 ? rate.Rare / count.Rare : 0f;
        float unique = count.Unique > 0 ? rate.Unique / count.Unique : 0f;
        float epic = count.Epic > 0 ? rate.Epic / count.Epic : 0f;

        nRateText.text = $"{normal * 100f:F3}%";
        rRateText.text = $"{rare * 100f:F3}%";
        uRateText.text = $"{unique * 100f:F3}%";
        lRateText.text = $"{epic * 100f:F3}%";
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
