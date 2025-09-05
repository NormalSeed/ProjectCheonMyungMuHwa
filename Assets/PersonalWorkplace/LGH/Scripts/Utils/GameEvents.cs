using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<int> OnHeroLevelChanged;

    public static void HeroLevelChanged(int newLevel)
    {
        OnHeroLevelChanged?.Invoke(newLevel);
    }
}
