using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentInfoPanel : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI textEquipmentName;       //장비 이름
    [SerializeField] private TextMeshProUGUI textEquipmentEffect;     //장비 효과
    [SerializeField] private TextMeshProUGUI textEffectRate;          //효과 수치;
    [SerializeField] private TextMeshProUGUI textUserStone;           //보유중인 연마석
    [SerializeField] private TextMeshProUGUI textNeedStone;           //소모 예상 연마석
    [SerializeField] private TextMeshProUGUI textEquipLevel;          //장비 레벨
    [SerializeField] private TextMeshProUGUI textNextlevel;           //다음 장비 레벨
    [SerializeField] private TextMeshProUGUI textPresentEff;          //현재 장비 효과
    [SerializeField] private TextMeshProUGUI textNextEff;             //다음 레벨 효과
    [SerializeField] private TextMeshProUGUI textPresentEffectRate;   //현재 장비 효과 수치
    [SerializeField] private TextMeshProUGUI textNextEffectRate;      //다음 레벨 효과 수치
    [SerializeField] private TextMeshProUGUI textEquip;               //장비/ 해제 텍스트

    [Header("Image")]
    [SerializeField] private Image imageEquipment;                    // 장비 이미지
    
    [Header("Button")]
    [SerializeField] private Button exitButton;                   //나가기 버튼
    [SerializeField] private Button equipButton;                  //장비하기/ 해제하기 버튼
    [SerializeField] private Button upgradeButton;                //장비 강화 버튼

    private EquipmentInstance instance;
    private int level;                      // 장비 레벨
    private int effectRate;                 // 증가량
    private int nextEffectRate;             // 다음 레벨 증가량


    #region Unity
    private void OnEnable()
    {
        Init();
        SetButtonAddListener();
    }
    private void OnDisable()
    {
        SetButtonRemoveListener();
    }
    #endregion

    #region Init
    private void Init()
    {
        textEquipmentName.text = instance.template.equipmentName;
        textEquipmentEffect.text = instance.template.description;
        textEffectRate.text = instance.statType.ToString();

    }
    private void SetPanelText()
    {

    }
    private void SetButtonAddListener()
    {
        exitButton.onClick.AddListener(OnClickExit);
        equipButton.onClick.AddListener(OnClickEquip);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
    }
    private void SetButtonRemoveListener()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
        equipButton.onClick.RemoveListener(OnClickEquip);
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
    }
    #endregion

    #region OnClick Mathood
    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClickUpgrade()
    {

    }

    private void OnClickEquip()
    {

    }
    #endregion

    #region Private
    #endregion

    #region Public
    #endregion
}
