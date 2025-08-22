using UnityEngine;
using UnityEngine.UI;

public class MainBottomUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button heroButton;
    [SerializeField] private Button dungeonButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button shopButton;

    [Header("UI Object")]
    [SerializeField] private GameObject heroUI;
    [SerializeField] private GameObject dungeonUI;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject summonUI;
    [SerializeField] private GameObject shopUI;

    private GameObject activeUI;

    private void Awake()
    {
        heroButton.onClick.AddListener(OnClickHero);
        dungeonButton.onClick.AddListener(OnClickDungeon);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        inventoryButton.onClick.AddListener(OnClickInventory);
        summonButton.onClick.AddListener(OnClickSummon);
        shopButton.onClick.AddListener(OnClickShop);
    }

    #region Button OnClick
    private void OnClickHero()
    {
        ShowUI(heroUI);
    }
    private void OnClickDungeon()
    {
        ShowUI(dungeonUI);
    }
    private void OnClickUpgrade()
    {
        ShowUI(upgradeUI);
    }
    private void OnClickInventory()
    {
        ShowUI(inventoryUI);
    }
    private void OnClickSummon()
    {
        ShowUI(summonUI);
    }
    private void OnClickShop()
    {
        ShowUI(shopUI);
    }
    #endregion

    #region private
    private void ShowUI(GameObject targetUI)
    {
        if (activeUI == targetUI && activeUI.activeSelf)
        {
            activeUI.SetActive(false);
            activeUI = null;
            return;
        }

        if (activeUI != null)
            activeUI.SetActive(false);

        activeUI = targetUI;
        activeUI.SetActive(true);
    }
    #endregion
}
