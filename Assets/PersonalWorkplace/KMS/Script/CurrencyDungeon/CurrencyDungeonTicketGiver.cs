using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using System;
using System.Collections;

// current : 현재 접속 시각 기록용
// last : 마지막 접속 시각
public class CurrencyDungeonTicketGiver : MonoBehaviour
{
    string _uid;
    DatabaseReference _dbRef;
    long lastEPO;
    long currentEPO;

    DateTime last;
    DateTime current;
    void Awake()
    {
        Give();
        //Reset();

    }

    private async void Give()
    {
        _uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(_uid).Child("loginTime");
        await SaveCurrentTime();
        await LoadTimes();
        await SaveLastTime();
        if (Compare())
        {
            StartCoroutine(AddTicket());
            Debug.Log($"<color=green>티켓 지급됨</color>");
        }
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
        if (timeDict.ContainsKey("last"))
        {
            lastEPO = long.Parse(timeDict["last"].ToString());
        }
        else
        {
            lastEPO = 0;
        }
        currentEPO = long.Parse(timeDict["current"].ToString());
        last = Epoch2Time(lastEPO);
        current = Epoch2Time(currentEPO);
        Debug.Log($"<color=green>마지막 접속 : {last.ToString("yyyy-MM-dd-HH-mm-ss")}</color>");
        Debug.Log($"<color=green>현재 접속 : {current.ToString("yyyy-MM-dd-HH-mm-ss")}</color>");
    }

    private async Task SaveLastTime()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "last", currentEPO }
        };
        await _dbRef.UpdateChildrenAsync(data);
    }

    // 매 시각 상 02:00:00 마다 보상 지급 초기화 (24시간)
    private bool Compare2()
    {
        while (last <= current)
        {
            last += new TimeSpan(1, 0, 0);
            if (last.Hour == 2)
            {
                //조건 성립
                return true;
            }
        }
        return false;
    }

    // 매 시각 상 5분 마다 보상 지급 초기화
    private bool Compare()
    {
        while (last <= current)
        {
            last += new TimeSpan(0, 1, 0);
            if (last.Minute % 5 == 0)
            {
                //조건 성립
                return true;
            }
        }
        return false;
    }

    private IEnumerator AddTicket()
    {
        int amount = 3;
        yield return new WaitUntil(() => CurrencyManager.Instance != null);
        CurrencyManager manager = CurrencyManager.Instance;
        BigCurrency gold = manager.Get(CurrencyType.GoldChallengeTicket);
        if (gold.Value < amount)
        {
            manager.Set(CurrencyType.GoldChallengeTicket, new BigCurrency(amount));
        }
        BigCurrency soul = manager.Get(CurrencyType.SoulChallengeTicket);
        if (soul.Value < amount)
        {
            manager.Set(CurrencyType.SoulChallengeTicket, new BigCurrency(amount));
        }
        BigCurrency spirit = manager.Get(CurrencyType.SpiritStoneChallengeTicket);
        if (spirit.Value < amount)
        {
            manager.Set(CurrencyType.SpiritStoneChallengeTicket, new BigCurrency(amount));
        }


    }
    public async void Reset()
    {
        _uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(_uid).Child("loginTime");
        await _dbRef.RemoveValueAsync();
    }
    private DateTime Epoch2Time(long epo)
    {
        DateTime UTC = DateTimeOffset.FromUnixTimeMilliseconds(epo).UtcDateTime;
        DateTime UTC9 = UTC + new TimeSpan(9, 0, 0);
        return UTC9;
        //return UTC_Plus_Nine.ToString("yyyy-MM-dd-HH-mm-ss");
    }

}
