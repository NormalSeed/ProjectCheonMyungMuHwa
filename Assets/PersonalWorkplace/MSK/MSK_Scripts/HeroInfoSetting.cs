using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoSetting : MonoBehaviour
{
    [Header("SO")]
    [SerializeField] private CardInfo chardata;

    public string HeroID { get; private set; }
    public int HeroStage { get; private set; }
    private HeroRarity rarity;
    private HeroRelationship relationship;


    [Header("Root References")]
    [SerializeField] private Transform cardBackgroundRoot;
    [SerializeField] private Transform characterRoot;
    [SerializeField] private Transform stageRoot;
    [SerializeField] private Transform badgeRoot;
    [SerializeField] private Transform selectRoot;

    [Header("UI")]
    [SerializeField] private Button CardButton;
    [SerializeField] private TextMeshProUGUI PartyNum;
    [SerializeField] private GameObject HeroInfoUI;


    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        CardButton.onClick.RemoveListener(OnClickCard);
    }

    private void Init()
    {
        HeroID = chardata.HeroID;
        HeroStage = chardata.HeroStage;
        rarity = chardata.rarity;
        relationship = chardata.relationship;

        SetBackground();
        SetCharacter();
        SetStage();
        SetBadge();

        CardButton.onClick.AddListener(OnClickCard);
    }

    #region Init
    private void SetBackground()
    {
        foreach (Transform child in cardBackgroundRoot)
            child.gameObject.SetActive(false);

        Transform target = cardBackgroundRoot.Find(rarity.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetCharacter()
    {
        foreach (Transform child in characterRoot)
            child.gameObject.SetActive(false);

        Transform target = characterRoot.Find(HeroID.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }
    private void SetBadge()
    {
        foreach (Transform child in badgeRoot)
            child.gameObject.SetActive(false);

        Transform target = badgeRoot.Find(relationship.ToString());
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private void SetStage()
    {
        foreach (Transform stage in stageRoot)
        {
            Transform red = stage.Find("Stage_Red");
            Transform gray = stage.Find("Stage_gray");

            if (red != null) red.gameObject.SetActive(false);
            if (gray != null) gray.gameObject.SetActive(true);
        }

        for (int i = 1; i <= HeroStage; i++)
        {
            Transform stage = stageRoot.Find("Stage" + i);
            if (stage != null)
            {
                Transform red = stage.Find("Stage_Red");
                Transform gray = stage.Find("Stage_gray");

                if (red != null) red.gameObject.SetActive(true);
                if (gray != null) gray.gameObject.SetActive(false);
            }
        }
    }
    #endregion


    #region OnClick
    private void OnClickCard()
    {
        // 파티 편성중일 경우 HeroSetting()
        // 선택 효과 + 편성 번호 표시

        // 일반적 상황에서   HeroUIActive()
        /* 영웅 개별 UI 활성화 */
    }
    #endregion

    private void HeroSetting()
    {
        selectRoot.gameObject.SetActive(true);

    }

    private void HeroUIActive()
    {

    }
}
