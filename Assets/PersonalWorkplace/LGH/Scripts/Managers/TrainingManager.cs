using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TrainingType
{
    ExtAtk, InnAtk, HP
}

public class TrainingManager : MonoBehaviour
{
    public static TrainingManager Instance { get; private set; }

    public Dictionary<TrainingType, int> trainingLevels = new();
    private string userID;
    public int trainingTier = 1; // 현재 훈련 레벨
    public int MaxLevelPerTier => trainingTier * 200;

    public bool IsTrainingDataLoaded { get; private set; } = false;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;

        // 훈련 타입별 초기값 설정
        foreach (TrainingType type in Enum.GetValues(typeof(TrainingType)))
        {
            if (!trainingLevels.ContainsKey(type))
                trainingLevels[type] = 0;
        }
    }


    private void Start()
    {
        userID = CurrencyManager.Instance.UserID;
        LoadTrainingLevelsFromFirebase();
    }

    public bool CanLevelUpTraining(TrainingType type)
    {
        int level = trainingLevels.GetValueOrDefault(type, 0);
        return level < MaxLevelPerTier;
    }

    public void LevelUpTraining(TrainingType type)
    {
        if (!CanLevelUpTraining(type))
        {
            Debug.LogWarning($"{type} 훈련은 현재 티어({trainingTier})에서 최대 레벨에 도달했습니다.");
            return;
        }

        trainingLevels[type]++;
        SaveTrainingLevelToFirebase(type);
        ApplyTrainingModifiers(type);
    }

    public bool CanUpgradeTrainingTier()
    {
        return trainingLevels.Values.All(level => level >= MaxLevelPerTier);
    }

    public void UpgradeTrainingTier()
    {
        if (!CanUpgradeTrainingTier())
        {
            Debug.LogWarning("모든 훈련이 현재 티어의 최대 레벨에 도달해야 티어를 올릴 수 있습니다.");
            return;
        }

        trainingTier++;
        Debug.Log($"훈련 티어가 {trainingTier}로 상승했습니다!");
        SaveTrainingTierToFirebase();
    }


    private void LoadTrainingLevelsFromFirebase()
    {
        string path = $"users/{userID}/training";
        FirebaseDatabase.DefaultInstance.GetReference(path)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    var root = task.Result;

                    // 훈련 티어 불러오기
                    if (root.HasChild("tier"))
                    {
                        trainingTier = int.Parse(root.Child("tier").Value.ToString());
                        Debug.Log($"훈련 티어 불러옴: {trainingTier}");
                    }

                    // 훈련 레벨 불러오기
                    if (root.HasChild("levels"))
                    {
                        foreach (var child in root.Child("levels").Children)
                        {
                            if (Enum.TryParse(child.Key, out TrainingType type))
                            {
                                trainingLevels[type] = int.Parse(child.Value.ToString());
                            }
                        }

                        foreach (var type in trainingLevels.Keys)
                            ApplyTrainingModifiers(type);

                        IsTrainingDataLoaded = true;
                    }

                    GameEvents.TrainingDataLoaded();
                }
                else
                {
                    Debug.LogWarning("훈련 데이터 Firebase에서 불러오기 실패");
                }
            });
    }

    private void SaveTrainingLevelToFirebase(TrainingType type)
    {
        string path = $"users/{userID}/training/levels/{type}";
        FirebaseDatabase.DefaultInstance.GetReference(path)
            .SetValueAsync(trainingLevels[type])
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"훈련 레벨 저장 실패: {task.Exception}");
            });
    }

    private void SaveTrainingTierToFirebase()
    {
        string path = $"users/{userID}/training/tier";
        FirebaseDatabase.DefaultInstance.GetReference(path)
            .SetValueAsync(trainingTier)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"훈련 티어 저장 실패: {task.Exception}");
            });
    }

    private void ApplyTrainingModifier(TrainingType type, PlayerController player)
    {
        string originID = $"Training_{type}";
        int level = trainingLevels.GetValueOrDefault(type, 0);
        float value = GetTrainingBonus(type, level);

        string charID = player.charID.Value;

        StatType statType = type switch
        {
            TrainingType.ExtAtk => StatType.ExtAtk,
            TrainingType.InnAtk => StatType.InnAtk,
            TrainingType.HP => StatType.Health,
            _ => StatType.Health
        };

        StatModifierManager.RemoveModifiersByOrigin(charID, originID);
        StatModifierManager.ApplyModifier(charID,
            new StatModifier(statType, value, ModifierSource.Training, originID));
        StatModifierManager.ApplyToModel(player.model);
    }

    private void ApplyTrainingModifiers(TrainingType type)
    {
        foreach (var player in PartyManager.Instance.players)
        {
            if (player?.model?.modelSO == null) continue;
            ApplyTrainingModifier(type, player);
        }
    }

    public void ApplyTrainingModifiersToPlayer(PlayerController player)
    {
        foreach (var type in trainingLevels.Keys)
        {
            ApplyTrainingModifier(type, player);
            Debug.Log($"적용 스탯 : {type}");
        }
    }

    /// <summary>
    /// 수련 보너스 계산 메서드
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public float GetTrainingBonus(TrainingType type, int level)
    {
        if (level <= 0)
            return 0f;

        float basePerLevel = type switch
        {
            TrainingType.ExtAtk => 10f,
            TrainingType.InnAtk => 10f,
            TrainingType.HP => 100f,
            _ => 0f
        };

        float totalBonus = 0f;
        int remainingLevel = level;
        int multiplier = 1;

        while (remainingLevel > 0)
        {
            int chunk = Mathf.Min(200, remainingLevel);
            totalBonus += chunk * basePerLevel * multiplier;

            remainingLevel -= chunk;
            multiplier++;
        }

        return totalBonus;
    }

    /// <summary>
    /// TrainingUI에서 쓰기 위한 오버로드 메서드
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetTrainingBonus(TrainingType type)
    {
        int level = trainingLevels.GetValueOrDefault(type, 0);
        return GetTrainingBonus(type, level);
    }
}
