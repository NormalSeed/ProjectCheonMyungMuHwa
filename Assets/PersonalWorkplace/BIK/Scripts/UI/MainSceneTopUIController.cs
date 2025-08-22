using UnityEngine;

public class MainSceneTopUIController : MonoBehaviour
{
    #region Serialized Fields

    [Header("재화 UI")]
    [SerializeField] private GameObject _ownedGoodsOpen;
    [SerializeField] private GameObject _ownedGoodsClose;
    [SerializeField] private GameObject _allCurrencyUI; // 전체 재화 UI

    [Header("메뉴 UI")]
    [SerializeField] private GameObject _menuOpen;
    [SerializeField] private GameObject _menuClose;
    [SerializeField] private GameObject _menuUI; // 전체 재화 UI

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

    #endregion // public funcs
}
