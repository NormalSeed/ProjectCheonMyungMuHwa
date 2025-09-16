using UnityEngine;

public enum HeroRarity
{
    Normal = 0,  // 노먈(흰색)
    Rare = 1,   // 레어(하늘색)
    Unique = 2,   // 에픽(보라색)
    Legend = 3,  // 유니크(노란색)
}

public enum HeroFaction
{
    J,  // 정파
    M,  // 마교
    S,  // 사파
}

public enum CurrencyType
{
    Jewel,                      // 용옥(다이아)
    Gold,                       // 금화
    Soul,                       // 혼백(경험치)
    SpiritStone,                // 영석(보물 강화)
    SummonTicket,               // 등용패(뽑기권)
    InvitationTicket,           // 초대장(입장권)
    GoldChallengeTicket,        // 도전장(입장권)
    SoulChallengeTicket,        // 혼백도전장(입장권)
    SpiritStoneChallengeTicket, // 영석도전장(입장권)
}

public enum UIType
{
    None = 0,
    Notice,         // 공지
    Quest,          // 퀘스트
    Mail,           // 우편함
    Attendance,     // 출석표
    Ranking,        // 순위
    Setting,        // 설정
    Hero,           // 영웅 UI
    Upgrade,        // 성장 UI
    Dungeon,        // 던전 UI
    Inventory,      // 인벤토리 UI
    Summon,         // 소환/뽑기 UI
    Shop,           // 상점 UI
    HeroInfo,       // 영웅정보 UI
    PlayerProfile,  // 플레이어 프로필
}

public enum SummonLevel
{
    level01 = 1,        // 소환레벨
    level02 = 2,
    level03 = 3,
    level04 = 4,
    level05 = 5,
    level06 = 6,
    level07 = 7,
    level08 = 8,
    level09 = 9,
    level10 = 10
}

public enum PopupType
{
    OfflineRewardPopup,
    RewardPopup,
    Tooltip,
    Alert,
}

public enum TableType
{
    Item,
    Level,
}

public enum ItemType
{
    None,
    Currency,
    Box,
    Equipment,
    Etc
}