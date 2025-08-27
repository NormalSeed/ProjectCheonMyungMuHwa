using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneUIController : MonoBehaviour
{
    #region Structs

    [System.Serializable]
    public struct UIEntry
    {
        public UIType type;
        public UIBase ui;
    }

    #endregion // Structs





    #region serialized fields

    [Header("등록된 UI 패널들")]
    [SerializeField] private List<UIEntry> _uiEntries = new List<UIEntry>();

    [Header("하단 버튼들")]
    [SerializeField] private Button _heroUI;
    [SerializeField] private Button _dungeonUI;
    [SerializeField] private Button _upgradeUI;
    [SerializeField] private Button _inventoryUI;
    [SerializeField] private Button _summonUI;
    [SerializeField] private Button _shopUI;

    #endregion // serialized fields





    #region private fields

    private Dictionary<UIType, UIBase> _uiDict = new Dictionary<UIType, UIBase>();
    private UIBase _currentMainUI; // 현재 열려있는 메인 UI

    #endregion // private fields





    #region mono funcs

    private void Awake()
    {
        _heroUI.onClick.AddListener(() => ShowUI(UIType.Hero));
        _dungeonUI.onClick.AddListener(() => ShowUI(UIType.Dungeon));
        _upgradeUI.onClick.AddListener(() => ShowUI(UIType.Upgrade));
        _inventoryUI.onClick.AddListener(() => ShowUI(UIType.Inventory));
        _summonUI.onClick.AddListener(() => ShowUI(UIType.Summon));
        _shopUI.onClick.AddListener(() => ShowUI(UIType.Shop));
    }

    private void Start()
    {
        foreach (var entry in _uiEntries) {
            if (entry.ui == null) continue;
            if (!_uiDict.ContainsKey(entry.type)) {
                _uiDict.Add(entry.type, entry.ui);
                entry.ui.SetHide(); // 시작 시 비활성화
            }
        }
    }

    #endregion // mono funcs





    #region public funcs

    /// <summary>
    /// UIType으로 UI 열기 (메인 UI 전용)
    /// </summary>
    public void ShowUI(UIType type)
    {
        if (!_uiDict.TryGetValue(type, out var ui) || ui == null) return;

        // 한번 더 눌렀을 때 닫기
        if (_currentMainUI == ui) {
            _currentMainUI.SetHide();
            _currentMainUI = null;
            return;
        }

        // 기존 메인 UI 닫기
        if (_currentMainUI != null) {
            _currentMainUI.SetHide();
        }

        // 새로운 메인 UI 열기
        _currentMainUI = ui;
        _currentMainUI.SetShow();
        _currentMainUI.RefreshUI();
    }

    /// <summary>
    /// 모든 메인 UI 닫기
    /// </summary>
    public void CloseAllUI()
    {
        if (_currentMainUI != null) {
            _currentMainUI.SetHide();
            _currentMainUI = null;
        }
    }

    /// <summary>
    /// 현재 열려 있는 메인 UI 가져오기
    /// </summary>
    public UIBase GetCurrentUI()
    {
        return _currentMainUI;
    }

    #endregion // public funcs
}
