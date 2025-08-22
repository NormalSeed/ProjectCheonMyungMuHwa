using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SkillSet : ScriptableObject
{
    [SerializeField] public string SkillSetID;
    [SerializeField] public List<PlayerSkillSO> skills;
}
