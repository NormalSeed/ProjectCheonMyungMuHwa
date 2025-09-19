using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HeroUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button stageUpgrade;       // 자동 일괄승급
    [SerializeField] private Button heroSet;            // 파티편성 시작
    [SerializeField] private Button autoSet;            // 파티 자동편성
    [SerializeField] private Button heroSetSave;        // 편성파티 저장
    [SerializeField] private Button heroSetEnd;         // 파티 편성 취소

    [Header("Root References")]
    [SerializeField] private Transform IsHeroSetting;
    [SerializeField] private GameObject infoPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI PartyMembersCount;

    [Header("HeroSlot")]
    [SerializeField] List<HeroSlotUI> heroSlots;
    [SerializeField] private Transform partySlotRoot;   // 슬롯들이 들어갈 부모 오브젝트

    [Header("HeroCard")]
    [SerializeField] List<HeroInfoSetting> heroCard;

    public event Action PartySetFin;                    // 파티 편성 시작 알림
    public event Action PartySetStart;                  // 파티 편성 종료 알림
    public event Action PartyNumChanged;                // 파티 순서 변경 알림

    #region Unity LifeCycle

    private void OnEnable()
    {
        stageUpgrade.onClick.AddListener(OnClickStageUpgrade);
        heroSet.onClick.AddListener(OnClickHeroSet);
        CheckUpgradableHeroes();
    }

    private void OnDisable()
    {
        infoPanel.SetActive(false);
        stageUpgrade.onClick.RemoveListener(OnClickStageUpgrade);
        heroSet.onClick.RemoveListener(OnClickHeroSet);
    }
    #endregion



    #region Button OnClick

    // 자동 승급
    private void OnClickStageUpgrade()
    {
        foreach (var ownedHero in HeroDataManager.Instance.ownedHeroes)
        {
            var hero = ownedHero.Value;

            // 승급 로직
            while (hero.stage < 5)
            {
                int rarityValue = (int)hero.cardInfo.rarity;
                int requiredPiece = hero.stage * (5 - rarityValue);

                if (hero.heroPiece < requiredPiece)
                    break;

                hero.heroPiece -= requiredPiece;
                hero.stage++;

                // 저장
                CurrencyManager.Instance.SaveHeroStageToFireBase(hero.heroId, hero.stage);
                CurrencyManager.Instance.SavePieceToFireBase(hero.heroId, hero.heroPiece);
            }
            // UI 갱신
            foreach (var cardUI in heroCard)
            {
                if (cardUI.chardata.HeroID == hero.heroId)
                {
                    cardUI.SetStage();

                }
            }
        }

        CheckUpgradableHeroes(); // 버튼 상태 갱신
        stageUpgrade.onClick.RemoveListener(OnClickStageUpgrade);
    }

    //  영웅 자동 배치
    private void OnClickAutoSet()
    {
        // 규칙에 따라서 영웅을 자동으로 배치

        Debug.LogWarning("[OnClickAutoSet] 입력됨");
    }


    //  영웅 배치 시작
    private void OnClickHeroSet()
    {
        //  버튼 비활성화
        heroSet.gameObject.SetActive(false);

        //  편성변수 True
        PartyManager.Instance.StartPartySetting();  //편성 시작
        //  영웅 편성화면 활성화
        IsHeroSetting.gameObject.SetActive(true);
        heroSetSave.gameObject.SetActive(true);
        autoSet.gameObject.SetActive(true);
        heroSetEnd.gameObject.SetActive(true);

        Debug.LogWarning("[heroSetSave] 등록");
        heroSetSave.onClick.AddListener(OnClickHeroSetSave);
        Debug.LogWarning("[heroSetEnd] 등록");
        heroSetEnd.onClick.AddListener(OnClickHeroSetEnd);
        Debug.LogWarning("리스너 등록됨: " + heroSetEnd.onClick != null);
        Debug.LogWarning("[heroSetEnd] 등록");
        autoSet.onClick.AddListener(OnClickAutoSet);

        PartySetStart?.Invoke();
    }


    //  영웅 배치 저장
    private void OnClickHeroSetSave()
    {
        //  변경 후 비활성화
        IsHeroSetting.gameObject.SetActive(false);
        heroSetSave.gameObject.SetActive(false);
        autoSet.gameObject.SetActive(false);
        heroSetEnd.gameObject.SetActive(false);

        heroSetSave.onClick.RemoveListener(OnClickHeroSetSave);
        heroSetEnd.onClick.RemoveListener(OnClickHeroSetEnd);
        autoSet.onClick.RemoveListener(OnClickAutoSet);

        PartyManager.Instance.EndPartySetting();    // 편성 종료

        // 배치하기 버튼 활성화
        heroSet.gameObject.SetActive(true);
        PartySetFin?.Invoke();
    }

    // 영웅 배치하지 않고 저장
    private void OnClickHeroSetEnd()
    {
        IsHeroSetting.gameObject.SetActive(false);
        heroSetSave.gameObject.SetActive(false);
        autoSet.gameObject.SetActive(false);
        heroSetEnd.gameObject.SetActive(false);

        heroSetSave.onClick.RemoveListener(OnClickHeroSetSave);
        heroSetEnd.onClick.RemoveListener(OnClickHeroSetEnd);
        autoSet.onClick.RemoveListener(OnClickAutoSet);
        SlotClear();
        PartyManager.Instance.StartWithoutPartySetting();
        heroSet.gameObject.SetActive(true);

        PartySetFin?.Invoke();
    }
    #endregion

    #region Private
    //  승급 가능 여부 반환
    private void CheckUpgradableHeroes()
    {
        foreach (var hero in HeroDataManager.Instance.ownedHeroes.Values)
        {
            if (hero.stage >= 5) continue;
            Debug.LogWarning($"{hero.heroId}");
            Debug.LogWarning($"{hero.cardInfo.rarity}");
            int rarityValue = (int)hero.cardInfo.rarity;
            int requiredPiece = hero.stage * (5 - rarityValue);

            if (hero.heroPiece >= requiredPiece)
            {
                stageUpgrade.gameObject.SetActive(true);
                return;
            }
        }
        stageUpgrade.gameObject.SetActive(false);
    }

    private void SlotClear()
    {
        // 파티 멤버 리스트 복사 후 초기화
        var members = PartyManager.Instance.MembersID;
        PartyManager.Instance.MembersID.Clear();

        // 해당 멤버에 대응하는 카드만 HeroSettingEnd 호출
        foreach (var member in members)
        {
            foreach (var card in heroCard)
            {
                if (card.chardata == member)
                {
                    card.HeroSettingEnd();
                    break; // 찾았으면 이탈
                }
            }
        }

        // 슬롯 초기화
        for (int i = 0; i < heroSlots.Count; i++)
        {
            if (i < PartyManager.Instance.MembersID.Count)
            {
                var card = PartyManager.Instance.MembersID[i];
                heroSlots[i].SetCard(card, i);
            }
            else
            {
                heroSlots[i].SetCard(null, i);
            }
        }
    }
    #endregion

    #region Public
    public void RefreshSlot(CardInfo input)
    {
        int index = PartyManager.Instance.MembersID.IndexOf(input);
        if (index < 0 || index >= heroSlots.Count) return;

        heroSlots[index].SetCard(input, index);
        PartyManager.Instance.PartyLoadUI();
        PartyNumChanged?.Invoke();
    }
    public void RefreshAllCards()
    {
        foreach (var card in heroCard)
        {
            card.SetStage();
        }
    }
    public void SetSlot(CardInfo input, int index)
    {
        if (index < 0 || index >= heroSlots.Count)
            return;

        heroSlots[index].SetCard(input, index);
    }

    #endregion
}
