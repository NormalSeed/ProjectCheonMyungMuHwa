using System;
using UnityEngine;

public class StageBarFill : MonoBehaviour
{
    [SerializeField] float totalWidth;

    RectTransform rect => transform as RectTransform;

    public void SetValue(float value)
    {
        rect.sizeDelta = new Vector2(value * totalWidth, 0);
    }
    public void AddValue(float value)
    {
        rect.sizeDelta += new Vector2(value * totalWidth, 0);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Inactivate()
    {
        gameObject.SetActive(false);
    }
}
