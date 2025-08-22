using UnityEngine;

/// <summary>
/// 카드 표기에만 사용할 정보를 저장합니다.
/// </summary>
[CreateAssetMenu(fileName = "CharCardBase", menuName = "Scriptable Objects/CharCardBase")]
public class CharCardBase : ScriptableObject
{   
    public int HeroID;                      // 캐릭터의 고유 ID
    public int HeroStage;                   // 돌파 정보
    public HeroRarity rarity;               // 레어도
    public HeroRelationship relationship;   // 소속
}
