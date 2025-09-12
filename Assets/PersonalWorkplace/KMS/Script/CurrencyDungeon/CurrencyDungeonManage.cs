using UnityEngine;

public class CurrencyDungeonManage : MonoBehaviour
{
    [SerializeField] CurrencyDugeonBossSpawner bossSpawner;
    [SerializeField] CurrencyDungeonPlayerSet playerSet;

    [SerializeField] CurrencyDungeonTimer timer;

    [SerializeField] UIBase failUI;
    [SerializeField] UIBase clearUI;


    void Awake()
    {
        bossSpawner.InitBoss(DungeonClear);
        playerSet.InitPlayer();

        timer.OnTimeOver += DungeonFail;
    }

    void Start()
    {
        bossSpawner.SpawnBoss();
        playerSet.SpawnPlayer();
    }

    private void DungeonClear()
    {
        timer.Stop();
        clearUI.SetShow();
    }

    private void DungeonFail()
    {
        bossSpawner.SpawnedBoss.SetActive(false);
        failUI.SetShow();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            DungeonClear();
        }

    }

}
