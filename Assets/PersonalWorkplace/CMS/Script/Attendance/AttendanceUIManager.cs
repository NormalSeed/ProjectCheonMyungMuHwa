using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class AttendanceUIManager : MonoBehaviour
{
    public static AttendanceUIManager Instance;

    [Header("UI Elements")]
    public GameObject panel;
    public Transform slotParent;
    public AttendanceSlot slotPrefab;
    public Button closeButton;
    public TextMeshProUGUI titleText;

    private List<AttendanceSlot> slots = new();
    private int totalDays = 14;

    private TableManager _tableManager;

    [Inject]
    public void Construct(TableManager tableManager)
    {
        _tableManager = tableManager;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    private void Start()
    {
        closeButton.onClick.AddListener(() => panel.SetActive(false));
    }

    public void ShowUI()
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            Debug.Log("[AttendanceUIManager] Panel 활성화됨");
        }
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (slots.Count == 0)
        {
            for (int i = 0; i < totalDays; i++)
            {
                var slotObj = Instantiate(slotPrefab, slotParent);
                slots.Add(slotObj);
                Debug.Log($"[AttendanceUIManager] 슬롯 {slots.Count}개 생성됨");
            }
        }

        int currentDay = AttendanceManager.Instance.GetCurrentAttendanceDay();
        Debug.Log($"[AttendanceUIManager] 현재 출석일: {currentDay}");
        string lastDate = PlayerPrefs.GetString("LastAttendanceDate", "");
        string todayKey = System.DateTime.Now.ToString("yyyyMMdd"); // 간단한 비교용. 실제 시간은 서버시간 기준으로.
        bool hasClaimedToday = lastDate == todayKey;

        for (int i = 0; i < totalDays; i++)
        {
            int day = i + 1;
            var rewardData = AttendanceManager.Instance.GetRewardForDay(day);
            var slot = slots[i];

            bool isClaimed = day < currentDay || (day == currentDay && hasClaimedToday);
            bool isToday = day == currentDay;

            slot.SetData(rewardData, day, isToday, isClaimed, _tableManager);

            bool adButtonActive = false;
            if (day == 7)
            {
                // 7일차 광고는 현재 출석일이 7일 이상이고, 아직 보상을 받지 않았다면 활성화
                if (currentDay >= 7 && !AttendanceManager.Instance.IsAdRewardClaimed(7))
                {
                    adButtonActive = true;
                }
            }
            else if (day == 14)
            {
                // 14일차 광고는 정확히 14일차이고, 기본 보상을 받았고, 아직 광고 보상을 받지 않았다면 활성화
                if (currentDay == 14 && hasClaimedToday && !AttendanceManager.Instance.IsAdRewardClaimed(14))
                {
                    adButtonActive = true;
                }
            }

            slot.SetAdButtonActive(adButtonActive);
        }
    }
}