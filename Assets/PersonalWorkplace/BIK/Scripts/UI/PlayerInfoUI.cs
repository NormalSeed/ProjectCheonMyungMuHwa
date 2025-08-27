using TMPro;
using UnityEngine;
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

    private void Awake()
    {
        // 버튼 리스너 등록
        _profileButton.onClick.AddListener(OnClick_ProfileButton);
    }

    private void OnEnable()
    {
        if (PlayerProfileManager.Instance != null)
            PlayerProfileManager.Instance.OnProfileChanged += ApplyProfileData;

        // 혹시 이미 로드되어 있다면 즉시 반영
        ApplyProfileData(PlayerProfileManager.Instance?.CurrentProfile);
    }

    private void OnDisable()
    {
        if (PlayerProfileManager.Instance != null)
            PlayerProfileManager.Instance.OnProfileChanged -= ApplyProfileData;
    }

    /// <summary>
    /// 버튼 클릭 시 PlayerProfile UI 열기
    /// </summary>
    private void OnClick_ProfileButton()
    {
        _mainSceneUIController.ShowUI(UIType.PlayerProfile);
    }

    /// <summary>
    /// PlayerProfileManager에서 데이터 가져와 적용
    /// </summary>
    private void ApplyProfileData(PlayerProfileData profile)
    {
        if (profile == null) return;

        // 닉네임 적용
        nicknameText.text = profile.Nickname;

        // 레벨은 추후 적용 예정
        levelText.text = "-";

        // 프로필 이미지 적용 (Resources 기준 예시)
        var profileSprite = Resources.Load<Sprite>($"Profile/{profile.ProfileImage}");
        if (profileSprite != null)
            _profileImage.sprite = profileSprite;
    }
}
