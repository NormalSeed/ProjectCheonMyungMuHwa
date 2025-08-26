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

    public bool isBossDead = false;

    public bool isStartReady = false;

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
    }

    public void RespawnMonsters()
    {
        PoolManager.Instance.ActiveAll(stageNum);
        stageProgress++;
        
    }

    public void SpawnBoss()
    {
        PoolManager.Instance.ActiveBoss(stageNum);
        Debug.Log("보스 소환함");
        stageProgress = 0;
    }

    public void ExamineAllAligned(int num)
    {
        if (isProcessingAlignment) return;

        if (alignedNum.Value == 5)
        {
            if (stageProgress < 2)
            {
                RespawnMonsters();
            }
            else
            {
                SpawnBoss();
            }

            alignedNum.Value = 0;

            StartCoroutine(ResetAlignmentFlag());
        }
    }
    private IEnumerator ResetAlignmentFlag()
    {
        yield return new WaitForSeconds(0.1f);
        isProcessingAlignment = false;
    }


    public void SetNextStage()
    {
        stageNum++;
    }
}
