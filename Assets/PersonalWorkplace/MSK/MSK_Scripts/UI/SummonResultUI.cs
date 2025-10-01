using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SummonResultUI : UIBase
{
    [Header("Button")]
    [SerializeField] private Button resultButton;
    
    [Header("Pool")]
    [SerializeField] private GachaCardPoolManager poolManager;

    [Header("Panel")]
    [SerializeField] private SummonEquipUI SummonEquipUI;
    [SerializeField] private SummonHeroUI SummonHeroUI;

    private List<GameObject> cardsToShow = new();
    private List<GameObject> equipCardsToShow = new();
    private Coroutine revealRoutine;
    private Coroutine equipRevealRoutine;

    private bool skipAnimation = false;
    private bool isShowCards = false;
    public void SkipReveal()
    {
        skipAnimation = true;
    }

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        resultButton.onClick.RemoveListener(OnClickResult);
    }
    #endregion

    #region Init
    private void Init()
    {
        resultButton.onClick.AddListener(OnClickResult);
    }

    private void OnClickResult()
    {
        if (isShowCards)
        {
            SkipSummonAnimation();
        }
        else
        {
            poolManager.ReturnAll();
            this.gameObject.SetActive(false);
        }
    }
    #endregion



    #region Private
    // 영웅 획득 코루틴
    private IEnumerator ShowCardsRoutine()
    {
        foreach (var card in cardsToShow)
        {
            var setting = card.GetComponent<CardSetting>();
            var info = setting.chardata;

            if (info.rarity == HeroRarity.Legend)
            {
                PopupManager.Instance.ShowHeroGetPopup(info);
            }

            card.SetActive(true);

            if (skipAnimation)
                continue;
            AudioManager.Instance.PlaySound("1. 모든 캐릭터 획득 사운드");
            yield return new WaitForSeconds(0.1f);
        }

        SummonHeroUI.HandleGachaCompleted();
        Debug.Log("[ShowCardsRoutine]");
        skipAnimation = false;
        isShowCards = false;
    }
    // 장비 획득 코루틴
    private IEnumerator ShowEquipCardsRoutine()
    {
        foreach (var card in equipCardsToShow)
        {
            card.SetActive(true);

            if (skipAnimation)
                continue;

            AudioManager.Instance.PlaySound("1. 모든 캐릭터 획득 사운드");
            yield return new WaitForSeconds(0.1f);
        }

        SummonEquipUI.HandleGachaCompleted();
        skipAnimation = false;
        isShowCards = false;
    }


    /// <summary>
    /// 소환 결과를 보여줍니다.
    /// </summary>
    /// <param name="times"></param>
    public void ShowSummonResult(List<CardInfo> results)
    {
        isShowCards = true;
        gameObject.SetActive(true);
        poolManager.ReturnAll();
        cardsToShow.Clear();
        skipAnimation = false;

        for (int i = 0; i < results.Count; i++)
        {
            var info = results[i];
            var card = poolManager.GetCard();
            var setting = card.GetComponent<CardSetting>();
            setting.chardata = info;

            card.transform.SetAsLastSibling();
            card.SetActive(false); // 정보만 넣고 숨김
            cardsToShow.Add(card);
        }

        // 활성화 코루틴 시작
        revealRoutine = StartCoroutine(ShowCardsRoutine());
    }


    /// <summary>
    /// 장비 소환 결과를 보여줍니다.
    /// </summary>
    /// <param name="results">소환된 장비 리스트</param>
    public void ShowSummonResult(List<EquipmentInstance> results)
    {
        isShowCards = true;
        gameObject.SetActive(true);
        poolManager.ReturnAll();
        equipCardsToShow.Clear();
        skipAnimation = false;

        foreach (var equip in results)
        {
            var card = poolManager.GetCard();
            var display = card.GetComponent<EquipmentCardDisplay>();

            if (display != null)
            {
                display.SetData(equip);
            }

            card.transform.SetAsLastSibling();
            card.SetActive(false); // 미리 숨겨두기
            equipCardsToShow.Add(card);
        }

        equipRevealRoutine = StartCoroutine(ShowEquipCardsRoutine());
    }


    // 스킵 입력
    public void SkipSummonAnimation()
    {
        skipAnimation = true;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
            ShowRemainingCardsInstantly();
        }

        if (equipRevealRoutine != null)
        {
            StopCoroutine(equipRevealRoutine);
            equipRevealRoutine = null;
            ShowRemainingEquipCardsInstantly();
        }
    }

    // 장비 스킵
    private void ShowRemainingEquipCardsInstantly()
    {
        foreach (var card in equipCardsToShow)
        {
            if (!card.activeSelf)
            {
                card.SetActive(true);
            }
        }

        SummonEquipUI.HandleGachaCompleted();
        AudioManager.Instance.PlaySound("1. 모든 캐릭터 획득 사운드");
        skipAnimation = false;
        isShowCards = false;
    }

    // 영웅 스킵
    private void ShowRemainingCardsInstantly()
    {
        foreach (var card in cardsToShow)
        {
            if (!card.activeSelf)
            {
                var setting = card.GetComponent<CardSetting>();
                var info = setting.chardata;

                if (info.rarity == HeroRarity.Legend)
                {
                    PopupManager.Instance.ShowHeroGetPopup(info);
                }

                card.SetActive(true);
            }
        }
        AudioManager.Instance.PlaySound("1. 모든 캐릭터 획득 사운드");
        SummonHeroUI.HandleGachaCompleted();
        skipAnimation = false;
        isShowCards = false;
    }

    #endregion
}
