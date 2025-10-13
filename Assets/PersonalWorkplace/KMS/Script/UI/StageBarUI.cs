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

    public StageBarFill TargetMonsterFill;
    public StageBarFill TargetBossFill;


    void Start()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnStageStart += SetStage;
            InGameManager.Instance.OnStageStart += SpawnStagePanel;
        }
        timer.OnTimeOver += KillAllPlayer;
    }

    private void SetStage(int door, int progress) // 수정 : 0 1 2에서 몬스터 3에서 보스
    {
        stageValueText.text = door.ToString();
        if (progress < InGameManager.MAX_PROGRESS)
        {
            MonsterSetting(progress);
        }
        else
        {
            BossSetting();
        }
    }
    private void SpawnStagePanel(int door, int progress)
    {
        if (progress == 0) PopupManager.Instance.ShowStagePopup(door);
    }

    private void BossSetting()
    {
        timer.Activate();
        healthText.gameObject.SetActive(true);
        foreach (var f in monsterFills)
        {
            f.Inactivate();
        }
        TargetBossFill = bossFill;
        TargetBossFill.Activate();
        TargetBossFill.SetValue(1);
        TargetMonsterFill = null;
    }
    private void MonsterSetting(int progress) //0 1 2
    {
        timer.Inactivate();
        healthText.gameObject.SetActive(false);
        bossFill.Inactivate();
        TargetMonsterFill = monsterFills[progress];
        TargetBossFill = null;
        for (int i = 0; i < monsterFills.Length; i++)
        {
            if (i < progress)
            {
                monsterFills[i].Activate();
                monsterFills[i].SetValue(1);

            }
            else if (i == progress)
            {
                monsterFills[i].Activate();
                monsterFills[i].SetValue(0);
            }
            else
            {
                monsterFills[i].SetValue(0);
                monsterFills[i].Inactivate();
            }

        }
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

    private void KillAllPlayer()
    {
        foreach (PlayerController p in PartyManager.Instance.players)
        {
            if (!p.isDead.Value) p.TakeDamage(float.MaxValue);
        }
    }


}
