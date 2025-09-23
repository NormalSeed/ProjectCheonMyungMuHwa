using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlignTest : MonoBehaviour
{

    [SerializeField] bool clockwise;
    [SerializeField] float defaultDPS;
    [SerializeField] Transform roulette;
    [SerializeField] float targetDeg;
    [SerializeField] int samplingCount;

    private Vector3 axis;
    private bool isStarted;
    private float targetRotate;
    private List<float> sample;
    private float sum = 0;
    private float average = 0;
    private float timer = 0;
    private float speed = 0;
    private int index = 0;

    void Awake()
    {
        sample = new();
        axis = clockwise ? Vector3.back : Vector3.forward;
        int a = Random.Range(12, 15);
        targetRotate = clockwise ? targetDeg : 360 - targetDeg;
        targetRotate += 360 * a;
        Sampling();
        GetAverage();
    }
    void Update()
    {
        if (!isStarted) return;
        if (targetRotate <= 0) return;
        if (timer <= 0)
        {
            timer = average;
            if (index < samplingCount)
            {
                speed = sample[index];
                index++;
            }
            else
            {
                speed *= 0.6f; // 샘플링 값 다 썼는데도 안끝날 경우..
            }
        }
        float delta = Time.deltaTime;
        float temp = timer - delta;
        float rotate = speed * defaultDPS;
        if (temp < 0)
        {
            rotate *= timer;
        }
        else
        {
            rotate *= delta;
        }
        roulette.Rotate(axis, rotate);
        targetRotate -= rotate;
        timer = temp;
    }

    public void Start()
    {
        isStarted = true;
    }

    private void Sampling()
    {
        for (int i = 0; i < samplingCount; i++)
        {
            float val = Function((float)i / samplingCount);
            sum += val;
            sample.Add(val);
        }
    }
    private float Function(float x)
    {
        return Mathf.Pow(0.002f, 1.2f * x) * 500;
    }
    private void GetAverage()
    {
        average = targetRotate / sum / defaultDPS;
    }
}
