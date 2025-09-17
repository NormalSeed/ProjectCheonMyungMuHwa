using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class InventoryManager : IStartable, IDisposable
{
    #region Singleton

    public static InventoryManager Instance { get; private set; }

    #endregion





    #region private fields

    private readonly DatabaseReference _dbRef;
    private readonly string _uid;

    private Dictionary<string, int> _items = new();
    private bool _initialized = false;

    #endregion





    #region properties

    public bool IsInitialized => _initialized;
    public DatabaseReference DbRef => _dbRef;
    public string UserID => _uid;

    /// <summary> 인벤토리 전체 접근 </summary>
    public IReadOnlyDictionary<string, int> Items => _items;

    #endregion





    #region events

    public static event Action OnInitialized;
    public event Action<string, int> OnItemChanged; // (아이템ID, 변경 후 개수)

    #endregion





    #region constructor

    public InventoryManager()
    {
        Instance = this;

        _uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    #endregion





    #region IStartable

    public void Start()
    {
        LoadFromFirebase();
    }

    #endregion





    #region private funcs

    private async void LoadFromFirebase()
    {
        if (string.IsNullOrEmpty(_uid)) return;

        try {
            var snapshot = await _dbRef.Child("users").Child(_uid).Child("inventory").GetValueAsync();

            if (snapshot.Exists) {
                _items.Clear();
                foreach (var child in snapshot.Children) {
                    string itemId = child.Key;
                    if (int.TryParse(child.Value.ToString(), out int count)) {
                        _items[itemId] = count;
                    }
                }
                Debug.Log("[InventoryManager] 서버에서 인벤토리 로드 완료");
            }
            else {
                Debug.Log("[InventoryManager] 서버에 데이터 없음 → 기본 인벤토리 등록");
                RegisterDefaultInventory();
            }
        }
        catch (Exception ex) {
            Debug.LogError($"[InventoryManager] Firebase 로드 실패: {ex.Message}");
            RegisterDefaultInventory();
        }

        _initialized = true;
        OnInitialized?.Invoke();
    }

    private void SaveToFirebase(string itemId)
    {
        if (!_initialized || string.IsNullOrEmpty(_uid)) return;

        int count = _items.ContainsKey(itemId) ? _items[itemId] : 0;
        _dbRef.Child("users").Child(_uid).Child("inventory").Child(itemId).SetValueAsync(count);
        Debug.Log($"[InventoryManager] 저장됨: {itemId}={count}");
    }

    private void RegisterDefaultInventory()
    {
        _items["NormalChest"] = 0;
        _items["RareChest"] = 0;

        foreach (var kv in _items) {
            SaveToFirebase(kv.Key);
        }
    }

    #endregion





    #region public funcs

    public void Dispose()
    {
        // 현재는 별도 이벤트 구독 없음
    }

    public int Get(string itemId)
    {
        return _items.TryGetValue(itemId, out var count) ? count : 0;
    }

    public void Set(string itemId, int value)
    {
        _items[itemId] = Mathf.Max(0, value);
        SaveToFirebase(itemId);
        OnItemChanged?.Invoke(itemId, _items[itemId]);
    }

    public void Add(string itemId, int delta)
    {
        if (!_items.ContainsKey(itemId)) _items[itemId] = 0;
        _items[itemId] = Mathf.Max(0, _items[itemId] + delta);
        SaveToFirebase(itemId);
        OnItemChanged?.Invoke(itemId, _items[itemId]);
    }

    public bool TryUse(string itemId, int count)
    {
        if (!_items.ContainsKey(itemId) || _items[itemId] < count)
            return false;

        _items[itemId] -= count;
        SaveToFirebase(itemId);
        OnItemChanged?.Invoke(itemId, _items[itemId]);
        return true;
    }
    #endregion
}
