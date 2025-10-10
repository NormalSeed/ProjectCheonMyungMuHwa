using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PopupEntry
{
    public PopupType popupType;
    public UIBase uiBase;
}

public partial class PopupManager : MonoBehaviour
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

    public void ShowRewardPopup(List<ItemData> rewards, List<BigCurrency> rewardCount, bool bonus = false, float bonusRate = 0f)
    {
        if (!_popupDict.TryGetValue(PopupType.RewardPopup, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Reward 팝업이 등록되지 않았습니다.");
            return;
        }
        if (uiBase is RewardUI rewardUI) {
            rewardUI.SetShow(rewards, rewardCount, bonus, bonusRate);
            Debug.Log($"[PopupManager] ShowRewardPopup: {rewards.Count}개 보상");
        }
        else {
            Debug.LogError("[PopupManager] PopupType.Reward 이 RewardUI가 아님");
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

    public void ShowConfirmPopup(string title, string message, Action onConfirm, Action onCancel = null)
    {
        Debug.Log("[PopupManager] ShowConfirmPopup 호출됨");
        if (!_popupDict.TryGetValue(PopupType.Confirm, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Confirm 팝업이 등록되지 않았습니다.");
            return;
        }

        Debug.Log($"[PopupManager] Confirm popup found: {uiBase.name}, activeInHierarchy={uiBase.gameObject.activeInHierarchy}");
        if (uiBase is ConfirmPopup confirmPopup) {
            Debug.Log("[PopupManager] ConfirmPopup.SetShow 호출 전");
            confirmPopup.SetShow(title, message, onConfirm, onCancel);
            Debug.Log("[PopupManager] ConfirmPopup.SetShow 호출 후");
        }
        else {
            Debug.LogError("[PopupManager] PopupType.Confirm 이 ConfirmPopup이 아님 (타입: " + uiBase.GetType().Name + ")");
        }
    }

    public void ShowHeroGetPopup(CardInfo cardInfo)
    {
        Debug.Log("[PopupManager] ShowHeroGetPopup 호출됨");
        if (!_popupDict.TryGetValue(PopupType.HeroGet, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] HeroGet 팝업이 등록되지 않았습니다.");
            return;
        }

        if (uiBase is HeroGetPopup HeroGetPopup) {
            HeroGetPopup.SetShow(cardInfo);
        }
        else {

        }
    }

    public void ShowStagePopup(int stage)
    {
        Debug.Log("[PopupManager] ShowStagePopup 호출됨");
        if (!_popupDict.TryGetValue(PopupType.Stage, out var uiBase) || uiBase == null) {
            Debug.LogWarning("[PopupManager] Stage 팝업이 등록되지 않았습니다.");
            return;
        }

        Debug.Log($"[PopupManager] Stage popup found: {uiBase.name}, activeInHierarchy={uiBase.gameObject.activeInHierarchy}");
        if (uiBase is StagePopup stagePopup) {
            Debug.Log("[PopupManager] StagePopup.SetShow 호출 전");
            stagePopup.SetShow(stage);
            Debug.Log("[PopupManager] StagePopup.SetShow 호출 후");
        }
        else {
            Debug.LogError("[PopupManager] PopupType.Stage 이 StagePopup이 아님 (타입: " + uiBase.GetType().Name + ")");
        }
    }

    #endregion // public funcs
}
