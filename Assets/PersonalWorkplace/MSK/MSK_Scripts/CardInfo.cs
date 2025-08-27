using UnityEngine;

/// <summary>
/// 카드 표기에만 사용할 정보를 저장합니다.
/// </summary>
[CreateAssetMenu(fileName = "CharCardBase", menuName = "Scriptable Objects/CharCardBase")]
public class CardInfo : ScriptableObject
{   
    public string HeroID;                   // 캐릭터의 고유 ID
    //  돌파정보는 파이어베이스 서버에서 가져와야 한다.
    public int HeroStage;                   // 돌파 정보
    public HeroRarity rarity;               // 레어도
    public HeroFaction faction;   // 소속
}
