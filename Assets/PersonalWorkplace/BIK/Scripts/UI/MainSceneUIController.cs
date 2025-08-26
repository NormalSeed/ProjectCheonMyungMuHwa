using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    None = 0,
    Notice,
    Quest,
    Mail,
    Attendance,
    Ranking,
    Setting,
}


public class MainSceneUIController : MonoBehaviour
{
    [System.Serializable]
    public struct UIEntry
    {
        public UIType type;
        public UIBase ui;
    }

    [Header("등록된 UI 패널들")]
    [SerializeField] private List<UIEntry> _uiEntries = new List<UIEntry>();

    private Dictionary<UIType, UIBase> _uiDict = new Dictionary<UIType, UIBase>();
    private Stack<UIBase> _uiStack = new Stack<UIBase>();

    #region Unity Methods
    private void Start()
    {
        // 딕셔너리에 매핑
        foreach (var entry in _uiEntries) {
            if (entry.ui == null) continue;
            if (!_uiDict.ContainsKey(entry.type)) {
                _uiDict.Add(entry.type, entry.ui);
                entry.ui.SetHide(); // 시작 시 비활성화
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// UIType으로 UI 열기
    /// </summary>
    public void ShowUI(UIType type)
    {
        if (!_uiDict.TryGetValue(type, out var ui) || ui == null) return;

        ui.SetShow();
        ui.RefreshUI();
        _uiStack.Push(ui);
    }

    /// <summary>
    /// 현재 UI 닫기
    /// </summary>
    public void CloseCurrentUI()
    {
        if (_uiStack.Count == 0) return;

        var currentUI = _uiStack.Pop();
        currentUI.SetHide();
    }

    /// <summary>
    /// 모든 UI 닫기
    /// </summary>
    public void CloseAllUI()
    {
        while (_uiStack.Count > 0) {
            var ui = _uiStack.Pop();
            ui.SetHide();
        }
    }
    #endregion

    #region Helper
    /// <summary>
    /// 현재 활성화된 UI 가져오기
    /// </summary>
    public UIBase GetCurrentUI()
    {
        return _uiStack.Count > 0 ? _uiStack.Peek() : null;
    }

    /// <summary>
    /// 특정 UI 가져오기
    /// </summary>
    public UIBase GetUI(UIType type)
    {
        return _uiDict.TryGetValue(type, out var ui) ? ui : null;
    }
    #endregion
}
