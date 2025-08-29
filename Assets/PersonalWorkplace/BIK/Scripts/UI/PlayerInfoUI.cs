using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image _profileImage;
    [SerializeField] private Button _profileButton;

    [Header("Controller Reference")]
    [SerializeField] private MainSceneUIController _mainSceneUIController;

    // 현재 로드된 핸들 캐싱 (메모리 릭 방지용)
    private AsyncOperationHandle<Sprite>? _currentHandle;

    private void Awake()
    {
        _profileButton.onClick.AddListener(OnClick_ProfileButton);
    }

    private IEnumerator Start()
    {
        while (PlayerProfileManager.Instance == null)
            yield return null;

        PlayerProfileManager.Instance.OnProfileChanged += ApplyProfileData;
        ApplyProfileData(PlayerProfileManager.Instance.CurrentProfile);
    }

    //private void OnEnable()
    //{
    //    if (PlayerProfileManager.Instance != null)
    //        PlayerProfileManager.Instance.OnProfileChanged += ApplyProfileData;

    //    ApplyProfileData(PlayerProfileManager.Instance?.CurrentProfile);
    //}

    private void OnDisable()
    {
        if (PlayerProfileManager.Instance != null)
            PlayerProfileManager.Instance.OnProfileChanged -= ApplyProfileData;

        // 핸들 해제
        if (_currentHandle.HasValue) {
            Addressables.Release(_currentHandle.Value);
            _currentHandle = null;
        }
    }

    private void OnClick_ProfileButton()
    {
        _mainSceneUIController.ShowUI(UIType.PlayerProfile);
    }

    private void ApplyProfileData(PlayerProfileData profile)
    {
        if (profile == null) return;

        nicknameText.text = profile.Nickname;
        levelText.text = "-";

        if (!string.IsNullOrEmpty(profile.ProfileImage)) {
            // 기존 핸들 해제
            if (_currentHandle.HasValue) {
                Addressables.Release(_currentHandle.Value);
                _currentHandle = null;
            }

            // 어드레서블 로드
            var handle = Addressables.LoadAssetAsync<Sprite>(profile.ProfileImage);
            _currentHandle = handle;
            handle.Completed += op => {
                if (op.Status == AsyncOperationStatus.Succeeded)
                    _profileImage.sprite = op.Result;
            };
        }
    }
}
