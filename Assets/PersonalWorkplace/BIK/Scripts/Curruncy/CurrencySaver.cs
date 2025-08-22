using Firebase.Auth;
using Firebase.Database;
using System;
using UnityEngine;
using VContainer.Unity;

public class CurrencySaver : IStartable, IDisposable
{
    private readonly ICurrencyModel _model;
    private readonly string _uid;
    private readonly DatabaseReference _dbRef;

    public CurrencySaver(ICurrencyModel model)
    {
        _model = model;
        _uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test"; // TODO : 삭제
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Start()
    {
        _model.OnChanged += Save;
    }

    private void Save(CurrencyId id, BigCurrency value)
    {
        if (string.IsNullOrEmpty(_uid)) return;

        var data = ((CurrencyModel)_model).ToSaveData();
        string json = JsonUtility.ToJson(data);

        _dbRef.Child("users").Child(_uid).Child("currency").SetRawJsonValueAsync(json);
    }

    public void Dispose()
    {
        _model.OnChanged -= Save;
    }
}
