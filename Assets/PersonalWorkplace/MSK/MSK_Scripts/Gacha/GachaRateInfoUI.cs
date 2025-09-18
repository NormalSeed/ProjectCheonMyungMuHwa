using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

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
    [SerializeField] private GameObject legendPanel;

    [Header("Category")]
    [SerializeField] private SummonCategory summonCategory;
    public enum SummonCategory
    {
        Hero,
        Equip,
        Pet
    }

    private DatabaseReference _dbRef;
    private string _uid;

    #region Unity

    private void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;
            levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(index));
        }
        exitButton.onClick.AddListener(OnClickExit);
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
        int summonLevel = levelIndex + 1;
        await LoadRateDataAsync(summonLevel);
        summonLevelText.text = $"Lv.{summonLevel}";
        legendPanel.SetActive(summonLevel == 1);
    }
    
    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }
    #endregion

    #region private
    private async Task LoadRateDataAsync(int summonLevel)
    {
        string categoryKey = summonCategory.ToString().ToLower(); // "hero", "equip", "pet"

        var snap = await _dbRef.Child("summon").Child(categoryKey).Child(summonLevel.ToString()).GetValueAsync();
        if (snap == null || !snap.Exists)
        {
            Debug.LogWarning($"[GachaRateInfoUI] summon/{categoryKey}/{summonLevel} 경로 없음");
            return;
        }

        float normal = Convert.ToSingle(snap.Child("normal").Value);
        float rare = Convert.ToSingle(snap.Child("rare").Value);
        float unique = Convert.ToSingle(snap.Child("unique").Value);
        float epic = Convert.ToSingle(snap.Child("epic").Value);

        nRateText.text = $"{normal * 100f:F1}%";
        rRateText.text = $"{rare * 100f:F1}%";
        uRateText.text = $"{unique * 100f:F1}%";
        lRateText.text = $"{epic * 100f:F1}%";
    }
    #endregion
}
