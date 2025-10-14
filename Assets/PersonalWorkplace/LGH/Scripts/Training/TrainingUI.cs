using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrainingUI : UIBase
{
    public Button extLevelUpButton;
    public Button innLevelUpButton;
    public Button HPLevelUpButton;
    public Button TrainingTierLevelUpButton;
    public Button exitButton;

    public TextMeshProUGUI extLevelText;
    public TextMeshProUGUI innLevelText;
    public TextMeshProUGUI HPLevelText;
    public TextMeshProUGUI TrainingTierText;

    public TextMeshProUGUI extValueText;
    public TextMeshProUGUI innValueText;
    public TextMeshProUGUI HPValueText;

    private BigCurrency extRequireGold = new();
    private BigCurrency innRequireGold = new();
    private BigCurrency HPRequireGold = new();

    public TextMeshProUGUI extRequireGoldText;
    public TextMeshProUGUI innRequireGoldText;
    public TextMeshProUGUI HPRequireGoldText;

    public int trainingCount;

    // 홀드시 연속 레벨업 기능용 필드
    private bool isHoldingExt;
    private bool isHoldingInn;
    private bool isHoldingHP;

    private float initialDelayTimer = 0f;
    private float repeatTimer = 0f;

    private const float InitialDelay = 1f;
    private const float RepeatInterval = 0.1f;
    private bool hasTriggeredInitial = false;

    // 트레이닝 매니저
    private TrainingManager training => TrainingManager.Instance;

    private void Start()
    {
        TrainingTierLevelUpButton.onClick.AddListener(OnClickTierUp);
        exitButton.onClick.AddListener(OnClickExit);

        trainingCount = 1;

        SetupHoldableButton(extLevelUpButton, TrainingType.ExtAtk, "Ext");
        SetupHoldableButton(innLevelUpButton, TrainingType.InnAtk, "Inn");
        SetupHoldableButton(HPLevelUpButton, TrainingType.HP, "HP");

        UpdateUI();
    }

    private void OnEnable()
    {
        GameEvents.OnTrainingDataLoaded += UpdateUI;

        UpdateUI();
    }

    private void Update()
    {
        if (isHoldingExt || isHoldingInn || isHoldingHP)
        {
            initialDelayTimer += Time.unscaledDeltaTime;

            if (!hasTriggeredInitial && initialDelayTimer >= InitialDelay)
            {
                hasTriggeredInitial = true;
                repeatTimer = 0f;
            }

            if (hasTriggeredInitial)
            {
                repeatTimer += Time.unscaledDeltaTime;

                if (repeatTimer >= RepeatInterval)
                {
                    if (isHoldingExt) OnClickLevelUp(TrainingType.ExtAtk);
                    if (isHoldingInn) OnClickLevelUp(TrainingType.InnAtk);
                    if (isHoldingHP) OnClickLevelUp(TrainingType.HP);

                    repeatTimer = 0f;
                }
            }
        }
    }

    private void OnClickLevelUp(TrainingType type)
    {
        int currentLevel = training.trainingLevels.GetValueOrDefault(type, 0);
        int maxLevel = training.MaxLevelPerTier;
        int targetLevel = Mathf.Min(currentLevel + trainingCount, maxLevel);
        int levelsToUpgrade = targetLevel - currentLevel;

        if (levelsToUpgrade <= 0)
        {
            Debug.LogWarning("이미 최대 레벨에 도달했거나 올릴 수 있는 레벨이 없습니다.");
            return;
        }

        BigCurrency totalCost = BigCurrency.FromBaseAmount(0);

        for (int i = 0; i < levelsToUpgrade; i++)
        {
            int level = currentLevel + i;
            totalCost += BigCurrency.FromBaseAmount(level * 10); // 골드 요구량 계산
        }

        if (!CurrencyManager.Instance.TrySpend(CurrencyType.Gold, totalCost))
        {
            Debug.LogWarning($"골드 부족: {totalCost} 필요");
            return;
        }

        for (int i = 0; i < levelsToUpgrade; i++)
        {
            QuestManager.Instance.ReportEvent(QuestTargetType.ExtPow, 1);
            training.LevelUpTraining(type);
        }

        UpdateUI();
    }

    private void SetupHoldableButton(Button button, TrainingType type, string flagName)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((_) =>
        {
            SetHoldingFlag(flagName, true);
            OnClickLevelUp(type); // 즉시 1회 실행
            initialDelayTimer = 0f;
            repeatTimer = 0f;
            hasTriggeredInitial = false;
        });
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((_) => SetHoldingFlag(flagName, false));
        trigger.triggers.Add(up);
    }

    private void SetHoldingFlag(string flagName, bool value)
    {
        switch (flagName)
        {
            case "Ext": isHoldingExt = value; break;
            case "Inn": isHoldingInn = value; break;
            case "HP": isHoldingHP = value; break;
        }

        if (!value)
        {
            initialDelayTimer = 0f;
            repeatTimer = 0f;
            hasTriggeredInitial = false;
        }
    }

    private void OnClickTierUp()
    {
        training.UpgradeTrainingTier();
        UpdateUI();
    }

    private void OnClickExit()
    {
        gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        int extLevel = training.trainingLevels.GetValueOrDefault(TrainingType.ExtAtk, 0) - 200 * (training.trainingTier - 1);
        int innLevel = training.trainingLevels.GetValueOrDefault(TrainingType.InnAtk, 0) - 200 * (training.trainingTier - 1);
        int hpLevel = training.trainingLevels.GetValueOrDefault(TrainingType.HP, 0) - 200 * (training.trainingTier - 1);

        if (extLevel >= 200 && innLevel >= 200 && hpLevel >= 200 && training.CanUpgradeTrainingTier())
        {
            training.UpgradeTrainingTier();
            Debug.Log("모든 훈련 레벨이 200에 도달하여 자동으로 티어가 업그레이드되었습니다.");
            extLevel = training.trainingLevels.GetValueOrDefault(TrainingType.ExtAtk, 0);
            innLevel = training.trainingLevels.GetValueOrDefault(TrainingType.InnAtk, 0);
            hpLevel = training.trainingLevels.GetValueOrDefault(TrainingType.HP, 0);
        }

        extLevelText.text = $"Lv. {extLevel}";
        innLevelText.text = $"Lv. {innLevel}";
        HPLevelText.text = $"Lv. {hpLevel}";

        TrainingTierText.text = $"훈련 티어: {training.trainingTier}";

        extValueText.text = $"+{BigCurrency.FromBaseAmount(training.GetTrainingBonus(TrainingType.ExtAtk)):N0}";
        innValueText.text = $"+{BigCurrency.FromBaseAmount(training.GetTrainingBonus(TrainingType.InnAtk)):N0}";
        HPValueText.text = $"+{BigCurrency.FromBaseAmount(training.GetTrainingBonus(TrainingType.HP)):N0}";

        extRequireGold = CalculateTotalCost(extLevel + 200 * (training.trainingTier - 1), trainingCount);
        innRequireGold = CalculateTotalCost(innLevel + 200 * (training.trainingTier - 1), trainingCount);
        HPRequireGold = CalculateTotalCost(hpLevel + 200 * (training.trainingTier - 1), trainingCount);

        extRequireGoldText.text = extRequireGold.ToString();
        innRequireGoldText.text = innRequireGold.ToString();
        HPRequireGoldText.text = HPRequireGold.ToString();

        extLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.ExtAtk);
        innLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.InnAtk);
        HPLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.HP);

        TrainingTierLevelUpButton.interactable = training.CanUpgradeTrainingTier();

        Debug.Log("훈련정보가 로딩되어 UI에 적용했습니다.");

    }

    private BigCurrency CalculateTotalCost(int currentLevel, int count)
    {
        int maxLevel = training.MaxLevelPerTier;
        int targetLevel = Mathf.Min(currentLevel + count, maxLevel);
        int levelsToUpgrade = targetLevel - currentLevel;

        BigCurrency total = BigCurrency.FromBaseAmount(0);
        for (int i = 0; i < levelsToUpgrade; i++)
        {
            int level = currentLevel + i;
            total += BigCurrency.FromBaseAmount(level * 10);
        }

        return total;
    }
}
