using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class SlotAutoLayout : MonoBehaviour
{
    [Header("References (assign in inspector)")]
    public RectTransform background;   // Background (Image) - contains Icon
    public RectTransform icon;         // Icon inside background
    public TextMeshProUGUI dayText;
    public RectTransform claimedMark;  // ClaimedMark overlay

    [Header("Defaults")]
    public Vector2 backgroundPreferredSize = new Vector2(120, 120);
    public Vector2 iconSize = new Vector2(96, 96);

    void Reset()
    {
        SetupLayout();
    }

    void Awake()
    {
        SetupLayout();
        SetupBackgroundAndIcon();
        SetupClaimedMark();
    }

    void SetupLayout()
    {
        var v = GetComponent<VerticalLayoutGroup>();
        if (v == null) v = gameObject.AddComponent<VerticalLayoutGroup>();

        v.spacing = 6f;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlHeight = false; // background defines height
        v.childControlWidth = true;
        v.childForceExpandHeight = false;
        v.childForceExpandWidth = false;
    }

    void SetupBackgroundAndIcon()
    {
        if (background != null)
        {
            var le = background.GetComponent<LayoutElement>();
            if (le == null) le = background.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = backgroundPreferredSize.x;
            le.preferredHeight = backgroundPreferredSize.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // 아이콘 중앙 정렬, 크기 설정
            if (icon != null)
            {
                icon.anchorMin = new Vector2(0.5f, 0.5f);
                icon.anchorMax = new Vector2(0.5f, 0.5f);
                icon.pivot = new Vector2(0.5f, 0.5f);
                icon.sizeDelta = iconSize;
                icon.anchoredPosition = Vector2.zero;
            }
        }
    }

    void SetupClaimedMark()
    {
        if (claimedMark == null) return;

        // 레이아웃에서 무시
        var le = claimedMark.GetComponent<LayoutElement>();
        if (le == null) le = claimedMark.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        // overlay (슬롯 전체 덮기)
        claimedMark.SetAsLastSibling();
        claimedMark.anchorMin = Vector2.zero;
        claimedMark.anchorMax = Vector2.one;
        claimedMark.offsetMin = Vector2.zero;
        claimedMark.offsetMax = Vector2.zero;
    }
}