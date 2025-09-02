using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("맵 프리팹이 붙을 부모 오브젝트")]
    [SerializeField] private Transform mapParent;

    [Header("테마별 맵 프리팹 이름")]
    [SerializeField] private string[] mapThemes = { "Map_Stage01", "Map_Stage02", "Map_Stage03" };

    private int currentStageIndex = 1; // 1스테이지부터 시작
    private List<GameObject> spawnedMaps = new List<GameObject>(); // 생성된 맵 기록

    [SerializeField] private GameObject[] monsterPrefabs; // 몬스터 프리팹

    private bool isSpawning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnStage(currentStageIndex, Vector3.zero);
    }

    public void GoToNextStage(Vector3 spawnPosition)
    {
        if (isSpawning) return; // 이미 생성 중이면 무시
        StartCoroutine(SpawnNextStageCoroutine(spawnPosition));
    }

    private IEnumerator SpawnNextStageCoroutine(Vector3 spawnPosition)
    {
        isSpawning = true;

        currentStageIndex++;
        SpawnStage(currentStageIndex, spawnPosition);

        yield return null; // 한 프레임 기다렸다가 풀어줌
        isSpawning = false;
    }

    private void SpawnStage(int stageIndex, Vector3 spawnPosition)
    {
        // 맵 생성
        int themeIndex = (stageIndex - 1) / 100;
        if (themeIndex >= mapThemes.Length)
            themeIndex = mapThemes.Length - 1;

        string prefabName = mapThemes[themeIndex];
        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            GameObject newMap = Instantiate(prefab, spawnPosition, Quaternion.identity, mapParent);
            newMap.name = $"Stage_{stageIndex}_{prefabName}";
            spawnedMaps.Add(newMap);

            if (spawnedMaps.Count > 5)
            {
                GameObject oldMap = spawnedMaps[0];
                spawnedMaps.RemoveAt(0);
                Destroy(oldMap);
            }

            Debug.Log($"스테이지 {stageIndex}, {prefabName} 생성 완료");

            // 여기서 보스/일반 체크
            if (stageIndex % 3 == 0)  // 스테이지마다 보스
            {
                PoolManager.Instance.ActiveBoss(stageIndex);
            }
            else
            {
                PoolManager.Instance.ActiveAll(stageIndex);
            }
        }
        else
        {
            Debug.LogError($"맵 프리팹을 찾을 수 없습니다: {prefabName}");
        }
    }
}