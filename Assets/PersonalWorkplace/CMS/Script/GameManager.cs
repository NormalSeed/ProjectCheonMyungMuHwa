using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using System;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] OfflineRewardDataTableSO table;

    string _uid;
    DatabaseReference _dbRef;
    long exitEPO;
    long currentEPO;
    int stage;

    DateTime exit;
    DateTime current;

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
        Give();
    }
    private async void Give()
    {
        _uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(_uid).Child("rewardTime");
        await LoadStage();
        await SaveCurrentTime();
        await LoadTimes();
        CalculateOffineReward();
        StartCoroutine(ExitTimeRoutine());
    }

    private async Task SaveCurrentTime()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "current", ServerValue.Timestamp }
        };
        await _dbRef.UpdateChildrenAsync(data);
    }
    private async Task LoadTimes()
    {
        DataSnapshot snapshot = await _dbRef.GetValueAsync();

        object obj = snapshot.GetValue(true);
        Dictionary<string, object> timeDict = obj as Dictionary<string, object>;
        currentEPO = long.Parse(timeDict["current"].ToString());
        if (timeDict.ContainsKey("exit"))
        {
            exitEPO = long.Parse(timeDict["exit"].ToString());
        }
        else
        {
            exitEPO = currentEPO - 100000;
        }
        exit = Epoch2Time(exitEPO);
        current = Epoch2Time(currentEPO);
        Debug.Log($"<color=green>마지막 종료시각 : {exit.ToString("yyyy-MM-dd-HH-mm-ss")}</color>");
        Debug.Log($"<color=green>현재 접속시각 : {current.ToString("yyyy-MM-dd-HH-mm-ss")}</color>");
    }
    private async Task LoadStage()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference($"users/{_uid}/stage");
        DataSnapshot snapshot = await dbRef.GetValueAsync();
        object saved = snapshot.GetValue(true);
        if (saved == null)
        {
            stage = 1;
        }
        else
        {
            int savedStage = int.Parse(saved.ToString());
            stage = savedStage == 0 ? 1 : savedStage;
        }
    }
    private DateTime Epoch2Time(long epo)
    {
        DateTime UTC = DateTimeOffset.FromUnixTimeMilliseconds(epo).UtcDateTime;
        DateTime UTC9 = UTC + new TimeSpan(9, 0, 0);
        return UTC9;
        //return UTC_Plus_Nine.ToString("yyyy-MM-dd-HH-mm-ss");
    }

    private IEnumerator ExitTimeRoutine()
    {
        while (true)
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "exit", ServerValue.Timestamp }
            };
            _dbRef.UpdateChildrenAsync(data);
            yield return new WaitForSeconds(60f);
        }
    }

    private void CalculateOffineReward()
    {
        TimeSpan difference = current - exit;
        int totalMin = (int)difference.TotalMinutes;
        if (totalMin < 1) return;
        OfflineRewardData data = table.Table[stage - 1];
        data.Multiply(totalMin);
        Dictionary<CurrencyType, BigCurrency> rewards = new Dictionary<CurrencyType, BigCurrency>();
        if (data.Gold > 0) rewards.Add(CurrencyType.Gold, new BigCurrency(data.Gold));
        if (data.Soul > 0) rewards.Add(CurrencyType.Soul, new BigCurrency(data.Soul));
        if (data.Stone > 0) rewards.Add(CurrencyType.SpiritStone, new BigCurrency(data.Stone));
        if (data.EquipTicket > 0) rewards.Add(CurrencyType.EquipmentSummonTicket, new BigCurrency(data.EquipTicket));
        if (data.HeroTicket > 0) rewards.Add(CurrencyType.SummonTicket, new BigCurrency(data.HeroTicket));
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
                    rewardUI.ShowReward(rewards, totalMin);
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
                    //rewardUI.ShowReward(rewards);
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