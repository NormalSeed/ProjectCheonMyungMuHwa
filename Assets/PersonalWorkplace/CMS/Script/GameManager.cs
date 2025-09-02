using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Offline Reward UI")]
    public GameObject offlinePopup;
    public TextMeshProUGUI rewardText;
    public Button claimButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        LoadOfflineReward();
    }

    private void OnApplicationQuit()
    {
        SaveLastLogoutTime();
    }

    private void SaveLastLogoutTime()
    {
        PlayerPrefs.SetString("LastLogoutTime", DateTime.Now.ToString());
    }

    private void LoadOfflineReward()
    {
        if (PlayerPrefs.HasKey("LastLogoutTime"))
        {
            DateTime lastTime;
            if (DateTime.TryParse(PlayerPrefs.GetString("LastLogoutTime"), out lastTime))
            {
                TimeSpan offlineDuration = DateTime.Now - lastTime;

                double rewardPerSecond = 10; // 보상 비율
                double reward = rewardPerSecond * offlineDuration.TotalSeconds;

                if (reward > 0)
                {
                    ShowOfflineRewardPopup(reward);
                }
            }
        }
    }

    private void ShowOfflineRewardPopup(double reward)
    {
        offlinePopup.SetActive(true);
        rewardText.text = $"{reward:N0} 골드";

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            PlayerDataManager.Instance.Gold += reward;
            offlinePopup.SetActive(false);
        });
    }
}