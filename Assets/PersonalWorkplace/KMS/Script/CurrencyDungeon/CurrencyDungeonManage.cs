using UnityEngine;

public class CurrencyDungeonManage : MonoBehaviour
{
    [SerializeField] CurrencyDugeonBossSpawner bossSpawner;
    [SerializeField] CurrencyDungeonPlayerSet playerSet;

    [SerializeField] CurrencyDungeonTimer timer;

    [SerializeField] UIBase failUI;


    void Awake()
    {
        bossSpawner.InitBoss(() => timer.Stop());
        playerSet.InitPlayer();

        timer.OnTimeOver += DisableBoss;
        timer.OnTimeOver += failUI.SetShow;
    }

    void Start()
    {
        bossSpawner.SpawnBoss();
        playerSet.SpawnPlayer();
    }

    private void DisableBoss()
    {
        bossSpawner.SpawnedBoss.SetActive(false);
    }

}
