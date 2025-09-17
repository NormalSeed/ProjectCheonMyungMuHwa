using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEditor.Progress;

[Serializable]
public class PopupEntry
{
    public PopupType popupType;
    public UIBase uiBase;
}

public class PopupManager : MonoBehaviour
{
    #region Singleton

    public static PopupManager Instance { get; private set; }

    #endregion // Singleton





    #region serialized fields

    [Header("Popup Panel")]
    [SerializeField] private List<PopupEntry> _popupEntries = new List<PopupEntry>();

    #endregion // serialized fields





    #region private fields

    private Dictionary<PopupType, UIBase> _popupDict;

    #endregion // private fields





    #region mono funcs

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Dictionary 변환
        _popupDict = new Dictionary<PopupType, UIBase>();
        foreach (var entry in _popupEntries) {
            if (!_popupDict.ContainsKey(entry.popupType))
                _popupDict.Add(entry.popupType, entry.uiBase);
        }
    }

    #endregion // mono funcs





    #region public funcs

    public void ShowTooltip(ItemData item)
    {
        if (!_popupDict.TryGetValue(PopupType.Tooltip, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Tooltip 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is TootipPanel tooltipPanel) {
            tooltipPanel.SetShow(item);
            Debug.Log($"[PopupManager] ShowTooltip: {item.Name}");
        }
        else {
            Debug.LogError("[PopupManager] PopupType.Tooltip 이 TooltipPanel이 아님");
        }
    }

    //public void ShowPopup(PopupType popupType, string message, Action onLeft = null, Action onRight = null)
    //{
    //    if (!_popupDict.TryGetValue(popupType, out var popup) || popup == null) {
    //        Debug.LogWarning($"[PopupManager] 등록되지 않은 팝업: {popupType}");
    //        return;
    //    }

    //    popup.SetShow();
    //    Debug.Log($"[PopupManager] ShowPopup: {popupType}, Message: {message}");

    //    // TODO: popup 내부에 message, 버튼 콜백 전달
    //}

    //public void ClosePopup(PopupType popupType)
    //{
    //    if (_popupDict.TryGetValue(popupType, out var popup) && popup != null) {
    //        popup.SetHide();
    //        Debug.Log($"[PopupManager] ClosePopup: {popupType}");
    //    }
    //}

    public void ShowLevelUpPopup(int lastLevel, int currLevel)
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.Level, lastLevel.ToString(), currLevel.ToString());
        }
        else {

        }
    }

    public void ShowBossStagePopup(int stage)
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.Boss, stage.ToString());
        }
        else {

        }
    }

    public void ShowPowerUpPanel(BigCurrency currPower, BigCurrency changePower)
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.PowerUp, currPower.ToString(), changePower.ToString());
        }
        else {

        }
    }

    public void ShowPowerDownPanel(BigCurrency currPower, BigCurrency changePower)
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.PowerDown, currPower.ToString(), changePower.ToString());
        }
        else {

        }
    }

    public void ShowMissionClearPanel(string missionClearText)
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.MissionClear, missionClearText);
        }
        else {

        }
    }

    public void ShowStageClearPopup()
    {
        if (!_popupDict.TryGetValue(PopupType.Alert, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Alert 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is AlertPanel tooltipPanel) {
            tooltipPanel.SetShow(AlertType.ClearStage);
        }
        else {

        }
    }

    public void CloseAllPopups()
    {
        foreach (var kvp in _popupDict) {
            kvp.Value?.SetHide();
        }
        Debug.Log("[PopupManager] 모든 팝업 닫음");
    }

    public bool TryGetPopup(PopupType popupType, out UIBase popup)
    {
        return _popupDict.TryGetValue(popupType, out popup);
    }
    #endregion // public funcs
}
