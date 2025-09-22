using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttendanceUIManager : MonoBehaviour
{
    [Header("Prefab & Parent")]
    [SerializeField] private AttendanceSlot slotPrefab;
    [SerializeField] private Transform slotParent;

    [Header("UI")]
    [SerializeField] private Button closeButton;

    private List<AttendanceSlot> slots = new();

    private async void OnEnable()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        AttendanceManager.Instance.OnAttendanceUpdated += SetupSlots;
        await ServerTimeManager.GetServerTime(); // 서버 시간 초기화 용
        SetupSlots();
    }

    private void OnDisable()
    {
        AttendanceManager.Instance.OnAttendanceUpdated -= SetupSlots;
    }

    private async void SetupSlots()
    {
        foreach (var s in slots) Destroy(s.gameObject);
        slots.Clear();

        DateTime serverDate = await ServerTimeManager.GetServerTime();
        int todayIndex = AttendanceManager.Instance.GetTodayIndex(serverDate);

        for (int day = 1; day <= 14; day++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            var reward = AttendanceManager.Instance.GetRewardForDay(day);
            bool isToday = (day == todayIndex);
            bool isClaimed = await AttendanceManager.Instance.IsClaimed(day);

            slot.SetData(day, reward, isToday, isClaimed, async () =>
            {
                await AttendanceManager.Instance.ClaimTodayReward();
            });

            slots.Add(slot);
        }
    }
}