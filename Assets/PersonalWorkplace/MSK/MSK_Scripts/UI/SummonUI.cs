using UnityEngine;
using UnityEngine.UI;

public class SummonUI : UIBase
{
    [Header("Panel")]
    [SerializeField] private GameObject heroSummon;
    [SerializeField] private GameObject equipSummon;

    [Header("Button")]
    [SerializeField] private Button heroBtn;
    [SerializeField] private Button equipBtn;

    #region Unity
    private void OnEnable()
    {
        Init();

    }

    private void OnDisable()
    {
        heroSummon.SetActive(true);
        equipSummon.SetActive(false);;

        heroBtn.onClick.RemoveListener(onClickHeroButton);
        equipBtn.onClick.RemoveListener(onClickEquipButton);
    }
    #endregion

    #region Private
    private void Init()
    {
        heroBtn.onClick.AddListener(onClickHeroButton);
        equipBtn.onClick.AddListener(onClickEquipButton);
    }
    #endregion

    #region Button OnClick
    private void onClickHeroButton()
    {
        heroSummon.SetActive(true);
        equipSummon.SetActive(false);
    }
    private void onClickEquipButton()
    {
        heroSummon.SetActive(false);
        equipSummon.SetActive(true);
    }
    private void onClickPetButton()
    {
        heroSummon.SetActive(false);
        equipSummon.SetActive(false);
    }
    #endregion
}
