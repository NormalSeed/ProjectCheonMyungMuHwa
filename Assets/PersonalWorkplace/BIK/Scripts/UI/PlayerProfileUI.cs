using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    [SerializeField] private TMP_Text _playerNicknameText;
    [SerializeField] private TMP_InputField _playerNicknameInput;
    [SerializeField] private TMP_Text _playerStyleText;
    [SerializeField] private TMP_Text _playerUIDText;

    [Header("토글 및 패널들")]
    [SerializeField] private List<Toggle> _toggles;
    [SerializeField] private List<GameObject> _panels;

    [Header("컨트롤러 참조")]
    [SerializeField] private MainSceneUIController _mainSceneUIController;

    [Header("캐릭터 UI")]
    [SerializeField] private List<CharImage> _characterSlots; // 미리 배치한 슬롯들
    [SerializeField] private List<PlayerModelSO> _allCharacters; // SO 자동 생성된 캐릭터 목록

    [Header("타이틀 UI")]
    [SerializeField] private List<Button> _titleButtons;       // 미리 배치된 버튼들
    [SerializeField] private List<TitleData> _titleDataList;   // 같은 순서대로 매핑

    [Header("배경 UI")]
    [SerializeField] private List<Button> _backgroundButtons;
    [SerializeField] private List<BackgroundData> _backgroundDataList;

    private bool _isEditingNickname = false;

    // Save 버튼 눌러야 확정되는 값
    private string _tempTitle;
    private string _tempBackground;
    private string _tempProfileImage;

    // 유저 진행도 (스테이지만 사용)
    private int _playerStageProgress = 30; // 예시 값

    private void Awake()
    {
        _closeButton.onClick.AddListener(OnClickClose);
        _saveButton.onClick.AddListener(OnClickSave);
        _editNicknameButton.onClick.AddListener(OnClickEditNickname);

        _playerNicknameInput.onEndEdit.AddListener(OnNicknameEditEnd);

        for (int i = 0; i < _toggles.Count; i++) {
            int index = i;
            _toggles[i].onValueChanged.AddListener(isOn => OnToggleChanged(index, isOn));
        }

        InitTitleUI();
        InitBackgroundUI();
    }

    private void Start()
    {
        LoadProfileFromServer();
        ToggleNicknameEdit(false);

        // 예시 보유 캐릭터 목록 (실제는 서버에서 불러옴)
        List<string> ownedIds = new List<string> { "A01", "B02" };
        ApplyCharacterList(ownedIds);
    }

    private void OnDestroy()
    {
        _playerNicknameInput.onEndEdit.RemoveListener(OnNicknameEditEnd);
    }

    #region Button Callbacks
    private void OnClickClose() => _mainSceneUIController.CloseAllUI();
    private void OnClickSave() => SaveProfileToServer();
    private void OnClickEditNickname() => ToggleNicknameEdit(!_isEditingNickname);
    #endregion

    #region Toggle Logic
    private void OnToggleChanged(int index, bool isOn)
    {
        if (index < 0 || index >= _panels.Count) return;
        for (int i = 0; i < _panels.Count; i++)
            _panels[i].SetActive(i == index && isOn);
    }
    #endregion

    #region Nickname
    private void ToggleNicknameEdit(bool isEdit)
    {
        _isEditingNickname = isEdit;
        _playerNicknameText.gameObject.SetActive(!isEdit);
        _playerNicknameInput.gameObject.SetActive(isEdit);

        if (isEdit) {
            _playerNicknameInput.text = _playerNicknameText.text;
            _playerNicknameInput.ActivateInputField();
        }
    }

    private void OnNicknameEditEnd(string newName)
    {
        _playerNicknameText.text = newName;
        PlayerProfileManager.Instance.SetNickname(newName); // 닉네임은 즉시 저장
        ToggleNicknameEdit(false);
    }
    #endregion

    #region Profile Load/Save
    private void LoadProfileFromServer()
    {
        var profile = PlayerProfileManager.Instance.CurrentProfile;
        if (profile == null) return;

        _playerNicknameText.text = profile.Nickname;
        _playerUIDText.text = profile.Uid;
        _playerStyleText.text = profile.Title;

        _tempTitle = profile.Title;
        _tempBackground = profile.Background;
        _tempProfileImage = profile.ProfileImage;

        if (!string.IsNullOrEmpty(profile.Background)) {
            Addressables.LoadAssetAsync<Sprite>(profile.Background).Completed += op => {
                if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    _playerProfileBGImage.sprite = op.Result;
            };
        }

        if (!string.IsNullOrEmpty(profile.ProfileImage)) {
            Addressables.LoadAssetAsync<Sprite>(profile.ProfileImage).Completed += op => {
                if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    _playerProfileImage.sprite = op.Result;
            };
        }
    }

    private void SaveProfileToServer()
    {
        var profile = PlayerProfileManager.Instance.CurrentProfile;
        if (profile == null) return;

        profile.Title = _tempTitle;
        profile.Background = _tempBackground;
        profile.ProfileImage = _tempProfileImage;

        PlayerProfileManager.Instance.SaveProfileToFirebase();
    }
    #endregion

    #region Character
    private void ApplyCharacterList(List<string> ownedIds)
    {
        // 정렬: 보유 캐릭터 먼저
        var sorted = _allCharacters
            .OrderByDescending(c => ownedIds.Contains(c.CharID))
            .ToList();

        for (int i = 0; i < _characterSlots.Count; i++) {
            if (i < sorted.Count) {
                var model = sorted[i];
                var slot = _characterSlots[i];

                slot.gameObject.SetActive(true);
                slot.SetCharacter(model);

                var button = slot.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    _playerProfileImage.sprite = slot.CurrentSprite; // UI 즉시 반영
                    _tempProfileImage = model.SpriteKey;             // Save 버튼 눌러야 확정
                });

                slot.SetLocked(!ownedIds.Contains(model.CharID));
            }
            else {
                _characterSlots[i].gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region Title / Background
    private void InitTitleUI()
    {
        for (int i = 0; i < _titleButtons.Count && i < _titleDataList.Count; i++) {
            var btn = _titleButtons[i];
            var data = _titleDataList[i];

            bool unlocked = IsUnlocked(data.Conditions);
            btn.interactable = unlocked;
            btn.GetComponentInChildren<TMP_Text>().text = data.DisplayName;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                if (unlocked) {
                    _playerStyleText.text = data.DisplayName;
                    _tempTitle = data.Id;
                }
            });
        }
    }

    private void InitBackgroundUI()
    {
        for (int i = 0; i < _backgroundButtons.Count && i < _backgroundDataList.Count; i++) {
            var btn = _backgroundButtons[i];
            var data = _backgroundDataList[i];

            bool unlocked = IsUnlocked(data.Conditions);
            btn.interactable = unlocked;
            btn.GetComponentInChildren<TMP_Text>().text = data.DisplayName;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                if (unlocked) {
                    Addressables.LoadAssetAsync<Sprite>(data.SpriteKey).Completed += op => {
                        if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                            _playerProfileBGImage.sprite = op.Result;
                    };
                    _tempBackground = data.SpriteKey;
                }
            });
        }
    }

    private bool IsUnlocked(UnlockCondition[] conditions)
    {
        foreach (var cond in conditions) {
            if (cond.Type == UnlockCondition.ConditionType.Stage &&
                _playerStageProgress < cond.RequiredValue)
                return false;
        }
        return true;
    }
    #endregion
}
