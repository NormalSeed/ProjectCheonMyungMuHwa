using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CheckOfflineReward();
    }

    private void OnApplicationQuit()
    {
        // 종료 시각 저장
        PlayerPrefs.SetString("LastQuitTime", DateTime.Now.ToString());
    }

    private void CheckOfflineReward()
    {
        Debug.Log("[CheckOfflineReward] 실행됨");

        if (!PlayerPrefs.HasKey("LastQuitTime"))
        {
            Debug.Log("[CheckOfflineReward] 저장된 종료 시간이 없음, 보상 없음");
            return;
        }

        DateTime lastQuitTime = DateTime.Parse(PlayerPrefs.GetString("LastQuitTime"));
        TimeSpan offlineTime = DateTime.Now - lastQuitTime;

        Debug.Log($"[CheckOfflineReward] 마지막 종료 시각={lastQuitTime}, 오프라인 시간={offlineTime.TotalSeconds:F1}초");

        if (offlineTime.TotalSeconds < 10)
        {
            Debug.Log("[CheckOfflineReward] 오프라인 시간이 10초 미만, 보상 없음");
            return;
        }

        // --- 오프라인 보상 계산 ---
        Dictionary<CurrencyType, BigCurrency> rewards = new Dictionary<CurrencyType, BigCurrency>();

        double goldPerSecond = 10;
        double totalGold = goldPerSecond * offlineTime.TotalSeconds;
        rewards.Add(CurrencyType.Gold, new BigCurrency(totalGold));

        Debug.Log($"[CheckOfflineReward] 보상 계산 완료: Gold={totalGold}");

        // PopupManager 확인
        if (PopupManager.Instance != null)
        {
            Debug.Log("[CheckOfflineReward] PopupManager.Instance 발견됨");

            if (PopupManager.Instance.TryGetPopup(PopupType.OfflineRewardPopup, out var popup))
            {
                Debug.Log("[CheckOfflineReward] OfflineRewardPopup 찾음");

                if (popup is OfflineRewardUI rewardUI)
                {
                    Debug.Log("[CheckOfflineReward] OfflineRewardUI 캐스팅 성공, ShowReward 호출");
                    popup.SetShow();
                    rewardUI.ShowReward(rewards);
                }
                else
                {
                    Debug.LogError("[CheckOfflineReward] OfflineRewardPopup이 OfflineRewardUI 타입이 아님");
                }
            }
            else
            {
                Debug.LogError("[CheckOfflineReward] PopupManager에서 OfflineRewardPopup 찾기 실패");
            }
        }
        else
        {
            Debug.LogError("[CheckOfflineReward] PopupManager.Instance가 없음");
        }
    }
}