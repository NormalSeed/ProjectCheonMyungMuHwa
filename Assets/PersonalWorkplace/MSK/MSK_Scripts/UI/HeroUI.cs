using UnityEngine;
using UnityEngine.UI;

public class HeroUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button stageUpgrade;
    [SerializeField] private Button heroSet;
    [SerializeField] private Button autoSet;
    [SerializeField] private Button heroSetSave;

    [Header("Root References")]
    [SerializeField] private Transform IsHeroSetting;

    public bool IsHeroSetNow = false;

    #region Unity LifeCycle
    private void OnEnable()
    {
        // 승급 가능한 영웅이 있을 경우에만 활성화
        // stageUpgrade.onClick.AddListener(onClickStageUpgrade);
        heroSet.onClick.AddListener(OnClickHeroSet);
    }

    private void OnDisable()
    {
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


    //  영웅 배치 시작
    private void OnClickHeroSet()
    {
        //  버튼 비활성화
        heroSet.gameObject.SetActive(false);

        //  영웅 편성화면 활성화
        IsHeroSetting.gameObject.SetActive(true);
        heroSetSave.gameObject.SetActive(true);
        autoSet.gameObject.SetActive(true);

        heroSetSave.onClick.AddListener(OnClickHeroSetSave);
        autoSet.onClick.AddListener(OnClickAutoSet);
    }

    //  영웅 자동 배치
    private void OnClickAutoSet()
    {
        // 규칙에 따라서 영웅을 자동으로 배치
    }

    //  영웅 배치 저장
    private void OnClickHeroSetSave()
    {
        //  변경 후 비활성화
        IsHeroSetting.gameObject.SetActive(false);
        heroSetSave.gameObject.SetActive(false);
        autoSet.gameObject.SetActive(false);

        heroSetSave.onClick.RemoveListener(OnClickHeroSetSave);
        autoSet.onClick.RemoveListener(OnClickAutoSet);

        // 배치라기 버튼 활성화
        heroSet.gameObject.SetActive(true);
    }
    #endregion
}
