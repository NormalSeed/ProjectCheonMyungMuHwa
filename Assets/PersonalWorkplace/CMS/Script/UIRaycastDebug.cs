using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIRaycastDebug : MonoBehaviour
{
    public GraphicRaycaster raycaster; // ConfirmPopup Canvas의 GraphicRaycaster 할당
    private PointerEventData pointerData;
    private List<RaycastResult> results = new List<RaycastResult>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem 없음!");
                return;
            }
            pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            results.Clear();
            raycaster.Raycast(pointerData, results);

            Debug.Log($"--- Raycast 결과 ({results.Count}) pos={Input.mousePosition} ---");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Debug.Log($"{i}: GO='{r.gameObject.name}', module={r.module}, index={r.sortingLayer}, depth={r.depth}");
            }
        }
    }
}