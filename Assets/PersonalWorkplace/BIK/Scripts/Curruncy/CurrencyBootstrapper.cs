using Firebase.Auth;
using Firebase.Database;
using System;
using UnityEngine;
using VContainer.Unity;

public class CurrencyBootstrapper : IStartable
{
    private readonly ICurrencyModel _model;
    private readonly IGameCurrencyController _controller;

    public CurrencyBootstrapper(ICurrencyModel model, IGameCurrencyController controller)
    {
        _model = model;
        _controller = controller;
    }

    public void Start()
    {
        LoadCurrencyFromFirebase();
    }

    private async void LoadCurrencyFromFirebase()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        var db = FirebaseDatabase.DefaultInstance;

        try {
            var snapshot = await db.GetReference($"users/{uid}/currency").GetValueAsync();

            if (snapshot.Exists) {
                string json = snapshot.GetRawJsonValue();
                var data = JsonUtility.FromJson<CurrencySaveData>(json);
                ((CurrencyModel)_model).FromSaveData(data);
                Debug.Log("[CurrencyBootstrapper] 서버에서 재화 데이터 로드 완료");
            }
            else {
                Debug.Log("[CurrencyBootstrapper] 서버에 재화 데이터 없음. 기본값 설정");
                RegisterDefaultCurrencies();
            }
        }
        catch (Exception ex) {
            Debug.LogError($"[CurrencyBootstrapper] 재화 로드 실패: {ex.Message}");
            RegisterDefaultCurrencies();
        }
    }

    private void RegisterDefaultCurrencies()
    {
        _model.Set(CurrencyIds.Jewel, new BigCurrency(0, 0));
        _model.Set(CurrencyIds.Gold, new BigCurrency(0, 0));
        _model.Set(CurrencyIds.Soul, new BigCurrency(0, 0));
        _model.Set(CurrencyIds.SpiritStone, new BigCurrency(0, 0));
        _model.Set(CurrencyIds.SummonTicket, new BigCurrency(0, 0));
    }
}
