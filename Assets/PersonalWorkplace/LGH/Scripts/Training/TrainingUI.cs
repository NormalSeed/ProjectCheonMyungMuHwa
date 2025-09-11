using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingUI : MonoBehaviour
{
    public Button extLevelUpButton;
    public Button innLevelUpButton;
    public Button HPLevelUpButton;
    public Button TrainingTierLevelUpButton;

    public TextMeshProUGUI extLevelText;
    public TextMeshProUGUI innLevelText;
    public TextMeshProUGUI HPLevelText;
    public TextMeshProUGUI TrainingTierText;

    public TextMeshProUGUI extValueText;
    public TextMeshProUGUI innBalueText;
    public TextMeshProUGUI HPValueText;

    private TrainingManager training => TrainingManager.Instance;

    private void Start()
    {
        extLevelUpButton.onClick.AddListener(() => OnClickLevelUp(TrainingType.ExtAtk));
        innLevelUpButton.onClick.AddListener(() => OnClickLevelUp(TrainingType.InnAtk));
        HPLevelUpButton.onClick.AddListener(() => OnClickLevelUp(TrainingType.HP));
        TrainingTierLevelUpButton.onClick.AddListener(OnClickTierUp);

        UpdateUI();
    }

    private void OnEnable()
    {
        GameEvents.OnTrainingDataLoaded += UpdateUI;

        UpdateUI();
    }

    private void OnClickLevelUp(TrainingType type)
    {
        training.LevelUpTraining(type);
        UpdateUI();
    }

    private void OnClickTierUp()
    {
        training.UpgradeTrainingTier();
        UpdateUI();
    }

    private void UpdateUI()
    {
        extLevelText.text = $"Lv. {training.trainingLevels.GetValueOrDefault(TrainingType.ExtAtk, 0)}";
        innLevelText.text = $"Lv. {training.trainingLevels.GetValueOrDefault(TrainingType.InnAtk, 0)}";
        HPLevelText.text = $"Lv. {training.trainingLevels.GetValueOrDefault(TrainingType.HP, 0)}";

        TrainingTierText.text = $"훈련 티어: {training.trainingTier}";

        extValueText.text = $"+{training.GetTrainingBonus(TrainingType.ExtAtk):N0} 외공";
        innBalueText.text = $"+{training.GetTrainingBonus(TrainingType.InnAtk):N0} 내공";
        HPValueText.text = $"+{training.GetTrainingBonus(TrainingType.HP):N0} 체력";

        extLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.ExtAtk);
        innLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.InnAtk);
        HPLevelUpButton.interactable = training.CanLevelUpTraining(TrainingType.HP);

        TrainingTierLevelUpButton.interactable = training.CanUpgradeTrainingTier();
        Debug.Log("훈련정보가 로딩되어 UI에 적용했습니다.");
    }

}
