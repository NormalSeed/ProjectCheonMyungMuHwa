using System;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class CurrencyDungeonTimer : MonoBehaviour
{
    [SerializeField] float startTime;

    [SerializeField] TMP_Text timeText;

    [SerializeField] StageBarFill fill;

    private float currentTime;

    private bool isStopped;

    public Action OnTimeOver;

    void OnEnable()
    {
        isStopped = false;
        currentTime = startTime;
        fill?.SetValue(1);
        //timeText.text = GetTimeFormet();
    }

    void Update()
    {
        if (isStopped) return;
        if (currentTime <= 0)
        {
            //timeText.text = "타임 오버";
            fill?.SetValue(0);
            OnTimeOver?.Invoke();
            isStopped = true;
            return;
        }
        currentTime -= Time.deltaTime;
        fill?.SetValue(currentTime / startTime);
        //timeText.text = GetTimeFormet();
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
    public void Inactivate()
    {
        gameObject.SetActive(false);
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }

}
