using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileUI : UIBase
{
    [Header("버튼들")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _editNicknameButton;
    [SerializeField] private Button _saveButton;

    [Header("플레이어 프로필")]
    [SerializeField] private Image _playerProfileBGImage;
    [SerializeField] private Image _playerProfileImage;
    [SerializeField] private TMP_Text _playerNicknameText;     // 보기 모드
    [SerializeField] private TMP_InputField _playerNicknameInput; // 수정 모드
    [SerializeField] private TMP_Text _playerStyleText;
    [SerializeField] private TMP_Text _playerUIDText;

    [Header("토글 및 패널들")]
    [SerializeField] private List<Toggle> _toggles;      // 총 3개
    [SerializeField] private List<GameObject> _panels;  // 토글과 1:1 매칭

    [Header("컨트롤러 참조")]
    [SerializeField] private MainSceneUIController _mainSceneUIController;

    private bool _isEditingNickname = false;

    private void Awake()
    {
        // 버튼 리스너 등록
        _closeButton.onClick.AddListener(OnClickClose);
        _saveButton.onClick.AddListener(OnClickSave);
        _editNicknameButton.onClick.AddListener(OnClickEditNickname);

        // 닉네임 입력 필드 리스너 등록
        _playerNicknameInput.onEndEdit.AddListener(OnNicknameEditEnd);

        // 토글 리스너 등록
        for (int i = 0; i < _toggles.Count; i++) {
            int index = i; // 클로저 문제 방지
            _toggles[i].onValueChanged.AddListener(isOn => OnToggleChanged(index, isOn));
        }
    }

    private void Start()
    {
        LoadProfileFromServer();
        ToggleNicknameEdit(false); // 시작은 보기 모드
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        _playerNicknameInput.onEndEdit.RemoveListener(OnNicknameEditEnd);
    }

    #region Button Callbacks

    private void OnClickClose()
    {
        _mainSceneUIController.CloseAllUI();
    }

    private void OnClickSave()
    {
        SaveProfileToServer();
    }

    private void OnClickEditNickname()
    {
        ToggleNicknameEdit(!_isEditingNickname);
    }

    #endregion

    #region Toggle Logic

    private void OnToggleChanged(int index, bool isOn)
    {
        if (index < 0 || index >= _panels.Count) return;

        // 선택된 패널만 활성화
        for (int i = 0; i < _panels.Count; i++) {
            _panels[i].SetActive(i == index && isOn);
        }
    }

    #endregion

    #region Nickname Edit Mode

    private void ToggleNicknameEdit(bool isEdit)
    {
        _isEditingNickname = isEdit;

        _playerNicknameText.gameObject.SetActive(!isEdit);
        _playerNicknameInput.gameObject.SetActive(isEdit);

        if (isEdit) {
            // 수정 시작 시 현재 닉네임 복사
            _playerNicknameInput.text = _playerNicknameText.text;
            _playerNicknameInput.ActivateInputField();
        }
    }

    private void OnNicknameEditEnd(string newName)
    {
        // 엔터 입력 시 → Text 반영 + 서버 저장 + 보기 모드 전환
        _playerNicknameText.text = newName;
        PlayerProfileManager.Instance.SetNickname(newName);

        ToggleNicknameEdit(false);
    }

    #endregion

    private void LoadProfileFromServer()
    {
        var profile = PlayerProfileManager.Instance.CurrentProfile;
        if (profile == null) return;

        _playerNicknameText.text = profile.Nickname;
        _playerUIDText.text = profile.Uid;
        _playerStyleText.text = profile.Title;

        // 배경화면 적용
        var bgSprite = Resources.Load<Sprite>($"Backgrounds/{profile.Background}");
        if (bgSprite != null)
            _playerProfileBGImage.sprite = bgSprite;

        // 프로필 이미지 적용
        var profileSprite = Resources.Load<Sprite>($"Profile/{profile.ProfileImage}");
        if (profileSprite != null)
            _playerProfileImage.sprite = profileSprite;
    }

    private void SaveProfileToServer()
    {
        var profile = PlayerProfileManager.Instance.CurrentProfile;
        if (profile == null) return;

        // UI에서 수정된 값 반영
        profile.Nickname = _playerNicknameText.text;
        profile.Title = _playerStyleText.text;
        profile.Background = "DefaultBG";     // 예시
        profile.ProfileImage = "DefaultProfile"; // 예시

        PlayerProfileManager.Instance.SaveProfileToFirebase();
    }
}
