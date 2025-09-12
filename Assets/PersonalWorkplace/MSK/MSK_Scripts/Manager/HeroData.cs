using UnityEngine;

[System.Serializable]
public class HeroData
{   
    public bool hasHero;                     // 영웅 보유 여부
    public int heroPiece;                    // 영웅 조각 개수
    public int level;                        // 영웅 래벨
    public string rarity;                    // 영웅 레어도
    public int stage;                        // 영웅 돌파단계
    public string heroId;                    // 영웅 고유 ID
    public CardInfo cardInfo;                // 카드 정보
    public PlayerModelSO PlayerModelSO;      // 모델 정보
    // public PlayerSkillSO PlayerSkillSO;   // 스킬 정보
    public string weapone;                   // 무기 장비 ID
    public string armor;                     // 방어구 장비 ID
    public string boots;                     // 신발 장비 ID
    public string gloves;                    // 장갑 장비 ID
}