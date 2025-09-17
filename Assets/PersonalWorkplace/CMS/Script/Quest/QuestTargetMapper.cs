using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestTargetMapper
{
    private static readonly Dictionary<string, QuestTargetType> _map =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { "None", QuestTargetType.None },
        { "OnLogin", QuestTargetType.OnLogin },
        { "Monster", QuestTargetType.Monster },
        { "Gacha1", QuestTargetType.Gacha1 },
        { "Gacha2", QuestTargetType.Gacha2 },
        { "Growth", QuestTargetType.Growth },
        { "Training", QuestTargetType.Training },
        { "Enhance", QuestTargetType.Enhance },
        { "Organization", QuestTargetType.Organization },
        { "Vital", QuestTargetType.Vital },
        { "ExtPow", QuestTargetType.ExtPow },
        { "InnPow", QuestTargetType.InnPow },
        { "Box", QuestTargetType.Box },
        { "Stage", QuestTargetType.Stage },
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