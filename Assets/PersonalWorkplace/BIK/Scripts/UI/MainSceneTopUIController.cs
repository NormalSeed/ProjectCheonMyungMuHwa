using UnityEngine;

public class MainSceneTopUIController : MonoBehaviour
{
    #region Serialized Fields

    [Header("재화 UI")]
    [SerializeField] private GameObject _ownedGoodsOpen;
    [SerializeField] private GameObject _ownedGoodsClose;
    [SerializeField] private GameObject _allCurrencyUI;

    [Header("메뉴 UI")]
    [SerializeField] private GameObject _menuOpen;
    [SerializeField] private GameObject _menuClose;
    [SerializeField] private GameObject _menuUI;

    [Header("UI 컨트롤러 참조")]
    [SerializeField] private MainSceneUIController _uiController;

    #endregion // Serialized Fields





    #region public funcs

    public void OnClick_OwnedGoods()
    {
        _allCurrencyUI.SetActive(!_allCurrencyUI.activeSelf);
        _ownedGoodsOpen.SetActive(!_allCurrencyUI.activeSelf);
        _ownedGoodsClose.SetActive(_allCurrencyUI.activeSelf);
    }

    public void OnClick_Menu()
    {
        _menuUI.SetActive(!_menuUI.activeSelf);
        _menuOpen.SetActive(!_menuUI.activeSelf);
        _menuClose.SetActive(_menuUI.activeSelf);
    }

    public void OnClick_Notice()
    {
        _uiController.ShowUI(UIType.Notice);
    }

    public void OnClick_Quest()
    {
        _uiController.ShowUI(UIType.Quest);
    }

    public void OnClick_Mail()
    {
        _uiController.ShowUI(UIType.Mail);
    }

    public void OnClick_Attendance()
    {
        _uiController.ShowUI(UIType.Attendance);
    }

    public void OnClick_Ranking()
    {
        _uiController.ShowUI(UIType.Ranking);
    }

    public void OnClick_Setting()
    {
        _uiController.ShowUI(UIType.Setting);
    }

    #endregion // public funcs
}
