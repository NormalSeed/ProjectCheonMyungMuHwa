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
    [SerializeField] private string[] mapThemes = { "Map_Stage01", "Map_Stage02", "Map_Stage03", "Map_Stage04" };

    public int currentStageIndex = 1; // 1스테이지부터 시작
    private List<GameObject> spawnedMaps = new List<GameObject>(); // 생성된 맵 기록

    [SerializeField] private GameObject[] monsterPrefabs; // 몬스터 프리팹

    private bool isSpawning = false;

    public GameObject currentMap; // 마지막으로 생성된 맵

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {

    }
    public void InitStage()
  {
        SpawnStage(currentStageIndex, Vector3.zero);

        if (spawnedMaps.Count > 0)
        {
            currentMap = spawnedMaps[spawnedMaps.Count - 1];
            Transform alignRoot = currentMap.transform.Find("AlignPoint");
            InGameManager.Instance.alignPoint = alignRoot?.gameObject;

            if (alignRoot != null)
            {
                for (int i = 0; i < PartyManager.Instance.players.Count; i++)
                {
                    Transform point = alignRoot.Find($"Point{i}");
                    if (point != null)
                    {
                        var player = PartyManager.Instance.players[i];
                        var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();

                        if (agent != null)
                        {
                            agent.Warp(point.position);
                            player.transform.rotation = point.rotation;
                        }
                        else
                        {
                            player.transform.position = point.position;
                            player.transform.rotation = point.rotation;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("AlignPoint를 찾을 수 없습니다.");
            }
        }
  }

    public void GetAlignPoint()
    {
        if (spawnedMaps.Count > 0)
        {
            Transform alignRoot = currentMap.transform.Find("AlignPoint");
            InGameManager.Instance.alignPoint = alignRoot.gameObject;
        }
    }

    public void GoToNextStage(Vector3 spawnPosition)
    {
        if (isSpawning) return; // 이미 생성 중이면 무시
        StartCoroutine(SpawnNextStageCoroutine(spawnPosition));
    }

    private IEnumerator SpawnNextStageCoroutine(Vector3 spawnPosition)
    {
        isSpawning = true;

        //currentStageIndex++;
        SpawnStage(currentStageIndex, spawnPosition);

        // 한 프레임 기다렸다가 풀어줌
        yield return null;
        InGameManager.Instance.surface.BuildNavMesh();
        isSpawning = false;
    }

    private void SpawnStage(int stageIndex, Vector3 spawnPosition)
    {
        // 몇 번째 테마를 쓸지 결정 (순환 반복, 25스테이지마다)
        int themeIndex = ((stageIndex - 1) / 25) % mapThemes.Length;

        string prefabName = mapThemes[themeIndex];
        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            GameObject newMap = Instantiate(prefab, spawnPosition, Quaternion.identity, mapParent);
            newMap.name = $"Stage_{stageIndex}_{prefabName}";
            spawnedMaps.Add(newMap);
            currentMap = newMap;
            GetAlignPoint();

            if (spawnedMaps.Count > 5)
            {
                GameObject oldMap = spawnedMaps[0];
                spawnedMaps.RemoveAt(0);
                Destroy(oldMap);
            }

            Debug.Log($"스테이지 {stageIndex}, {prefabName} 생성 완료");
        }
        else
        {
            Debug.LogError($"맵 프리팹을 찾을 수 없습니다: {prefabName}");
        }
    }

    public bool SpawnMonsters(int stageIndex, int stageProgress)
    {
        
        PoolManager.Instance.SetMonsterState(stageIndex);

        var spawnPoints = currentMap.GetComponentsInChildren<SpawnPoint>();

        if (stageProgress < InGameManager.MAX_PROGRESS) // 0, 1, 2에서 몬스터만 소환
        {
            foreach (var point in spawnPoints)
            {
                PoolManager.Instance.SpawnMonster(point.transform.position, point.monsterType);
            }
            return false;
        }
        else // 3관문 → 보스 추가
        {
            PoolManager.Instance.SpawnMonster(
                currentMap.transform.position + new Vector3(0, 6.55f, 0),
                MonsterType.Boss
            );
            PopupManager.Instance.ShowBossStagePopup(stageIndex);
            return true;
        }
    }
    public void GoToNextGate(Vector3 spawnPosition)
    {
        int gate = InGameManager.Instance.stageProgress;

        if (gate < 2)
        {
            SpawnStage(currentStageIndex, spawnPosition);
        }
        else
        {
            // 보스 클리어 → 다음 스테이지로 이동
            currentStageIndex++;
            //InGameManager.Instance.stageProgress = 0; 
            SpawnStage(currentStageIndex, spawnPosition);

            Debug.Log($"스테이지 {currentStageIndex} 시작!");
        }
    }
}