using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttendanceUIManager : MonoBehaviour
{
    public static AttendanceUIManager Instance;

    [Header("UI Elements")]
    public GameObject panel;                 // 출석 UI 전체 패널
    public Transform slotParent;             // 슬롯들이 붙을 부모
    public AttendanceSlot slotPrefab;        // 슬롯 프리팹
    public Button closeButton;               // 닫기 버튼
    public TextMeshProUGUI titleText;        // "출석 체크" 제목

    private List<AttendanceSlot> slots = new();
    private int totalDays = 14;

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

    /// <summary> 출석 UI 표시 및 슬롯 갱신 </summary>
    public async void ShowUI()
    {
        panel.SetActive(true);

        // 서버 시간 기준 오늘 몇일차인지 가져오기
        var serverTime = await ServerTimeManager.GetServerTime();
        int todayIndex = AttendanceManager.Instance.GetRewardForDayIndex(serverTime);

        // 마지막 수령한 날짜 확인
        string todayKey = serverTime.ToString("yyyyMMdd");
        string lastDate = PlayerPrefs.GetString("LastAttendanceDate", "");

        RefreshSlots(todayIndex, todayKey == lastDate);
    }

    /// <summary> 슬롯 생성 및 상태 갱신 </summary>
    private void RefreshSlots(int todayIndex, bool alreadyClaimed)
    {
        // 슬롯 없으면 생성
        if (slots.Count == 0)
        {
            for (int i = 1; i <= totalDays; i++)
            {
                var slotObj = Instantiate(slotPrefab, slotParent);
                slots.Add(slotObj);
            }
        }

        // 각 슬롯 갱신
        for (int i = 0; i < totalDays; i++)
        {
            int day = i + 1;
            var rewardData = AttendanceManager.Instance.GetRewardForDay(day);
            var slot = slots[i];

            bool isToday = (day == todayIndex);
            bool isClaimed = (day < todayIndex) || (isToday && alreadyClaimed);

            slot.SetData(rewardData, day, isToday, isClaimed);

            // 7일차/14일차 → 금색 테두리 강조
            if (day % 7 == 0)
                slot.HighlightAsSpecial();
            else
                slot.ResetHighlight();

            // 광고 버튼은 7일차에만 표시
            if (day == 7 || day == 14)
            {
                slot.SetAdButtonActive(isToday && alreadyClaimed);
            }
            else
            {
                slot.SetAdButtonActive(false);
            }
        }
    }
}