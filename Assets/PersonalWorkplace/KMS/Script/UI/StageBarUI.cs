using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class StageBarUI : MonoBehaviour
{
    [SerializeField] TMP_Text stageText;
    [SerializeField] TMP_Text stageValueText;
    [SerializeField] StageBarFill bossFill;
    [SerializeField] StageBarFill[] monsterFills;
    [SerializeField] CurrencyDungeonTimer timer;

    private StageBarFill targetMonsterFill;


    void Start()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnStageStart += SetStage;
        }
    }

    private void SetStage(int door, int progress) // 첫번째 몬스터 나왔을때 1부터 3까지, 보스 나왔을때 0
    {
        stageValueText.text = door.ToString();

        switch (progress)
        {
            case 0: BossSetting(); break;
            case 1: case 2: case 3: MonsterSetting(progress); break;
        }
    }

    private void BossSetting()
    {
        timer.Activate();
        foreach (var f in monsterFills)
        {
            f.Inactivate();
        }
        targetMonsterFill = bossFill;
        targetMonsterFill.Activate();
        targetMonsterFill.SetValue(1);
    }
    private void MonsterSetting(int progress)
    {
        timer.Inactivate();
        bossFill.Inactivate();
        targetMonsterFill = monsterFills[progress - 1];
        targetMonsterFill.Activate();
        targetMonsterFill.SetValue(0);
    }

    public void SetFill(float val)
    {
        targetMonsterFill.SetValue(val);

    }
    public void AddFill(float val)
    {
        targetMonsterFill.AddValue(val);
    }


}
