using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : UIBase
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button closeButton; //

    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        SetHide();
    }

    public void SetShow(string title, string message, Action onConfirm, Action onCancel = null)
    {
        Debug.Log("[ConfirmPopup] SetShow 호출: " + title);

        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        titleText.text = title;
        messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners(); 

        yesButton.onClick.AddListener(() =>
        {
            Debug.Log("[ConfirmPopup] yes 클릭");
            onConfirm?.Invoke();
            SetHide();
        });

        noButton.onClick.AddListener(() =>
        {
            Debug.Log("[ConfirmPopup] no 클릭");
            onCancel?.Invoke();
            SetHide();
        });

        closeButton.onClick.AddListener(() =>
        {
            Debug.Log("[ConfirmPopup] 닫기 버튼 클릭");
            onCancel?.Invoke(); // 닫기를 취소로 처리
            SetHide();
        });

        // 최상단 보장
        transform.SetAsLastSibling();

        // CanvasGroup 강제 활성화
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        base.SetShow();
    }
}