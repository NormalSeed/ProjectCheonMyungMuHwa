using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkillSO", menuName = "Game/PlayerSkillSO")]
public class PlayerSkillSO : ScriptableObject
{
    [SerializeField] public string SkillID;
    [SerializeField] public string Skillname;
    [SerializeField] public string SkillType;
    [SerializeField] public string EffectType;
    [SerializeField] public float ExtSkillDmg;
    [SerializeField] public float InnSkillDmg;
    [SerializeField] public float CoolTime;
    [SerializeField] public float SkillRange;
}
