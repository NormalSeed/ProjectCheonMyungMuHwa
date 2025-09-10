using System;

public static class GameEvents
{
    public static event Action<int> OnHeroLevelChanged;
    public static event Action OnTrainingDataLoaded;

    public static void HeroLevelChanged(int newLevel)
    {
        OnHeroLevelChanged?.Invoke(newLevel);
    }

    public static void TrainingDataLoaded()
    {
        OnTrainingDataLoaded?.Invoke();
    }
}
