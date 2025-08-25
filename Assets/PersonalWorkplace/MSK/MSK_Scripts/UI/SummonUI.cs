using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button summon10thButton;
    [SerializeField] private Button summon50thTimesButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI summonLevel;
    [SerializeField] private TextMeshProUGUI button1;
    [SerializeField] private TextMeshProUGUI button10;
    [SerializeField] private TextMeshProUGUI button50;

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        summonButton.onClick.RemoveListener(onClickSummon);
        summon10thButton.onClick.RemoveListener(onClickSummon10th);
        summon50thTimesButton.onClick.RemoveListener(onClickSummon50th);
    }

    private void Init()
    {
        summonButton.onClick.AddListener(onClickSummon);
        summon10thButton.onClick.AddListener(onClickSummon10th);
        summon50thTimesButton.onClick.AddListener(onClickSummon50th);
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

    private void ChangeButtonText(TextMeshProUGUI text)
    {
        /*   TODO : 가진 재화를 확인하여 소환 타입을 설정하기   */
    }
    #endregion
}

