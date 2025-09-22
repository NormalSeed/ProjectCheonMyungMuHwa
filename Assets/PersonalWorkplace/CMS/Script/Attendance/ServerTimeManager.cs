using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ServerTimeManager
{
    private static DatabaseReference _dbRef => FirebaseDatabase.DefaultInstance.RootReference;

    public static async Task<DateTime> GetServerTime()
    {
        var timeRef = _dbRef.Child("serverTime");
        await timeRef.SetValueAsync(ServerValue.Timestamp); // 서버에서 현재 ms 저장
        var snapshot = await timeRef.GetValueAsync();

        if (snapshot.Exists && long.TryParse(snapshot.Value.ToString(), out long ms))
        {
            DateTime serverTime = DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime;
            return serverTime;
        }

        Debug.LogError("서버 시간 불러오기 실패, 로컬 시간으로 대체");
        return DateTime.UtcNow; // fallback
    }
}
