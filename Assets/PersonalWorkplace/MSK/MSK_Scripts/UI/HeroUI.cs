using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private GameObject heroSlotPrefab; // 슬롯 프리팹

    public event Action PartySetFin;                    // 파티 편성 시작 알림
    public event Action PartySetStart;                  // 파티 편성 종료 알림

    #region Unity LifeCycle

    private void OnEnable()
    {
        // 승급 가능한 영웅이 있을 경우에만 활성화
        // stageUpgrade.onClick.AddListener(onClickStageUpgrade);
        heroSet.onClick.AddListener(OnClickHeroSet);
    }

    private void OnDisable()
    {
        infoPanel.SetActive(false);
        heroSet.onClick.RemoveListener(OnClickHeroSet);
    }
    #endregion



    #region Button OnClick

    //  자동 승급 
    private void OnClickStageUpgrade()
    {
        stageUpgrade.gameObject.SetActive(false);
        // 승급 완료 후 버튼 비활성화
        stageUpgrade.onClick.RemoveListener(OnClickStageUpgrade);
    }

    //  영웅 자동 배치
    private void OnClickAutoSet()
    {
        // 규칙에 따라서 영웅을 자동으로 배치
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

        heroSetSave.onClick.AddListener(OnClickHeroSetSave);
        heroSetEnd.onClick.AddListener(OnClickHeroSetEnd);
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

        heroSetSave.onClick.RemoveListener(OnClickHeroSetSave);
        heroSetEnd.onClick.RemoveListener(OnClickHeroSetEnd);
        autoSet.onClick.RemoveListener(OnClickAutoSet);

        PartyManager.Instance.EndPartySetting();

        heroSet.gameObject.SetActive(true);

        PartySetFin?.Invoke();
    }
    #endregion

    #region Public
    public void RefreshPartySlots()
    {
        // 기존 슬롯 제거
        foreach (Transform child in partySlotRoot)
            Destroy(child.gameObject);

        // MembersID 리스트 기준으로 슬롯 다시 생성
        for (int i = 0; i < PartyManager.Instance.MembersID.Count; i++)
        {
            var slot = Instantiate(heroSlotPrefab, partySlotRoot).GetComponent<HeroSlotUI>();
            slot.SetCard(PartyManager.Instance.MembersID[i], i);
        }

        PartyMembersCount.text = $"{PartyManager.Instance.MembersID.Count} / {PartyManager.Instance.PartySize}";
    }

    public void SetSlot(CardInfo input, int index)
    {
        if (index < 0 || index >= heroSlots.Count)
            return;

        heroSlots[index].SetCard(input, index);
    }

    #endregion
}
