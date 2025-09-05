using UnityEngine;

[System.Serializable]
public class HeroData
{   
    public bool hasHero;                     // 영웅 보유 여부
    public int heroPiece;                    // 영웅 조각 개수
    public int level;                        // 영웅 래벨
    public string rarity;                    // 영웅 레어도
    public int stage;                        // 영웅 돌파단계
    public CardInfo cardInfo;                // 카드 정보
    public PlayerModelSO PlayerModelSO;      // 모델 정보
    // public PlayerSkillSO PlayerSkillSO;      // 스킬 정보
}