using System.Threading.Tasks;
using TMPro;
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
    [SerializeField] public Transform selectRoot;         // 배치 선택여부 

    [Header("UI")]
    [SerializeField] private Button CardButton;             // 캐릭터 카드
    [SerializeField] public TextMeshProUGUI PartyNum;      // 배치 순서

    [SerializeField] private HeroInfoUI heroInfoUI;         // 캐릭터 정보 판넬
    [SerializeField] private HeroUI heroUI;

    public HeroData heroData;
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
        if (heroUI != null) heroUI.PartySetFin -= HeroSettingEnd;
        if (heroUI != null) heroUI.PartySetStart -= HeroSettingStart;
        if (heroUI != null) heroUI.PartyNumChanged -= HeroSettingStart;
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Init    
    private async Task Init()
    {
        SetBackground();
        SetCharacter();
        SetStage();
        SetBadge();
        CardButton.onClick.RemoveListener(OnClickCard);
        CardButton.onClick.AddListener(OnClickCard);
        if (heroUI != null) heroUI.PartySetFin += HeroSettingEnd;
        if (heroUI != null) heroUI.PartySetStart += HeroSettingStart;
        if (heroUI != null) heroUI.PartyNumChanged += HeroSettingStart;
    }
    private void SetBackground()
    {
        foreach (Transform child in cardBackgroundRoot)
            child.gameObject.SetActive(false);

        Transform target = cardBackgroundRoot.Find(heroData.cardInfo.rarity.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private void SetCharacter()
    {
        characterRoot.sprite = HeroSprites.Instance.GetCharacterSprite(heroID);
    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(heroData.cardInfo.faction.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    public void SetStage()
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
    public void OnClickCard()
    {
        //  파티를 편성중이라면
        if (PartyManager.Instance.IsHeroSetNow)
        {
            if (!PartyManager.Instance.MembersID.Contains(chardata))
            {
                PartyManager.Instance.AddMember(chardata);
                selectRoot.gameObject.SetActive(true);
                PartyNum.text = (PartyManager.Instance.MembersID.Count).ToString();
            }
            else
            {
                PartyManager.Instance.RemoveMember(chardata);
                selectRoot.gameObject.SetActive(false);
            }
            heroUI.RefreshAllPartyNum();
        }
        else
        {
            heroInfoUI.SetHeroData(heroData);
            heroInfoUI.gameObject.SetActive(true);
            AudioManager.Instance.PlaySound("6. 팝업 열 때 사운드");
        }
    }
    #endregion


    #region Private

    #endregion

    #region Public
    /// <summary>
    /// 회색으로 표시된 배치표시를 비활성화
    /// </summary>
    public void HeroSettingEnd()
    {
        selectRoot.gameObject.SetActive(false);
    }
    public void HeroSettingStart()
    {
        for (int i = 0; i < PartyManager.Instance.MembersID.Count; i++)
        {
            var member = PartyManager.Instance.MembersID[i];
            if (member == chardata)
            {
                selectRoot.gameObject.SetActive(true);
                PartyNum.text = (i + 1).ToString();
                break;
            }
        }
    }
    #endregion
}
