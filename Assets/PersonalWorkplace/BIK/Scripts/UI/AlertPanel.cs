using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AlertConfig
{
    public AlertType Type;
    public Image BG;
    public GameObject Things;
}

public class AlertPanel : UIBase
{
    [Header("Animations")]
    [SerializeField] private Image _topSide;
    [SerializeField] private Image _bottomSide;

    [Header("Configs")]
    [SerializeField] private List<AlertConfig> _configs = new();

    [Header("AnimSettings")]
    [SerializeField] private float _topBottomSideTime = 0.5f;
    [SerializeField] private float _mainBGTime = 0.3f;
    [SerializeField] private Ease _ease = Ease.Linear;
    [SerializeField] private float _autoHideDelay = 1.5f; // 애니메이션 끝난 뒤 대기 시간

    [Header("UI")]
    [SerializeField] private TMP_Text _lastLevel;
    [SerializeField] private TMP_Text _currLevel;
    [SerializeField] private TMP_Text _stage;
    [SerializeField] private TMP_Text _powerUpCurr;
    [SerializeField] private TMP_Text _powerUp;
    [SerializeField] private TMP_Text _powerDownCurr;
    [SerializeField] private TMP_Text _powerDown;
    [SerializeField] private TMP_Text _missionText;

    private AlertType _currentType;
    private float _moveOffset = 800f;

    private Dictionary<AlertType, AlertConfig> _configMap;

    private void Awake()
    {
        // 빠른 접근을 위해 Dictionary 구성
        _configMap = new Dictionary<AlertType, AlertConfig>();
        foreach (var cfg in _configs)
            _configMap[cfg.Type] = cfg;
    }

    public void SetShow(AlertType alertType, string textFirst = "", string textSecond = "")
    {
        base.SetShow();
        _currentType = alertType;

        SetText(textFirst, textSecond);

        var topRect = _topSide.rectTransform;
        var bottomRect = _bottomSide.rectTransform;

        // 시작 위치
        topRect.anchoredPosition = new Vector2(-_moveOffset, topRect.anchoredPosition.y);
        bottomRect.anchoredPosition = new Vector2(_moveOffset, bottomRect.anchoredPosition.y);

        topRect.DOAnchorPosX(0, _topBottomSideTime).SetEase(_ease);
        bottomRect.DOAnchorPosX(0, _topBottomSideTime).SetEase(_ease);

        var cfg = _configMap[alertType];
        cfg.BG.gameObject.SetActive(true);
        var bgRect = cfg.BG.rectTransform;
        bgRect.sizeDelta = new Vector2(10, 10);

        Sequence bgSeq = DOTween.Sequence();

        // 1단계: 가로 확장
        bgSeq.Append(bgRect.DOSizeDelta(new Vector2(Screen.width, 10), _mainBGTime).SetEase(_ease));
        // 2단계: 세로 확장
        bgSeq.Append(bgRect.DOSizeDelta(new Vector2(Screen.width, 200), _mainBGTime).SetEase(_ease));

        // 완료 시 표시
        bgSeq.AppendCallback(() => cfg.Things.SetActive(true));

        // 일정 시간 대기 후 자동으로 SetHide 호출
        bgSeq.AppendInterval(_autoHideDelay);
        bgSeq.AppendCallback(() => SetHide());
    }

    public override void SetHide()
    {
        var cfg = _configMap[_currentType];
        cfg.Things.SetActive(false);

        var topRect = _topSide.rectTransform;
        var bottomRect = _bottomSide.rectTransform;
        var bgRect = cfg.BG.rectTransform;

        Sequence bgSeq = DOTween.Sequence();

        // 1단계: 세로 축소
        bgSeq.Append(bgRect.DOSizeDelta(new Vector2(Screen.width, 10), _mainBGTime).SetEase(_ease));

        // Top / Bottom 다시 밖으로
        bgSeq.AppendCallback(() => {
            float topWidth = topRect.rect.width;
            float bottomWidth = bottomRect.rect.width;

            topRect.DOAnchorPosX(-(_moveOffset + topWidth), _topBottomSideTime).SetEase(_ease);
            bottomRect.DOAnchorPosX((_moveOffset + bottomWidth), _topBottomSideTime).SetEase(_ease);
        });

        // 2단계: 가로 축소
        bgSeq.Append(bgRect.DOSizeDelta(new Vector2(10, 10), _mainBGTime).SetEase(_ease));

        bgSeq.OnComplete(() => {
            base.SetHide();
            cfg.BG.gameObject.SetActive(false);
        });
    }

    private void SetText(string textFirst, string textSecond)
    {
        if (_currentType == AlertType.Level) {
            _lastLevel.text = textFirst;
            _currLevel.text = textSecond;
        }
        else if (_currentType == AlertType.Boss) {
            _stage.text = textFirst;
        }
        else if (_currentType == AlertType.PowerUp) {
            _powerUpCurr.text = textFirst;
            _powerUp.text = textSecond;
        }
        else if (_currentType == AlertType.PowerDown) {
            _powerDownCurr.text = textFirst;
            _powerDown.text = textSecond;
        }
        else if (_currentType == AlertType.MissionClear) {
            _missionText.text = textFirst;
        }
        else {
            Debug.LogWarning($"[AlertPanel] 알 수 없는 AlertType: {_currentType}");
        }
    }
}