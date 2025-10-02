using DG.Tweening;
using TMPro;
using UnityEngine;

public class StagePopup : UIBase
{
    [SerializeField] private GameObject _alertObject;
    [SerializeField] private TMP_Text _stageText;

    [Header("AnimSettings")]
    [SerializeField] private float _slideTime = 1.5f;
    [SerializeField] private Ease _firstEase = Ease.OutBounce;
    [SerializeField] private float _autoHideDelay = 1.5f; // 아래로 내려온 뒤 대기 시간
    [SerializeField] private Ease _endEase = Ease.Linear;

    // 내부 상태
    private RectTransform _alertRect;
    private Vector2 _defaultPos;   // 최초 on-screen 기준 좌표(절대)
    private bool _posCached = false;
    private Sequence _seq;

    private void Awake()
    {
        if (_alertObject != null)
            _alertRect = _alertObject.GetComponent<RectTransform>();

        // 가능하면 초기에 캐시(프리팹 초기 위치)
        if (_alertRect != null) {
            _defaultPos = _alertRect.anchoredPosition;
            _posCached = true;
        }
    }

    private void OnEnable()
    {
        // 활성화시에도 위치를 항상 초기값으로 복원
        if (_alertRect != null && _posCached)
            _alertRect.anchoredPosition = _defaultPos;
    }

    private void OnDisable()
    {
        KillTween();
        // 비활성화될 때도 위치를 초기값으로 맞춰 두면 다음 호출에서 안전
        if (_alertRect != null && _posCached)
            _alertRect.anchoredPosition = _defaultPos;
    }

    public void SetShow(int stage)
    {
        KillTween();
        base.SetShow();

        _stageText.text = $"스테이지 {stage}";

        if (_alertObject == null || _alertRect == null)
            return;

        // 최초 한 번만 정확한 on-screen 좌표를 캐시
        if (!_posCached) {
            _defaultPos = _alertRect.anchoredPosition;
            _posCached = true;
        }

        _alertObject.SetActive(true);

        // 항상 "절대" on-screen 기준으로 offTop을 계산
        float offDist = _alertRect.rect.height + 200f;
        Vector2 offTopPos = _defaultPos + new Vector2(0f, offDist);

        // 시작은 화면 위쪽 바깥, 도착은 절대 on-screen
        _alertRect.anchoredPosition = offTopPos;

        _seq = DOTween.Sequence();
        _seq.Append(_alertRect.DOAnchorPos(_defaultPos, _slideTime).SetEase(_firstEase));
        _seq.AppendInterval(_autoHideDelay);
        _seq.Append(_alertRect.DOAnchorPos(offTopPos, _slideTime).SetEase(_endEase));

        _seq.OnComplete(() => {
            // 다음 호출 드리프트 방지: 비활성화 전에 위치를 원위치로 복구
            _alertRect.anchoredPosition = _defaultPos;
            _alertObject.SetActive(false);
            base.SetHide();
        });
    }

    public override void SetHide()
    {
        KillTween();

        // 수동으로 닫을 때도 위치를 복구
        if (_alertRect != null && _posCached)
            _alertRect.anchoredPosition = _defaultPos;

        if (_alertObject != null)
            _alertObject.SetActive(false);

        base.SetHide();
    }

    private void KillTween()
    {
        if (_seq != null && _seq.IsActive()) {
            _seq.Kill();
            _seq = null;
        }
    }
}
