using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("맵 프리팹이 붙을 부모 오브젝트")]
    [SerializeField] private Transform mapParent;

    [Header("맵 데이터베이스 (ScriptableObject)")]
    [SerializeField] private MapDatabase mapDatabase;

    private int currentStageIndex = 0;
    private GameObject currentMap;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadStage(currentStageIndex);
    }

    public void GoToNextStage()
    {
        currentStageIndex++;
        LoadStage(currentStageIndex);
    }

    private void LoadStage(int stageIndex)
    {
        if (currentMap != null)
            Destroy(currentMap);

        if (stageIndex < 0 || stageIndex >= mapDatabase.maps.Count)
        {
            Debug.Log("더 이상 스테이지가 없습니다!");
            return;
        }

        MapData data = mapDatabase.maps[stageIndex];
        GameObject prefab = Resources.Load<GameObject>(data.prefabName);

        if (prefab != null)
        {
            currentMap = Instantiate(prefab, Vector3.zero, Quaternion.identity, mapParent);
            currentMap.name = $"Stage_{stageIndex}_{data.mapId}";
        }
        else
        {
            Debug.LogError($"맵 프리팹을 찾을 수 없습니다: {data.prefabName}");
        }
    }
}