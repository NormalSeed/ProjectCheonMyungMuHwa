using Firebase.Auth;
using Firebase.Database;
using Google.MiniJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;

public class CurrencyManager : IStartable, IDisposable
{
    #region Singleton

    public static CurrencyManager Instance { get; private set; }

    #endregion // Singleton





    #region private fields

    private readonly ICurrencyModel _model;
    private readonly DatabaseReference _dbRef;
    private readonly string _uid;

    private bool _initialized = false;

    #endregion // private fields





    #region properties

    public ICurrencyModel Model => _model;

    public static event Action OnInitialized;
    #endregion // properties





    #region // constructor

    public CurrencyManager(ICurrencyModel model)
    {
        Instance = this;

        _model = model;
        _uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    #endregion // constructor





    #region mono funcs

    public void Start()
    {
        LoadFromFirebase();
    }

    #endregion // mono funcs





    #region private funcs

    private async void LoadFromFirebase()
    {
        if (string.IsNullOrEmpty(_uid)) return;

        try
        {
            var snapshot = await _dbRef.Child("users").Child(_uid).Child("currency").GetValueAsync();

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                var data = JsonUtility.FromJson<CurrencySaveData>(json);
                ((CurrencyModel)_model).FromSaveData(data);

                Debug.Log("[CurrencyManager] 서버에서 재화 로드 완료");
            }
            else
            {
                Debug.Log("[CurrencyManager] 서버에 데이터 없음 → 기본값 등록");
                RegisterDefaultCurrencies();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CurrencyManager] Firebase 로드 실패: {ex.Message}");
            RegisterDefaultCurrencies();
        }

        _model.OnChanged += SaveToFirebase;
        _initialized = true;
        OnInitialized?.Invoke();
    }

    private void SaveToFirebase(CurrencyType id, BigCurrency value)
    {
        if (!_initialized || string.IsNullOrEmpty(_uid)) return;

        var data = ((CurrencyModel)_model).ToSaveData();
        string json = JsonUtility.ToJson(data);

        _dbRef.Child("users").Child(_uid).Child("currency").SetRawJsonValueAsync(json);
        Debug.Log($"[CurrencyManager] 저장됨: {id}={value}");
    }

    private void RegisterDefaultCurrencies()
    {
        _model.Set(CurrencyType.Jewel, new BigCurrency(0, 0));
        _model.Set(CurrencyType.Gold, new BigCurrency(0, 0));
        _model.Set(CurrencyType.Soul, new BigCurrency(0, 0));
        _model.Set(CurrencyType.SpiritStone, new BigCurrency(0, 0));
        _model.Set(CurrencyType.SummonTicket, new BigCurrency(0, 0));
        _model.Set(CurrencyType.InvitationTicket, new BigCurrency(0, 0));
        _model.Set(CurrencyType.ChallengeTicket, new BigCurrency(0, 0));
    }

    #endregion // private funcs





    #region public funcs

    public void Dispose()
    {
        _model.OnChanged -= SaveToFirebase;
    }

    public BigCurrency Get(CurrencyType id)
    {
        return _model.Get(id);
    }

    public void Set(CurrencyType id, BigCurrency value)
    {
        _model.Set(id, value);
    }

    public void Add(CurrencyType id, BigCurrency delta)
    {
        _model.Add(id, delta);
    }

    public bool TrySpend(CurrencyType id, BigCurrency cost)
    {
        return _model.TrySpend(id, cost);
    }

    public void SavePartyToFirebase(List<string> party)
    {
        if (party == null)
        {
            Debug.LogError("파티가 Null임");
        }

        if (string.IsNullOrEmpty(_uid))
            return;

        _dbRef.Child("users").Child(_uid).Child("charator").Child("partyInfo").SetValueAsync(party);
    }
    public void LoadPartyIdsFromFirebase(List<string> list)
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        _dbRef.Child("users").Child(_uid).Child("charator").Child("partyInfo")
            .GetValueAsync().ContinueWith(task => 
            {
                list.Clear();
                var raw = task.Result.Value as List<object>;
                if (raw != null)
                {
                    foreach (var obj in raw)
                        list.Add(obj.ToString());
                }
            });
    }
    #endregion // public funcs
}
