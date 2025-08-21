using System.Collections;
using System.Collections.Generic;
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
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == heroUI)
            return;
        activeUI = heroUI;
        activeUI.SetActive(true);
    }
    private void OnClickDungeon()
    {
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == dungeonUI)
            return;
        activeUI = dungeonUI;
        activeUI.SetActive(true);
    }
    private void OnClickUpgrade()
    {
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == upgradeUI)
            return;
        activeUI = upgradeUI;
        activeUI.SetActive(true);
    }
    private void OnClickInventory()
    {
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == inventoryUI)
            return;
        activeUI = inventoryUI;
        activeUI.SetActive(true);
    }
    private void OnClickSummon()
    {
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == summonUI)
            return;
        activeUI = summonUI;
        activeUI.SetActive(true);
    }
    private void OnClickShop()
    {
        if (activeUI != null)
            activeUI.SetActive(false);
        if (activeUI == shopUI)
            return;
        activeUI = shopUI;
        activeUI.SetActive(true);
    }
    #endregion
}
