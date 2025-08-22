using UnityEngine;
using UnityEngine.UI;

public class SummonUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button summon10thButton;
    [SerializeField] private Button summon50thTimesButton;

    #region Unity LifeCycle
    private void OnEnable()
    {
        summonButton.onClick.AddListener(onClickSummon);
        summon10thButton.onClick.AddListener(onClickSummon10th);
        summon50thTimesButton.onClick.AddListener(onClickSummon50th);
    }

    private void OnDisable()
    {
        summonButton.onClick.RemoveListener(onClickSummon);
        summon10thButton.onClick.RemoveListener(onClickSummon10th);
        summon50thTimesButton.onClick.RemoveListener(onClickSummon50th);
    }
    #endregion

    #region Button OnClick
    private void onClickSummon()
    {
        SummonHeros(1);
    }
    private void onClickSummon10th()
    {
        SummonHeros(10);
    }
    private void onClickSummon50th()
    {
        SummonHeros(50);
    }
    #endregion

    #region private
    private void SummonHeros(int times)
    {

    }
    #endregion
}

