using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestTargetMapper
{
    private static readonly Dictionary<string, QuestTargetType> _map = new(StringComparer.OrdinalIgnoreCase)
{
    { "None", QuestTargetType.None },
    { "OnLogin", QuestTargetType.Onlogin },
    { "Monster", QuestTargetType.Monster },
    { "Summon", QuestTargetType.Summon },
    { "Training", QuestTargetType.Training },
    { "Upgrade", QuestTargetType.Upgrade },
    { "Playtime", QuestTargetType.Playtime },
};

    public static QuestTargetType ToQuestTargetType(string dbValue)
    {
        if (string.IsNullOrWhiteSpace(dbValue))
            return QuestTargetType.None;

        return _map.TryGetValue(dbValue, out var result)
            ? result
            : QuestTargetType.None;
    }
}