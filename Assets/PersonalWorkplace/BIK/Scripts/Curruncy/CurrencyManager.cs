using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public bool IsInitialized => _initialized;
    public DatabaseReference DbRef { get { return _dbRef; } }
    public string UserID { get { return _uid; } }

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

        try {
            var snapshot = await _dbRef.Child("users").Child(_uid).Child("currency").GetValueAsync();

            if (snapshot.Exists) {
                string json = snapshot.GetRawJsonValue();
                var data = JsonUtility.FromJson<CurrencySaveData>(json);
                ((CurrencyModel)_model).FromSaveData(data);

                Debug.Log("[CurrencyManager] 서버에서 재화 로드 완료");
            }
            else {
                Debug.Log("[CurrencyManager] 서버에 데이터 없음 → 기본값 등록");
                RegisterDefaultCurrencies();
            }
        }
        catch (Exception ex) {
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
    #endregion // public funcs


    #region MSK add
    /// <summary>
    /// 파티 편성정보 저장
    /// </summary>
    /// <param name="party"></param>
    public void SavePartyToFirebase(List<string> party)
    {
        if (string.IsNullOrEmpty(_uid))
            return;

        var partyInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("partyInfo");

        //  기존 저장된 리스트 삭제
        partyInfoRef.RemoveValueAsync().ContinueWith(removeTask => {
            if (removeTask.IsFaulted)
                return;
            // 새로운 리스트 저장
            partyInfoRef.SetValueAsync(party).ContinueWith(setTask => {
                if (setTask.IsFaulted) { }
            });
        });
    }

    /// <summary>
    /// 파티 편성정보 로딩
    /// </summary>
    /// <param name="list"></param>
    public void LoadPartyIdsFromFirebase(List<string> list)
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        _dbRef.Child("users").Child(_uid).Child("character").Child("partyInfo")
            .GetValueAsync().ContinueWith(task => {
                list.Clear();
                var raw = task.Result.Value as List<object>;
                if (raw != null) {
                    foreach (var obj in raw)
                        list.Add(obj.ToString());
                }
            });
    }

    /// <summary>
    /// 캐릭터 성장정보 저장
    /// </summary>
    /// <param name="chariID"></param>
    /// <param name="level"></param>
    public void SaveCharacterInfoToFireBase(string chariID, int level)
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        var partyInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(chariID);
        partyInfoRef.Child("level").SetValueAsync(level).ContinueWith(task => {
            if (task.IsFaulted)
                return;
        });
    }

    /// <summary>
    /// 캐릭터 성장정보 로딩
    /// </summary>
    public async Task<int> LoadCharacterInfoFromFireBase(string chariID)
    {
        int level = -1;
        if (string.IsNullOrEmpty(_uid))
            return level;

        var partyInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(chariID);
        var dataSnapshot = await partyInfoRef.Child("level").GetValueAsync();

        if (dataSnapshot.Exists)
            int.TryParse(dataSnapshot.Value.ToString(), out level);
        return level;
    }

    /// <summary>
    /// 캐릭터 돌파에 필요한 조각개수 저장
    /// </summary>
    /// <param name="piece"></param>
    public void SaveHeroPieceToFireBase(string chariID, int piece)
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        var partyInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(chariID);
        partyInfoRef.Child("piece").SetValueAsync(piece).ContinueWith(task =>
        {
            if (task.IsFaulted)
                return;
        });
    }

    /// <summary>
    /// 캐릭터 돌파에 필요한 조각 개수 불러오기
    /// </summary>
    public void LoadHeroPieceFromFireBase(string chariID)
    {
        if (string.IsNullOrEmpty(_uid))
            return;
        var partyInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo").Child(chariID);

    }
    #endregion
}
