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

    #region Unity LifeCycle
    public void PostStart()
    {
        Debug.Log("PostStart 실행됨");
    }
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
        SetHeroName();
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
    private void SetHeroName()
    {
        heroName.text = chardata.HeroName;
    }

    #endregion
}

