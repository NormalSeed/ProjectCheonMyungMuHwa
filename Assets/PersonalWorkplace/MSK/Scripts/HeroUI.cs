using UnityEngine;
using UnityEngine.UI;

public class HeroUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button stageUpgrade;
    [SerializeField] private Button heroSet;

    private void OnEnable()
    {
        stageUpgrade.onClick.AddListener(onClickStageUpgrade);
        heroSet.onClick.AddListener(OnClickHeroSet);
    }

    private void OnDisable()
    {
        stageUpgrade.onClick.RemoveListener(onClickStageUpgrade);
        heroSet.onClick.RemoveListener(OnClickHeroSet);
    }

    #region Button OnClick
    private void onClickStageUpgrade()
    {
        
    }
    private void OnClickHeroSet()
    {

    }
    #endregion
}
