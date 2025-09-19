using System.Collections.Generic;
using UnityEngine;

public class AttendanceManager : MonoBehaviour
{
    public AttendanceCSVLoader csvLoader;
    private List<AttendanceReward> rewardTable;

    private void Start()
    {
        rewardTable = csvLoader.LoadRewards();
    }

    public AttendanceReward GetRewardForDay(int day)
    {
        return rewardTable.Find(r => r.day == day);
    }
}
