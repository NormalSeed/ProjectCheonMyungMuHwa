using System.Collections;
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

    private string stage => $"<color=yellow>{Mathf.Ceil(stageNum / 3f)}관문 {(stageProgress == 3 ? "보스" : stageProgress + 1)}던전</color>";

    [SerializeField] TMPro.TMP_Text stagetext;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        monsterDeathStack.Value = 0;
        monsterDeathStack.Subscribe(val => { if (val == 0) PoolManager.Instance.GetItems(); });
        stageProgress = 0;

        alignedNum.Value = 0;

        alignedNum.Subscribe(ExamineAllAligned);
    }

    public void RespawnMonsters()
    {
        PoolManager.Instance.ActiveAll(stageNum);
        stageProgress++;
        if (stageProgress < 3) stageNum++;

    }

    public void SpawnBoss()
    {
        PoolManager.Instance.ActiveBoss(stageNum);
        Debug.Log("보스 소환함");
        stageProgress = 0;
        stageNum++;
    }

    public void ExamineAllAligned(int num)
    {
        if (isProcessingAlignment || num < playerCount)return;
        if (stagetext != null) stagetext.text = stage;

        isProcessingAlignment = true;
        if (stageProgress < 3)
        {
            RespawnMonsters();
        }
        else
        {
            SpawnBoss();
        }
        StartCoroutine(ResetAlignmentFlag());
    }
    private IEnumerator ResetAlignmentFlag()
    {
        yield return new WaitForSeconds(0.1f);
        alignedNum.Value = 0;
        isProcessingAlignment = false;
    }


    public void SetNextStage()
    {
        stageNum++;
    }
}
