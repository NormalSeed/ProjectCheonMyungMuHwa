using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public enum SettingUIType { Main, Logout, Delete, Quit }
[Serializable] public struct SettingUIMap { public SettingUIType type; public UIBase ui; }

public class SettingUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button quitButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button linkGoogleButton;
    [SerializeField] private Button closeButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI uidText;

    [SerializeField] SettingUIMap[] uis;

    private Dictionary<SettingUIType, UIBase> dict;

    private string uid;

    void Awake()
    {
        dict = new();
        foreach (var v in uis) dict.Add(v.type, v.ui);
    }


    void OnEnable()
    {
        OpenMain();
    }

    private void Start()
    {
        // UID 표시
        uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "알 수 없음";
        uidText.text = $"UID: {uid}";

        // 종료 버튼
        //quitButton.onClick.AddListener(QuitGame);
        quitButton.onClick.AddListener(() =>
        {
            dict[SettingUIType.Main].SetHide();
            dict[SettingUIType.Quit].SetShow();
        });
        logoutButton.onClick.AddListener(() =>
        {
            dict[SettingUIType.Main].SetHide();
            dict[SettingUIType.Logout].SetShow();
        });
        deleteAccountButton.onClick.AddListener(() =>
        {
            dict[SettingUIType.Main].SetHide();
            dict[SettingUIType.Delete].SetShow();
        });

        //logoutButton.onClick.AddListener(() =>
        //{
        //    ShowConfirmPopup(
        //        "로그아웃",
        //        "정말 로그아웃 하시겠습니까?\n(게스트 계정의 경우 정보가 삭제됩니다)",
        //        () => BackendManager.Instance?.Logout()
        //    );
        //});

        //deleteAccountButton.onClick.AddListener(() =>
        //{
        //    ShowConfirmPopup(
        //        "계정 삭제",
        //        "정말 계정을 삭제하시겠습니까?",
        //        () => BackendManager.Instance?.DeleteAccount(uid)
        //    );
        //});

        linkGoogleButton.onClick.AddListener(() =>
        {
            BackendManager.Instance?.LinkGuestToGoogle();
        });

        closeButton.onClick.AddListener(() =>
        {
            Debug.Log("[SettingUI] 닫기 버튼 클릭됨");
            SetHide();
        });
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void DeleteAccount()
    {
        BackendManager.Instance?.DeleteAccount(uid);
    }
    public void Logout()
    {
        BackendManager.Instance?.Logout();
    }
    public void OpenMain()
    {
        foreach (var v in uis)
        {
            if (v.type == SettingUIType.Main) v.ui.SetShow();
            else v.ui.SetHide();
        }
    }

    private void ShowConfirmPopup(string title, string message, System.Action onConfirm)
    {
        PopupManager.Instance.ShowConfirmPopup(
            title,
            message,
            onConfirm,
            () => Debug.Log($"{title} 취소됨")
        );
    }
}
