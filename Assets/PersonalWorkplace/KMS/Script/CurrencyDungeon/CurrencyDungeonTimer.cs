using System;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class CurrencyDungeonTimer : MonoBehaviour
{
    [SerializeField] float startTime;

    [SerializeField] TMP_Text timeText;

    private float currentTime;

    private bool isStopped;

    public Action OnTimeOver;


    void Start()
    {
        isStopped = false;
        currentTime = startTime;
        timeText.text = GetTimeFormet();
    }

    void Update()
    {
        if (isStopped) return;
        if (currentTime <= 0)
        {
            timeText.text = "타임 오버";
            OnTimeOver?.Invoke();
            return;
        }
        currentTime -= Time.deltaTime;
        timeText.text = GetTimeFormet();
    }

    private string GetTimeFormet()
    {
        int min = (int)(currentTime / 60);
        float sec = currentTime % 60;
        string form = $"{min.ToString("D2")}:{sec.ToString("00.00")}";
        return form;

    }

    public void Stop()
    {
        isStopped = true;
    }

}
