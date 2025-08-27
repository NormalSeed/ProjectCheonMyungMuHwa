using UnityEngine;

public enum HeroRarity
{
    Normal = 0,  // 노먈(흰색)
    Rare = 1,   // 레어(하늘색)
    Epic = 2,   // 에픽(보라색)
    Unique = 3,  // 유니크(노란색)
}

public enum HeroRelationship
{
    J,  // 정파
    M,  // 마교
    S,  // 사파
}

public enum CurrencyType
{
    Jewel,          // 용옥(다이아)
    Gold,           // 금화
    Soul,           // 혼백(경험치)
    SpiritStone,    // 영석(보물 강화)
    SummonTicket,   // 등용패(뽑기권)
    InvitationTicket,// 초대장(입장권)
    ChallengeTicket // 도전장(입장권)
}

public enum UIType
{
    None = 0,
    Notice,
    Quest,
    Mail,
    Attendance,
    Ranking,
    Setting,
    Hero,
    Upgrade,
    Dungeon,
    Inventory,
    Summon,
    Shop,
}