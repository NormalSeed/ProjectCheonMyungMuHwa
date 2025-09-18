using System;
using GoogleMobileAds.Api;
using Unity.VisualScripting;
using UnityEngine;

public class AdLoader : MonoBehaviour
{
    private static bool IsInitialized;
    [SerializeField] AdDataSO adData;
    void Awake()
    {
        if (IsInitialized) Destroy(gameObject);
        Init();
        IsInitialized = true;
    }

    private void Init()
    {
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                return;
            }
            adData.LoadInterstitial();
            adData.LoadReward();
        });
    }
}

