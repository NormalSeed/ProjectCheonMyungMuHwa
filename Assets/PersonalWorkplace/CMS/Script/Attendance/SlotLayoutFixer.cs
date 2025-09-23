using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SlotLayoutFixer : MonoBehaviour
{
    void Awake()
    {
        // 이미 VerticalLayoutGroup이 붙어 있다면 중복 방지
        var layout = GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = gameObject.AddComponent<VerticalLayoutGroup>();

        // 레이아웃 기본 설정
        layout.childAlignment = TextAnchor.MiddleCenter; // 위-아래 중앙 정렬
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 5f; // 요소 간격

        // ContentSizeFitter 추가 (자동 크기 맞춤)
        var fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}