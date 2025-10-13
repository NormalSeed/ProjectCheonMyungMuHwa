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

    // Realtime listeners
    private DatabaseReference _invRef;
    private bool _realtimeSubscribed = false;
    #endregion

    #region properties
    public bool IsInitialized => _initialized;
    public DatabaseReference DbRef => _dbRef;
    public string UserID => _uid;
    public IReadOnlyDictionary<string, int> Items => _items;
    #endregion

    #region events
    public static event Action OnInitialized;
    public event Action<string, int> OnItemChanged; // (아이템ID, 변경 후 개수)
    public event Action OnBulkSynced;               // 스냅샷 전체 동기화
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
                    if (int.TryParse(child.Value?.ToString(), out int count)) {
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
        RunOnMainThread(() => OnInitialized?.Invoke());

        // Realtime 구독 시작
        SubscribeRealtime();
    }

    private void SaveToFirebase(string itemId)
    {
        if (string.IsNullOrEmpty(_uid)) return;

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

    // 메인 스레드 디스패치 (Firebase.Extensions 없으면 즉시 호출)
    private void RunOnMainThread(Action a)
    {
#if FIREBASE_EXTENSIONS
        try {
            Firebase.Extensions.MainThreadDispatcher.Post(a);
        } catch {
            a?.Invoke();
        }
#else
        a?.Invoke();
#endif
    }

    // === Realtime 구독/해제 ===
    private void SubscribeRealtime()
    {
        if (_realtimeSubscribed || string.IsNullOrEmpty(_uid)) return;

        _invRef = _dbRef.Child("users").Child(_uid).Child("inventory");

        // 스냅샷 전체 변화(여러 키가 한 번에 바뀌는 경우)
        _invRef.ValueChanged += OnInventoryValueChanged;

        // 개별 키 변화
        _invRef.ChildAdded += OnInventoryChildAdded;
        _invRef.ChildChanged += OnInventoryChildChanged;
        _invRef.ChildRemoved += OnInventoryChildRemoved;

        _realtimeSubscribed = true;
        Debug.Log("[InventoryManager] Realtime 리스너 구독 시작");
    }

    private void UnsubscribeRealtime()
    {
        if (!_realtimeSubscribed || _invRef == null) return;

        _invRef.ValueChanged -= OnInventoryValueChanged;
        _invRef.ChildAdded -= OnInventoryChildAdded;
        _invRef.ChildChanged -= OnInventoryChildChanged;
        _invRef.ChildRemoved -= OnInventoryChildRemoved;

        _realtimeSubscribed = false;
        _invRef = null;
        Debug.Log("[InventoryManager] Realtime 리스너 구독 해제");
    }

    // === RTDB 이벤트 핸들러 ===
    private void OnInventoryValueChanged(object sender, ValueChangedEventArgs e)
    {
        // 스냅샷 전체를 로컬에 반영
        var newDict = new Dictionary<string, int>();

        if (e.Snapshot != null && e.Snapshot.Exists) {
            foreach (var child in e.Snapshot.Children) {
                string itemId = child.Key;
                if (int.TryParse(child.Value?.ToString(), out int count)) {
                    newDict[itemId] = count;
                }
            }
        }

        _items = newDict; // 통째로 교체
        RunOnMainThread(() => OnBulkSynced?.Invoke());
    }

    private void OnInventoryChildAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot == null) return;

        string itemId = e.Snapshot.Key;
        if (!int.TryParse(e.Snapshot.Value?.ToString(), out int count)) return;

        _items[itemId] = count;
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, count));
    }

    private void OnInventoryChildChanged(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot == null) return;

        string itemId = e.Snapshot.Key;
        if (!int.TryParse(e.Snapshot.Value?.ToString(), out int count)) return;

        _items[itemId] = count;
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, count));
    }

    private void OnInventoryChildRemoved(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot == null) return;

        string itemId = e.Snapshot.Key;
        _items.Remove(itemId);
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, 0));
    }
    #endregion

    #region public funcs
    public void Dispose()
    {
        UnsubscribeRealtime();
    }

    public int Get(string itemId)
    {
        return _items.TryGetValue(itemId, out var count) ? count : 0;
    }

    public void Set(string itemId, int value)
    {
        _items[itemId] = Mathf.Max(0, value);
        SaveToFirebase(itemId);
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, _items[itemId]));
    }

    public void Add(string itemId, int delta)
    {
        if (!_items.ContainsKey(itemId)) _items[itemId] = 0;
        _items[itemId] = Mathf.Max(0, _items[itemId] + delta);
        SaveToFirebase(itemId);
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, _items[itemId]));
    }

    public bool TryUse(string itemId, int count)
    {
        if (!_items.ContainsKey(itemId) || _items[itemId] < count)
            return false;

        _items[itemId] -= count;
        SaveToFirebase(itemId);
        RunOnMainThread(() => OnItemChanged?.Invoke(itemId, _items[itemId]));
        return true;
    }
    #endregion
}
