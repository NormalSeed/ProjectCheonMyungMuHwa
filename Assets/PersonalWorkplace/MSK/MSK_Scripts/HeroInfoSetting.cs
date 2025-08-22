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

    [Header("UI")]
    [SerializeField] private Button CardButton;
    [SerializeField] private GameObject HeroInfoUI;


    private void Awake()
    {
        Init();
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
}
