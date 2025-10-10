using System;
using GoogleMobileAds.Api;
using UnityEngine;

[CreateAssetMenu(fileName = "AdDataSO", menuName = "Scriptable Objects/AdDataSO")]
public class AdDataSO : ScriptableObject
{
  InterstitialAd loadedInterstitialAd;
  RewardedAd loadedRewardAd;
  [SerializeField] string interstitial_id;
  [SerializeField] string reward_id;
  public void LoadInterstitial()
  {
    if (loadedInterstitialAd != null) loadedInterstitialAd.Destroy();
    AdRequest request = new AdRequest();
    InterstitialAd.Load(interstitial_id, request, (InterstitialAd ad, LoadAdError error) =>
    {
      if (error != null) return;
      loadedInterstitialAd = ad;
    });
  }
  public void LoadReward()
  {
    if (loadedRewardAd != null) loadedInterstitialAd.Destroy();
    AdRequest request = new AdRequest();
    RewardedAd.Load(interstitial_id, request, (RewardedAd ad, LoadAdError error) =>
    {
      if (error != null) return;
      loadedRewardAd = ad;
      loadedRewardAd.OnAdFullScreenContentClosed += LoadReward;
    });
  }

  public void ShowInterstitialAD(Action onAdClosed)
  {
    if (loadedInterstitialAd == null || !loadedInterstitialAd.CanShowAd())
    {
      LoadInterstitial();
    }
    loadedInterstitialAd.OnAdFullScreenContentClosed += onAdClosed;
    loadedInterstitialAd.OnAdFullScreenContentClosed += LoadInterstitial;
    loadedInterstitialAd?.Show();
  }
  public void ShowRewardAD(Action onAdFinished)
  {
    if (loadedRewardAd == null || !loadedRewardAd.CanShowAd())
    {
      LoadInterstitial();
    }
    loadedRewardAd?.Show(r => onAdFinished?.Invoke());
  }
}
