using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class StageBarUI : MonoBehaviour
{
    [SerializeField] TMP_Text stageText;
    [SerializeField] TMP_Text stageValueText;
    [SerializeField] TMP_Text healthText;
    [SerializeField] StageBarFill bossFill;
    [SerializeField] StageBarFill[] monsterFills;
    [SerializeField] CurrencyDungeonTimer timer;

    public CurrencyDungeonTimer Timer => timer;

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
        healthText.gameObject.SetActive(true);
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
        healthText.gameObject.SetActive(false);
        bossFill.Inactivate();
        targetMonsterFill = monsterFills[progress - 1];
        for (int i = 0; i < progress - 1; i++)
        {
            monsterFills[i].Activate();
            monsterFills[i].SetValue(1);
        }
        targetMonsterFill.Activate();
        targetMonsterFill.SetValue(0);
        for (int i = progress; i < monsterFills.Length; i++)
        {
            monsterFills[i].SetValue(0);
            monsterFills[i].Inactivate();
        }
    }

    public void SetFill(float val)
    {
        targetMonsterFill.SetValue(val);

    }
    public void AddFill(float val)
    {
        targetMonsterFill.AddValue(val);
    }

    public void SetCurrencyDungeon(CurrencyDungeonType type)
    {
        BossSetting();
        stageValueText.text = "";
        switch (type)
        {
            case CurrencyDungeonType.Gold: stageText.text = "금화던전"; break;
            case CurrencyDungeonType.Honbaeg: stageText.text = "혼백던전"; break;
            case CurrencyDungeonType.Spirit: stageText.text = "영석던전"; break;
        }
    }

    public void SetHealthBarText(BigCurrency current, BigCurrency Initial)
    {
        healthText.text = $"{current} / {Initial}";
    }


}
