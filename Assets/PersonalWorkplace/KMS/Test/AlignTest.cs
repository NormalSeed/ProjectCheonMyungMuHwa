using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlignTest : MonoBehaviour
{

    [SerializeField] RectTransform canversTrs;
    [SerializeField] RectTransform contentsTrs;

    [SerializeField] GameObject[] cards;
    [SerializeField] Scrollbar scrollbar;


    private List<RectTransform> centers = new();
    [SerializeField] int select;

    void Awake()
    {
        foreach (GameObject c in cards)
        {
            centers.Add(c.GetComponentInChildren<RectTransform>());
        }
    }

    public void Align()
    {
        float canvWidth = canversTrs.rect.width;
        float contWidth = contentsTrs.rect.width;
        float wDelta = contWidth - canvWidth;
        float canvCenter = canvWidth / 2;
        float targetPos = centers[select].localPosition.x;
        float distance = targetPos - canvCenter;
        float scrollValue = Mathf.Clamp(distance / wDelta, 0, 1);
        scrollbar.value = scrollValue;
    }




}
