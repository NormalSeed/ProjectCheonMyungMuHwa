using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDatabase", menuName = "Game/MapDatabase")]
public class MapDatabase : ScriptableObject
{
    public List<MapData> maps;

    public MapData GetMap(string mapId)
    {
        return maps.Find(m => m.mapId == mapId);
    }
}