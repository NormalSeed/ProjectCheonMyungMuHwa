using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Start()
    {
        // UID 표시
        string uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "알 수 없음";
        uidText.text = $"UID: {uid}";

        // 종료 버튼
        quitButton.onClick.AddListener(QuitGame);

        logoutButton.onClick.AddListener(() =>
        {
            ShowConfirmPopup(
                "로그아웃",
                "정말 로그아웃 하시겠습니까?\n(게스트 계정의 경우 정보가 삭제됩니다)",
                () => BackendManager.Instance?.Logout()
            );
        });

        deleteAccountButton.onClick.AddListener(() =>
        {
            ShowConfirmPopup(
                "계정 삭제",
                "정말 계정을 삭제하시겠습니까?",
                () => BackendManager.Instance?.DeleteAccount(uid)
            );
        });

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

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
