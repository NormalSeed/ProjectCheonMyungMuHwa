using UnityEngine;

[System.Serializable] // ⭐ ScriptableObject 안에서 보이려면 꼭 필요
public class MapData
{
    public string mapId;            // 맵 ID 
    public int stageNumber;       // 스테이지 번호
    public string prefabName;     // 맵 프리팹 이름
    public MapTheme theme;        // 테마
}

public enum MapTheme
{
    Grass,
    Desert,
    Dungeon,
    Lava
}