using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ExpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _levelText;      // "Lv. 12"
    [SerializeField] private Image _progressImage;

    private TableManager _tableManager;

    [Inject]
    public void Construct(TableManager tableManager)
    {
        _tableManager = tableManager;
    }

    private void OnEnable()
    {
        if (PlayerProfileManager.Instance != null) {
            PlayerProfileManager.Instance.OnProfileChanged += ApplyProfile;
            ApplyProfile(PlayerProfileManager.Instance.CurrentProfile);
        }

        var tLevel = GetTLevel();
        if (tLevel != null) {
            if (!tLevel.IsInitialized)
                tLevel.OnLoaded += HandleLevelTableLoaded;
            else
                ApplyProfile(PlayerProfileManager.Instance?.CurrentProfile);
        }
    }

    private void OnDisable()
    {
        if (PlayerProfileManager.Instance != null)
            PlayerProfileManager.Instance.OnProfileChanged -= ApplyProfile;

        var tLevel = GetTLevel();
        if (tLevel != null)
            tLevel.OnLoaded -= HandleLevelTableLoaded;
    }

    private void HandleLevelTableLoaded()
    {
        ApplyProfile(PlayerProfileManager.Instance?.CurrentProfile);

        var tLevel = GetTLevel();
        if (tLevel != null)
            tLevel.OnLoaded -= HandleLevelTableLoaded;
    }

    private void ApplyProfile(PlayerProfileData profile)
    {
        if (profile == null) return;

        if (_levelText != null)
            _levelText.text = $"Lv. {profile.Level}";

        int maxExp = PlayerProfileManager.Instance?.GetCurrentLevelMaxExp() ?? 0;
        int curExp = profile.Exp;

        if (_progressImage != null) {
            _progressImage.fillAmount = (maxExp > 0) ? Mathf.Clamp01((float)curExp / maxExp) : 0f;
            _progressImage.enabled = true;
        }
    }

    private TLevel GetTLevel()
    {
        return _tableManager?.GetTable<TLevel>(TableType.Level);
    }

    // Test

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            var ppm = PlayerProfileManager.Instance;
            if (ppm != null) {
                ppm.AddExp(50);
                Debug.Log("[Test] Space pressed: +50 EXP");
            }
        }
    }
}
