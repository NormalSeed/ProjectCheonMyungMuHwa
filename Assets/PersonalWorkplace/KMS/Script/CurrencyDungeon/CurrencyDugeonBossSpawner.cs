using UnityEngine;
using System.Collections;
using VContainer;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CurrencyDugeonBossSpawner : MonoBehaviour
{
    [SerializeField] CurrencyDungeonPoint[] bossPoints;
    [SerializeField] CurrencyBossModelBaseSO model;
    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;

    private Dictionary<CurrencyDungeonType, MonsterController> bosses;

    private IObjectResolver container;

    private WaitForSeconds GetItemWfs;
    [Inject]
    public void VCTest(IObjectResolver container)
    {
        this.container = container;
    }

    void Awake()
    {
        bosses = new();
        GameObject[] loadedbosses = Resources.LoadAll<GameObject>("KMS/CurrencyBoss");
        foreach (GameObject go in loadedbosses)
        {
            GameObject boss = container.Instantiate(go);
            MonsterController con = boss.GetComponent<MonsterController>();
            con.OnLifeEnded += a => con.gameObject.SetActive(false);
            boss.SetActive(false);
            switch (go.name)
            {
                case "GoldBoss": bosses.Add(CurrencyDungeonType.Gold, con); break;
                case "HonbaegBoss": bosses.Add(CurrencyDungeonType.Honbaeg, con); break;
                case "SpiritBoss": bosses.Add(CurrencyDungeonType.Spirit, con); break;
            }
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetModel();
            SpawnBoss();
        }
    }

    private void SetModel()
    {
        int level = sceneData.data.Level;
        model.SetCurrencyBossFinalState(level);
    }
    private void SpawnBoss()
    {
        CurrencyDungeonType type = sceneData.type;
        foreach (CurrencyDungeonPoint point in bossPoints)
        {
            if (type == point.Type)
            {
                ActiveBoss(point.Pos, type);
            }
        }
    }
    private void ActiveBoss(Vector2 pos, CurrencyDungeonType type)
    {
        ParticleManager.Instance.GetParticle("Boss_1_Recall", pos);
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        MonsterController bosscon = bosses[type];
        bosscon.transform.position = pos;
        bosscon.Model.BaseModel = model;
        bosscon.gameObject.SetActive(true);
    }

    public void GotoMain()
    {
        SceneManager.LoadSceneAsync("Demo_GameScene");
    }
}
