using System;

[Serializable]
public class UnlockCondition
{
    public enum ConditionType { Stage, }
    public ConditionType Type;
    public int RequiredValue;
}