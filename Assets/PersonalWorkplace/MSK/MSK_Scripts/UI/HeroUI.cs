using System;
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

    public event Action partySetFin;                    // 파티 편성 시작 알림
    public event Action partySetStart;                  // 파티 편성 종료 알림

    #region Unity LifeCycle

    public void Start() { }

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
    private void onClickStageUpgrade()
    {
        stageUpgrade.gameObject.SetActive(false);
        // 승급 완료 후 버튼 비활성화
        stageUpgrade.onClick.RemoveListener(onClickStageUpgrade);
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
        
        partySetStart?.Invoke();
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

        partySetFin?.Invoke();
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

        partySetFin?.Invoke();
    }
    #endregion
}
