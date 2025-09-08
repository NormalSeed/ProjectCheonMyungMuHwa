using UnityEngine;
using UnityEngine.UI;

public class SummonUI : UIBase
{
    [Header("Panel")]
    [SerializeField] private GameObject heroSummon;
    [SerializeField] private GameObject equipSummon;
    [SerializeField] private GameObject petSummon;

    [Header("Button")]
    [SerializeField] private Button heroBtn;
    [SerializeField] private Button equipBtn;
    [SerializeField] private Button petBtn;

    #region Unity
    private void OnEnable()
    {
        Init();

    }

    private void OnDisable()
    {
        heroBtn.onClick.RemoveListener(onClickHeroButton);
        equipBtn.onClick.RemoveListener(onClickEquipButton);
        petBtn.onClick.RemoveListener(onClickPetButton);
    }
    #endregion

    #region Private
    private void Init()
    {
        heroBtn.onClick.AddListener(onClickHeroButton);
        equipBtn.onClick.AddListener(onClickEquipButton);
        petBtn.onClick.AddListener(onClickPetButton);
    }
    #endregion

    #region Button OnClick
    private void onClickHeroButton()
    {
        heroSummon.SetActive(true);
        equipSummon.SetActive(false);
        petSummon.SetActive(false);
    }
    private void onClickEquipButton()
    {
        heroSummon.SetActive(false);
        equipSummon.SetActive(true);
        petSummon.SetActive(false);
    }
    private void onClickPetButton()
    {
        heroSummon.SetActive(false);
        equipSummon.SetActive(false);
        petSummon.SetActive(true);
    }
    #endregion
}
