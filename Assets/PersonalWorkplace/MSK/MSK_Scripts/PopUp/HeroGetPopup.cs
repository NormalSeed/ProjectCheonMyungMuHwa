using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroGetPopup :  UIBase

{
    [Header("Image")]
    [SerializeField] private Image heroImage;               // 영웅 이미지

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI heroName;      // 이름
    [SerializeField] private TextMeshProUGUI healthText;    // 채력
    [SerializeField] private TextMeshProUGUI InnText;       // 내공
    [SerializeField] private TextMeshProUGUI extText;       // 외공
    
    [Header("Root Reference")]
    [SerializeField] private Transform badgeRoot;           // 팩션,소속
    [SerializeField] private HeroGetPopup heroGetPopup;     

    [Header("Button")]
    [SerializeField] private Button exitButton;             // 팝업 종료 버튼

    private CardInfo heroData;


    #region Unity
    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnClickExit);
    }

    #endregion



    #region Init
    private void Init()
    {
        SetCharacter();
        SetBadge();
        healthText.text = BigCurrency.FromBaseAmount(heroData.HealthPoint).ToString();
        InnText.text = BigCurrency.FromBaseAmount(heroData.InnAtkPoint).ToString();
        extText.text = BigCurrency.FromBaseAmount(heroData.ExtAtkPoint).ToString();
        exitButton.onClick.AddListener(OnClickExit);
    }
    #endregion



    #region OnClick
    private void OnClickExit()
    {
        SetHide();
    }

    #endregion



    #region Private
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(heroData.faction.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetCharacter()
    {
        heroImage.sprite = HeroSprites.Instance.GetCharacterSprite(heroData.HeroID);
    }
    #endregion



    #region Public
    public void SetShow(CardInfo data)
    {
        heroData = data;
        Init();
        heroGetPopup.gameObject.SetActive(true);
        AudioManager.Instance.PlaySound("4. 전설 캐릭터 획득 사운드");
    }

    public override void SetHide()
    {
        heroGetPopup.gameObject.SetActive(false);
    }
    #endregion
}
