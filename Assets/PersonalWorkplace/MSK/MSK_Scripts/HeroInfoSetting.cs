using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HeroInfoSetting : MonoBehaviour
{
    [Header("Hero ID")]
    [SerializeField] private string heroID;
    public string HeroID { get => heroID; set => heroID = value; }
    public CardInfo chardata;

    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot; // 배경 레어도
    [SerializeField] private Image characterRoot;          // 캐릭터 이미지
    [SerializeField] private Transform stageRoot;          // 돌파상태
    [SerializeField] private Transform badgeRoot;          // 캐릭터 소속
    [SerializeField] private Transform selectRoot;         // 배치 선택여부 

    [Header("UI")]
    [SerializeField] private Button CardButton;             // 캐릭터 카드
    [SerializeField] private TextMeshProUGUI PartyNum;      // 배치 순서

    [SerializeField] private HeroInfoUI heroInfoUI;         // 캐릭터 정보 판넬
    [SerializeField] private HeroUI heroUI;

    private HeroData heroData;
    #region Unity LifeCycle

    private async void OnEnable()
    {
        if (HeroDataManager.Instance.ownedHeroes.TryGetValue(heroID, out var data))
        {
            heroData = data;
            string cardInfoKey = $"{heroID}CardInfo";
            var handle = Addressables.LoadAssetAsync<CardInfo>(cardInfoKey);
            await handle.Task;

            string modelKey = $"{heroID}_model";
            var modelHandle = Addressables.LoadAssetAsync<PlayerModelSO>(modelKey);
            await modelHandle.Task;

            // modelHandle handle 둘 다 완료시 
            if (handle.Status == AsyncOperationStatus.Succeeded && modelHandle.Status == AsyncOperationStatus.Succeeded)
            {
                chardata = handle.Result;
                heroData.cardInfo = chardata;
                heroData.PlayerModelSO = modelHandle.Result;
            }
            await Init();
        }
    }

    private void OnDisable()
    {
        CardButton.onClick.RemoveListener(OnClickCard);
         if (heroUI != null) heroUI.partySetFin -= HeroSetting;
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Init    
    private async Task Init()
    {
        SetBackground();
        await SetCharacter(heroData.PlayerModelSO.SpriteKey);
        SetStage();
        SetBadge();

        CardButton.onClick.AddListener(OnClickCard);
         if (heroUI != null) heroUI.partySetFin += HeroSetting;
    }
    private void SetBackground()
    {
        foreach (Transform child in cardBackgroundRoot)
            child.gameObject.SetActive(false);

        Transform target = cardBackgroundRoot.Find(heroData.cardInfo.rarity.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private async Task SetCharacter(string spriteKey)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(spriteKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            characterRoot.sprite = handle.Result;
    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(heroData.cardInfo.faction.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private void SetStage()
    {
        foreach (Transform stage in stageRoot)
        {
            stage.Find("Stage_Red")?.gameObject.SetActive(false);
            stage.Find("Stage_gray")?.gameObject.SetActive(true);
        }

        for (int i = 1; i <= heroData.stage; i++)
        {
            Transform stage = stageRoot.Find("Stage" + i);
            if (stage != null)
            {
                stage.Find("Stage_Red")?.gameObject.SetActive(true);
                stage.Find("Stage_gray")?.gameObject.SetActive(false);
            }
        }
    }
    #endregion


    #region OnClick
    private void OnClickCard()
    {
        //  파티를 편성중이라면
        if (PartyManager.Instance.IsHeroSetNow)
        {
            if (!PartyManager.Instance.partyMembers.Contains(gameObject))
            {
                PartyManager.Instance.AddMember(gameObject);
                PartyManager.Instance.AddMemberID(heroID);
                selectRoot.gameObject.SetActive(true);
            }
            else
            {
                PartyManager.Instance.RemoveMember(gameObject);
                PartyManager.Instance.RemoveMemberID(heroID);
                selectRoot.gameObject.SetActive(false);
            }
        }
        else
        {
            heroInfoUI.SetHeroData(heroData);
            heroInfoUI.gameObject.SetActive(true);
        }   
    }
    #endregion


    #region Private
    /// <summary>
    /// 회색으로 표시된 배치표시를 비활성화
    /// </summary>
    private void HeroSetting()
    {
        selectRoot.gameObject.SetActive(false);
    }
    #endregion
}
