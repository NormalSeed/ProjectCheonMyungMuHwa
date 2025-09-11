using UnityEngine;

public class CurrencyDungeonManage : MonoBehaviour
{
    [SerializeField] CurrencyDugeonBossSpawner bossSpawner;
    [SerializeField] CurrencyDungeonPlayerSet playerSet;


    void Awake()
    {
        bossSpawner.InitBoss();
        playerSet.InitPlayer();
    }

    void Start()
    {
        bossSpawner.SpawnBoss();
        playerSet.SpawnPlayer();
    }

    public void DisableBoss()
    {
        bossSpawner.SpawnedBoss.SetActive(false);
    }

}
