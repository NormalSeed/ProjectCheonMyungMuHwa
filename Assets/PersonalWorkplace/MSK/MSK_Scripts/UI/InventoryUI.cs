using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class InventoryUI : UIBase
{
    [SerializeField] private List<ItemSlot> _itemSlots = new();
    [SerializeField] private Scrollbar _itemUseScrollbar;

    [SerializeField] private Button _useButton;
    [SerializeField] private Button _closeButton;

    private ItemSlot _selectedSlot;
    private int _useCount = 1;

    // 1) 가능하면 주입
    [Inject] private TableManager _tableManager;

    private TItem _itemTable;
    private bool _itemTableReady = false;
    private bool _eventsHooked = false;
    private Coroutine _resolveCo;

    private void Awake()
    {
        foreach (var slot in _itemSlots) {
            slot.OnClick += OnItemSlotClicked;
        }
        _itemUseScrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        _useButton.onClick.AddListener(OnClickUseButton);
        _closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void OnEnable()
    {
        if (_tableManager == null) {
            _resolveCo ??= StartCoroutine(WaitAndResolveTableManager());
        }
        else {
            PostResolveInit();
        }

        UpdateUseButtonInteractable();
    }

    private void OnDisable()
    {
        if (_resolveCo != null) {
            StopCoroutine(_resolveCo);
            _resolveCo = null;
        }

        UnhookEvents();
    }

    // === Lazy Resolve ===
    private IEnumerator WaitAndResolveTableManager()
    {
        IObjectResolver resolver = null;

        while (resolver == null) {
            var scopes = FindObjectsOfType<GameLifetimeScope>(true);

            for (int i = 0; i < scopes.Length; i++) {
                var s = scopes[i];
                if (s != null && s.Container != null) {
                    resolver = s.Container;
                    break;
                }
            }

            if (resolver == null)
                yield return null; // 아직 Firebase/Build 전이면 다음 프레임 대기
        }

        // 3) 원하는 컨테이너에서만 Resolve
        if (_tableManager == null) {
            _tableManager = resolver.Resolve<TableManager>();
        }

        PostResolveInit();
        _resolveCo = null;
    }

    private void PostResolveInit()
    {
        // TItem 테이블 준비
        if (_itemTable == null) {
            _itemTable = _tableManager.GetTable<TItem>(TableType.Item);
            if (_itemTable != null) {
                if (_itemTable.IsInitialized) {
                    _itemTableReady = true;
                }
                else {
                    _itemTable.OnLoaded += HandleItemTableLoaded;
                }
            }
        }

        // 이벤트 연결(중복 방지)
        if (!_eventsHooked && InventoryManager.Instance != null) {
            if (InventoryManager.Instance.IsInitialized && _itemTableReady) {
                RefreshAllSlots();
            }
            else {
                InventoryManager.OnInitialized += RefreshAllSlots;
            }
            InventoryManager.Instance.OnItemChanged += OnItemChanged;
            _eventsHooked = true;
        }
    }

    private void UnhookEvents()
    {
        if (_itemTable != null) {
            _itemTable.OnLoaded -= HandleItemTableLoaded;
        }
        if (_eventsHooked && InventoryManager.Instance != null) {
            InventoryManager.OnInitialized -= RefreshAllSlots;
            InventoryManager.Instance.OnItemChanged -= OnItemChanged;
        }
        _eventsHooked = false;
    }

    private void HandleItemTableLoaded()
    {
        _itemTableReady = true;
        RefreshAllSlots();
    }

    // === UI 갱신 ===
    private void RefreshAllSlots()
    {
        if (!_itemTableReady || InventoryManager.Instance == null) return;

        var items = InventoryManager.Instance.Items;

        int slotIndex = 0;
        foreach (var kv in items) {
            if (slotIndex >= _itemSlots.Count) break;

            string invKey = kv.Key;
            int count = kv.Value;

            if (!int.TryParse(invKey, out int itemIdInt)) {
                _itemSlots[slotIndex].SetEmpty();
                slotIndex++;
                continue;
            }

            var data = _itemTable.GetItem(itemIdInt);
            if (data != null && count > 0) {
                _itemSlots[slotIndex].SetItem(data, count);
            }
            else {
                _itemSlots[slotIndex].SetEmpty();
            }
            slotIndex++;
        }

        for (; slotIndex < _itemSlots.Count; slotIndex++) {
            _itemSlots[slotIndex].SetEmpty();
        }

        _selectedSlot = null;
        _itemUseScrollbar.value = 0;
        _useCount = 1;
        UpdateUseButtonInteractable();
    }

    private void OnItemChanged(string itemIdKey, int count)
    {
        if (!_itemTableReady) return;

        if (!int.TryParse(itemIdKey, out int itemIdInt)) {
            // 문자열 키는 슬롯에서 제거 시도
            for (int i = 0; i < _itemSlots.Count; i++) {
                var slot = _itemSlots[i];
                if (slot.Data != null && slot.Data.Id.ToString() == itemIdKey) {
                    slot.SetEmpty();
                    if (_selectedSlot == slot) {
                        _selectedSlot = null;
                        _itemUseScrollbar.value = 0;
                        _useCount = 1;
                    }
                    UpdateUseButtonInteractable();
                    return;
                }
            }
            return;
        }

        for (int i = 0; i < _itemSlots.Count; i++) {
            var slot = _itemSlots[i];
            if (slot.Data != null && slot.Data.Id == itemIdInt) {
                if (count > 0) {
                    slot.SetItem(slot.Data, count);
                }
                else {
                    slot.SetEmpty();
                    if (_selectedSlot == slot) {
                        _selectedSlot = null;
                        _itemUseScrollbar.value = 0;
                        _useCount = 1;
                    }
                }
                UpdateUseButtonInteractable();
                return;
            }
        }

        if (count > 0) {
            var data = _itemTable.GetItem(itemIdInt);
            if (data == null) return;

            foreach (var slot in _itemSlots) {
                if (slot.Data == null) {
                    slot.SetItem(data, count);
                    break;
                }
            }
        }

        UpdateUseButtonInteractable();
    }

    private void OnItemSlotClicked(ItemSlot slot)
    {
        if (slot == null || slot.Data == null || slot.Amount <= 0) return;

        _selectedSlot = slot;
        _useCount = 1;
        _itemUseScrollbar.value = 0;
        UpdateUseButtonInteractable();
    }

    private void OnScrollbarChanged(float value)
    {
        if (_selectedSlot == null) return;

        int maxCount = Mathf.Max(1, _selectedSlot.Amount);
        _useCount = Mathf.Max(1, Mathf.RoundToInt(value * maxCount));
        UpdateUseButtonInteractable();
    }

    private void OnClickUseButton()
    {
        if (_selectedSlot == null || _selectedSlot.Data == null) return;

        string invKey = _selectedSlot.Data.Id.ToString();

        if (InventoryManager.Instance.TryUse(invKey, _useCount)) {
            // OnItemChanged에서 갱신됨
        }
        UpdateUseButtonInteractable();
    }

    private void OnClickCloseButton()
    {
        base.SetHide();
    }

    private void UpdateUseButtonInteractable()
    {
        bool canUse = _selectedSlot != null && _selectedSlot.Data != null && _selectedSlot.Amount > 0 && _useCount > 0;
        if (_useButton != null) _useButton.interactable = canUse;
    }
}
