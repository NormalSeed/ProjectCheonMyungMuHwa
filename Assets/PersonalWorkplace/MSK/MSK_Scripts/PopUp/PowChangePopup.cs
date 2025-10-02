using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class PowChangePopup : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI powText;       // 전투력
    [SerializeField] private TextMeshProUGUI valueText;     // 전투력 변화량

    [Header("Image")]
    [SerializeField] private Image arrowImg;                // 화살표 이미지
    [SerializeField] private Sprite arrowUp;                // 화살표 상승
    [SerializeField] private Sprite arrowDown;              // 화살표 하락

    [Header("Root Ref")]
    [SerializeField] private PowChangePopup powPopup;

    [Header("Animation")]
    [SerializeField] private AnimationClip animEffect;

    private BigCurrency pow;        // 전투력
    private BigCurrency value;      // 전투력변화량
    private bool arrowUpDown;       // 전투력 상승여부
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetShow(BigCurrency.FromBaseAmount(10), BigCurrency.FromBaseAmount(1));
    }


    #region Init
    private void Init()
    {
        arrowUpDown = value > BigCurrency.FromBaseAmount(0);
        powText.text = pow.ToString();
        valueText.text = value.ToString();
        if (arrowUpDown)
        {
            valueText.color = new Color(239f / 255f, 55f / 255f, 61f / 255f, 1f); // 상승 빨강
            arrowImg.sprite = arrowUp;
        }
        else
        {
            valueText.color = new Color(109f/ 255f, 185f / 255f, 254f /255f, 1f); // 하락 파랑
            arrowImg.sprite = arrowDown;
        }
    }
    #endregion


    #region Private
    
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetHide();
    }


    #endregion

    #region Public
    public void SetShow(BigCurrency inputNow, BigCurrency inputPre)
    {
        pow = inputNow;
        value = inputNow - inputPre;
        Init();
        powPopup.gameObject.SetActive(true);
        animator.Play("Pow");
        StartCoroutine(HideAfterDelay(animEffect.length));
    }


    public override void SetHide()
    {
        powPopup.gameObject.SetActive(false);
    }
    #endregion
}
