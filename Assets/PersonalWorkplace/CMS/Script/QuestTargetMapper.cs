using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestTargetMapper
{
    private static readonly Dictionary<string, QuestTargetType> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "None", QuestTargetType.None },
        { "OnLogin", QuestTargetType.OnLogin },
        { "Collect", QuestTargetType.Collect },
        { "Kill", QuestTargetType.Kill },
        { "Monster", QuestTargetType.Kill }, // DB에서 Monster → Kill 로 매핑
        { "Explore", QuestTargetType.Explore },
        { "StageClear", QuestTargetType.StageClear },
        { "LevelReach", QuestTargetType.LevelReach }
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