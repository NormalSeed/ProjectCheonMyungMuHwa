using DG.Tweening;
using Firebase.Auth;
using Firebase.Database;
using GooglePlayGames.BasicApi;
using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;
    public const int MAX_PROGRESS = 3;

    private List<PlayerController> players;

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

    [Header("누적 데미지 UI")]
    [SerializeField] SynergyUI synergyUI;

    [Header("페이드 인/아웃 이미지")]
    [SerializeField] private Image fadeImage;

    [Header("게임 종료 UI")]
    [SerializeField] private GameObject gameQuitUI;
    public bool isQuitUIActive = false;

    private string stage => $"<color=yellow>{stageNum}관문 {(stageProgress == 3 ? "보스" : stageProgress + 1)}던전</color>";

    [SerializeField] TMPro.TMP_Text stagetext;

    public Action<int, int> OnStageStart;
    public MainSceneBGMPlayer bgmPlayer;
    private bool stageLoaded;

    [SerializeField] MainSceneUIController mainUI;
    private UIType mainUiToOpen;

    private void Awake()
    {
        Instance = this;
        bgmPlayer = new MainSceneBGMPlayer();
        LoadStageFromFirebase();
    }

    private void Start()
    {
        players = PartyManager.Instance.players;

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

    private void LoadStageFromFirebase()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference($"users/{uid}/stage");

        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int savedStage = int.Parse(task.Result.Value.ToString());
                stageNum = savedStage;
                //MapManager.Instance.currentStageIndex = stageNum;
                stageLoaded = true;
                Debug.Log($"Firebase에서 불러온 스테이지: {stageNum}");
            }
            else
            {
                Debug.LogWarning("Firebase에서 스테이지 데이터를 찾을 수 없습니다. 기본값 사용.");
            }
        });
        StartCoroutine(InitStage());
    }
    private IEnumerator InitStage()
    {
        yield return new WaitUntil(() => stageLoaded);
        bgmPlayer.SetInitialBGM(stageNum);
        MapManager.Instance.currentStageIndex = stageNum;
        MapManager.Instance.InitStage();
    }

    public void RespawnMonsters()
    {
        PoolManager.Instance.ActiveAll(stageNum);
        alignedNum.Value = 0;
        stageProgress++;

        foreach (var player in players)
        {
            player.damageDealt = 0;
        }
        synergyUI.UpdateDamageUI();
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
        playerCount = 0;
        foreach (var player in players)
        {
            if (player.gameObject.activeInHierarchy)
            {
                playerCount++;
            }
        }

        if (isProcessingAlignment || num < playerCount) return;
        if (stagetext != null) stagetext.text = stage;

        Debug.LogError("[InGame] aligned 검사 통과");
        
        isProcessingAlignment = true;
        MapManager.Instance.SpawnMonsters(stageNum, stageProgress);
        StartCoroutine(ResetAlignmentFlag());
        OnStageStart?.Invoke(stageNum, stageProgress);
        bgmPlayer.SetBGM(stageNum);

        foreach (var player in players)
        {
            player.damageDealt = 0;
        }
        synergyUI.UpdateDamageUI();
        Debug.Log($"<color=yellow>[igm]{stageProgress}");
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
            foreach (var player in players)
            {
                if (player.model == null) continue;
                if (player.isDead.Value) player.Resurrect();
                player.model.CurHealth.Value = player.model.Health;
            }
            if (stageProgress == MAX_PROGRESS - 1)
            {
                stageProgress++;
                MapManager.Instance.SpawnMonsters(stageNum, stageProgress);
                OnStageStart?.Invoke(stageNum, stageProgress);
                return;
            }

            alignedNum.Value = 0;
            isProcessingAlignment = false;
            alignPoint.SetActive(false);

            Map currentMap = MapManager.Instance.currentMap.GetComponent<Map>();
            if (currentMap != null)
            {
                Vector3 nextSpawnPos = currentMap.endPoint.position;

                if (stageProgress < MAX_PROGRESS - 1) // 아직 보스 전
                {
                    PoolManager.Instance.GetItems();
                    stageProgress++;
                    MapManager.Instance.GoToNextStage(nextSpawnPos);
                }
                else // 보스 클리어 → 다음 스테이지
                {
                    PoolManager.Instance.GetItems();
                    stageProgress = 0;
                    stageNum++;
                    MapManager.Instance.currentStageIndex = stageNum;
                    SetNextStage();
                    MapManager.Instance.GoToNextStage(nextSpawnPos);
                    PopupManager.Instance.ShowStageClearPopup();
                }
            }
        }
    }

    public void SetNextStage()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference($"users/{uid}/stage");

        dbRef.SetValueAsync(stageNum).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"스테이지 {stageNum} 저장 완료");
            }
            else
            {
                Debug.LogError("스테이지 저장 실패: " + task.Exception);
            }
        });
    }

    public void CheckAllPlayersDead(bool isDead)
    {
        List<PlayerController> players = PartyManager.Instance.players;

        // 모든 플레이어가 사망했는지 확인
        bool allActivePlayersDead = players
        .Where(p => p.gameObject.activeSelf)
        .All(p => p.isDead.Value);

        if (allActivePlayersDead)
        {
            DungeonFailUI ui = PopupManager.Instance.ShowDungeonFailPopup();
            ui.SetActionToButton(0, () => { mainUiToOpen = UIType.Hero; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
            ui.SetActionToButton(1, () => { mainUiToOpen = UIType.Dungeon; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
            ui.SetActionToButton(2, () => { mainUiToOpen = UIType.Upgrade; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
            ui.SetActionToButton(3, () => { mainUiToOpen = UIType.Summon; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
            ui.SetActionToScreen(() => { mainUiToOpen = UIType.None; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
            ui.SetActionToTimeOut(() => { mainUiToOpen = UIType.None; StartCoroutine(HandleAllPlayersDead()); ui.SetHide(); });
        }
    }

    private IEnumerator HandleAllPlayersDead()
    {
        Debug.Log("모든 플레이어 사망. 페이드 아웃 시작");

        // 페이드 아웃 효과 추가해야함
        FadeCanvas.Instance.FadeOut(2f);

        yield return new WaitForSeconds(2f); // 암전 시간

        // 몬스터 비활성화(풀매니저에서 현재 활성화 상태인 몬스터를 전부 풀로 되돌리는 메서드 추가 요청)
        PoolManager.Instance.ReleaseAll();

        // 스테이지 초기화
        stageProgress = 0;
        monsterDeathStack.Value = 0;
        stageProgress = 0;

        // 페이드인 전에 플레이어 위치 재배치
        if (alignPoint.activeInHierarchy == false)
        {
            Debug.LogError("정렬 포인트가 활성화되지 않음");
            alignPoint.SetActive(true);
        }
        for (int i = 0; i < PartyManager.Instance.players.Count; i++)
        {
            Transform point = alignPoint.transform.Find($"Point{i + 1}");
            if (point != null)
            {
                Vector3 offset = new Vector3(0, -0.1f, 0);
                var player = PartyManager.Instance.players[i];
                if (player.gameObject.activeSelf == false) continue;

                var agent = player.NMagent;

                if (agent != null)
                {
                    agent.Warp(point.position + offset);
                    player.transform.rotation = point.rotation;
                }
                else
                {
                    player.transform.position = point.position + offset;
                    player.transform.rotation = point.rotation;
                }
            }
            else
            {
                Debug.LogWarning("정렬 포인트가 없음");
            }
        }

        // 페이드 인 효과 추가해야함


        Debug.Log("스테이지 초기화 완료. 재시작 준비됨");

        // 재정렬 포인트 활성화
        alignPoint.SetActive(true);

        // 모든 플레이어의 상태를 Move로 설정
        foreach (var player in PartyManager.Instance.players)
        {
            if (!player.gameObject.activeSelf) continue;

            player.isDead.Value = false;
            player.spumController.PlayAnimation(PlayerState.IDLE, 0);

            var agent = player.BGagent;
            if (agent != null)
            {
                agent.SetVariableValue("CurState", PlayerStates.Move);

                agent.enabled = false;
                agent.enabled = true;
                agent.Init();
            }
        }

        mainUI.ShowUI(mainUiToOpen);
        FadeCanvas.Instance.FadeIn(1.5f);
    }

    private void FadeOut()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1f, 2f);
    }

    private void FadeIn()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0f, 1.5f);
    }

}
