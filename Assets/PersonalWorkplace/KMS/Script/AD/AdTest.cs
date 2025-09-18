using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] AdDataSO data;


    public void OnClickInterstitial()
    {
        data.ShowInterstitialAD(() => Debug.Log("<color=green> 납치 광고 종료됨</color>"));
    }
    public void OnClickReward()
    {
        data.ShowRewardAD(() => Debug.Log("<color=green> 보상 광고 끝까지 봄</color>"));
    }

}
