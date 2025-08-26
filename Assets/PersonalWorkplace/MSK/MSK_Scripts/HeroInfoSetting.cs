using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class HeroInfoSetting : MonoBehaviour
{
    [Header("SO")]
    [SerializeField] private CardInfo chardata;

    public string HeroID { get; private set; }
    public int HeroStage { get; private set; }
    private HeroRarity rarity;
    private HeroRelationship relationship;

    // 개인 폴더 경로로 되어 있어서 추후 수정 필요
    private string path = "Assets/PersonalWorkplace/MSK/MSK_Sprite/SampleChar";

    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot; // 배경 레어도
    [SerializeField] private Sprite characterRoot;          // 캐릭터 이미지
    [SerializeField] private Transform stageRoot;          // 돌파상태
    [SerializeField] private Transform badgeRoot;          // 캐릭터 소속
    [SerializeField] private Transform selectRoot;         // 배치 선택여부 

    [Header("UI")]
    [SerializeField] private Button CardButton;             // 캐릭터 카드
    [SerializeField] private TextMeshProUGUI PartyNum;      // 배치 순서



    // UI관리자에게 캐릭터 정보 판넬에 대한 정보를 받아와서 열어야 할 듯
    private HeroInfoUI HeroInfoUI;         // 캐릭터 정보 판넬
    private HeroUI heroUI;
    private PartyManager partyManager;

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        CardButton.onClick.RemoveListener(OnClickCard);
    }
    #endregion

    #region Init    
    private void Init()
    {
        HeroID = chardata.HeroID;
        HeroStage = chardata.HeroStage;
        rarity = chardata.rarity;
        relationship = chardata.relationship;

        SetBackground();
        SetCharacter();
        SetStage();
        SetBadge();

        CardButton.onClick.AddListener(OnClickCard);
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
        Addressables.LoadAssetAsync<Sprite>(HeroID).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                characterRoot = handle.Result;
            }
        };

    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(relationship.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private void SetStage()
    {
        foreach (Transform stage in stageRoot)
        {
            Transform red = stage.Find("Stage_Red");
            Transform gray = stage.Find("Stage_gray");

            if (red != null) red.gameObject.SetActive(false);
            if (gray != null) gray.gameObject.SetActive(true);
        }

        for (int i = 1; i <= HeroStage; i++)
        {
            Transform stage = stageRoot.Find("Stage" + i);
            if (stage != null)
            {
                Transform red = stage.Find("Stage_Red");
                Transform gray = stage.Find("Stage_gray");

                if (red != null) red.gameObject.SetActive(true);
                if (gray != null) gray.gameObject.SetActive(false);
            }
        }
    }
    #endregion


    #region OnClick
    private void OnClickCard()
    {
        Debug.Log(chardata);
        partyManager.AddMember(chardata);
        //    HeroUIActive(); 
        //  TODO : 파티 편성 구현하기
        // 파티 편성중일 경우 HeroSetting()
        // 선택 효과 + 편성 번호 표시
    }
    #endregion


    #region Private
    private void HeroSetting()
    {
        selectRoot.gameObject.SetActive(true);

    }

    private void HeroUIActive()
    {
        // 캐릭터 정보 SO도 함께 전달해주어야 한다.
        // HeroInfoUI.SetActive(true);
    }
    #endregion
}
