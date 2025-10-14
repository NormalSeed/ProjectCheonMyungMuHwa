using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HeroRateInfoSetting : MonoBehaviour
{
    [Header("SO")]
    [SerializeField] public CardInfo chardata;

    public string HeroID { get; private set; }
    private HeroRarity rarity;

    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot; // 배경 레어도
    [SerializeField] private Image characterRoot;          // 캐릭터 이미지
    [SerializeField] private TextMeshProUGUI heroName;     // 영웅이름
    [SerializeField] private TextMeshProUGUI rate;         // 확률
    [SerializeField] private GachaRateInfoUI rateUI;

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }
    #endregion

    #region Init    
    private void Init()
    {
        if (chardata == null)
            return;

        HeroID = chardata.HeroID;
        rarity = chardata.rarity;

        SetBackground();
        SetCharacter();
        SetHeroText();
    }

    private void SetBackground()
    {
        foreach (Transform child in cardBackgroundRoot)
            child.gameObject.SetActive(false);

        Transform target = cardBackgroundRoot.Find(rarity.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetCharacter()
    {
        Addressables.LoadAssetAsync<Sprite>(HeroID + "_sprite").Completed += task =>
        {
            if (task.Status == AsyncOperationStatus.Succeeded)
            {
                characterRoot.sprite = task.Result;
            }
        };

    }
    public void SetHeroText()
    {
        heroName.text = chardata.HeroName;

        switch (rarity)
        {
            case HeroRarity.Normal:
                rate.text = rateUI.normalRate;
                break;
            case HeroRarity.Rare:
                rate.text = rateUI.rareRate;
                break;
            case HeroRarity.Unique:
                rate.text = rateUI.uniqueRate;
                break;
            case HeroRarity.Legend:
                rate.text = rateUI.LegendaryRate;
                break;
            default:
                rate.text = "확률 정보 없음";
                break;
        }
    }
    #endregion
}

