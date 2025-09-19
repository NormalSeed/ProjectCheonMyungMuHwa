using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;
    public ObservableProperty<int> monsterDeathStack { get; private set; } = new();
    public int stageProgress = 0;
    public int stageNum = 1;
    public ObservableProperty<int> alignedNum { get; private set; } = new();
    private bool isProcessingAlignment = false;
    public int playerCount;

    public bool isBossDead = false;

    public bool isStartReady = false;

    public GameObject alignPoint;

    public NavMeshSurface surface;

    [Header("게임 종료 UI")]
    [SerializeField] private GameObject gameQuitUI;
    public bool isQuitUIActive = false;

    private string stage => $"<color=yellow>{stageNum}관문 {(stageProgress == 3 ? "보스" : stageProgress + 1)}던전</color>";

    [SerializeField] TMPro.TMP_Text stagetext;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        monsterDeathStack.Value = 0;
        stageProgress = 0;

        alignedNum.Value = 0;

        alignedNum.Subscribe(ExamineAllAligned);
        monsterDeathStack.Subscribe(CheckMonsterClear);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isQuitUIActive)
            {
                // UI가 떠 있는 상태에서 다시 뒤로가기 → UI 닫기
                gameQuitUI.SetActive(false);
                isQuitUIActive = false;
            }
            else
            {
                // UI가 안 떠 있는 상태에서 뒤로가기 → UI 띄우기
                gameQuitUI.SetActive(true);
                isQuitUIActive = true;
            }
        }
    }

    public void RespawnMonsters()
    {
        PoolManager.Instance.ActiveAll(stageNum);
        alignedNum.Value = 0;
        stageProgress++;
    }

    public void SpawnBoss()
    {
        PoolManager.Instance.ActiveBoss(stageNum);
        Debug.Log("보스 소환함");
        stageProgress = 0;
        //stageNum++;
    }

    public void ExamineAllAligned(int num)
    {
        if (isProcessingAlignment || num < playerCount)return;
        if (stagetext != null) stagetext.text = stage;

        isProcessingAlignment = true;
        bool isBossSpawned = MapManager.Instance.SpawnMonsters(stageNum, stageProgress);
        if (isBossSpawned)
        {
            stageProgress = 0;
        }
        else
        {
            stageProgress++;
        }
        StartCoroutine(ResetAlignmentFlag());
    }
    private IEnumerator ResetAlignmentFlag()
    {
        yield return new WaitForSeconds(0.1f);
        isProcessingAlignment = false;
    }

    public void CheckMonsterClear(int deathStack)
    {
        if (monsterDeathStack.Value <= 0)
        {
            List<PlayerController> players = PartyManager.Instance.players;

            foreach (PlayerController player in players)
            {
                player.model.CurHealth.Value = player.model.Health;
            }

            alignedNum.Value = 0; // 전투 종료 후 초기화
            isProcessingAlignment = false;
            PoolManager.Instance.GetItems();
            // 현재 정렬 포인트 비활성화
            alignPoint.SetActive(false);

            // 다음 스테이지로 이동
            Map currentMap = MapManager.Instance.currentMap.GetComponent<Map>();
            if (currentMap != null)
            {
                Vector3 nextSpawnPos = currentMap.endPoint.position;
                MapManager.Instance.GoToNextStage(nextSpawnPos);
            }
            else
            {
                Debug.LogWarning("현재 맵을 찾을 수 없습니다.");
            }
        }
    }

    public void SetNextStage()
    {
        stageNum++;
    }
}
