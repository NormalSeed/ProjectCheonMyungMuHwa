using UnityEngine;
using UnityEngine.UI;

public class HeroUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button stageUpgrade;
    [SerializeField] private Button heroSet;
    [SerializeField] private Button heroSetSave;



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
    private void onClickStageUpgrade()
    {
        stageUpgrade.gameObject.SetActive(false);
        // 승급 완료 후 버튼 비활성화
        stageUpgrade.onClick.RemoveListener(onClickStageUpgrade);
    }
    private void OnClickHeroSet()
    {
        // 영웅 배치에 변경이 있을 경우에만 활성화
        //heroSetSave.onClick.AddListener(OnClickHeroSetSave);
    }
    private void OnClickHeroSetSave()
    {
        //  변경 후 버튼 비활성화
        heroSetSave.gameObject.SetActive(false);
        heroSetSave.onClick.RemoveListener(OnClickHeroSetSave);
    }
    #endregion
}
