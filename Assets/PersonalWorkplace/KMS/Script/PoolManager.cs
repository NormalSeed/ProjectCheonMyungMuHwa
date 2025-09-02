
using UnityEngine;
using System.Collections;
using VContainer;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public DefaultPool<MonsterController> PunchPool;
    public DefaultPool<MonsterController> StickPool;
    public DefaultPool<MonsterController> CainPool;
    public DefaultPool<MonsterController> BowPool;

    public DefaultPool<MonsterProjectile> ArrowPool;
    public DefaultPool<MonsterProjectile> MagicPool;

    public DefaultPool<DamageText> DamagePool;

    public DefaultPool<DroppedItem> ItemPool;

    [SerializeField] private Transform[] spawwnPoints;
    [SerializeField] private Transform bossPoint;

    int currentstage;

    [SerializeField] private MonsterModelBaseSO[] models;
    [SerializeField] private MonsterModelBaseSO bossModel;

    [SerializeField] private RectTransform mainCanvasRect;
    [SerializeField] private RectTransform goldRect;

    private Dictionary<string, MonsterController> bosses;

    private Stack<DroppedItem> droppedItems;

    private IObjectResolver container;

    private WaitForSeconds GetItemWfs;
    [Inject]
    public void VCTest(IObjectResolver container)
    {
        this.container = container;

    }

    void Awake()
    {
        GetItemWfs = new WaitForSeconds(0.05f);
        Instance = this;
        droppedItems = new();
        bosses = new();
        PunchPool = new DefaultPool<MonsterController>("Punch", 3, false);
        StickPool = new DefaultPool<MonsterController>("Stick", 3, false);
        CainPool = new DefaultPool<MonsterController>("Cane", 3, false);
        BowPool = new DefaultPool<MonsterController>("Bow", 3, false);
        ArrowPool = new DefaultPool<MonsterProjectile>("Arrow", 8);
        MagicPool = new DefaultPool<MonsterProjectile>("MagicBall", 8);
        DamagePool = new DefaultPool<DamageText>("DamageText", 15);
        ItemPool = new DefaultPool<DroppedItem>("DroppedItem", 60);

        GameObject[] loadedbosses = Resources.LoadAll<GameObject>("KMS/Boss");
        foreach (GameObject go in loadedbosses)
        {
            GameObject boss = container.Instantiate(go);
            MonsterController con = boss.GetComponent<MonsterController>();
            bosses.Add(go.name, con);
            con.OnLifeEnded += a => con.gameObject.SetActive(false);
            boss.SetActive(false);
        }
    }


    public void ActiveAll(int stageNum)
    {
        currentstage = stageNum;
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        SetModelStates();

        ActiveMonster(PunchPool, 0, 0);
        ActiveMonster(PunchPool, 1, 0);
        ActiveMonster(PunchPool, 2, 0);
        ActiveMonster(StickPool, 3, 1);
        ActiveMonster(StickPool, 4, 1);
        ActiveMonster(StickPool, 5, 1);
        ActiveMonster(CainPool, 6, 2);
        ActiveMonster(CainPool, 7, 2);
        ActiveMonster(CainPool, 8, 2);
        ActiveMonster(BowPool, 9, 3);
        ActiveMonster(BowPool, 10, 3);
        ActiveMonster(BowPool, 11, 3);
    }

    public void ActiveBoss(int stageNum) //3의 배수로만 들어오는 것을 상정함
    {
        currentstage = stageNum;
        int last = currentstage / 3 % 10;
        string str;
        switch (last)
        {
            case 0: case 5: str = "LastBoss"; break;
            case 1: case 6: str = "PunchBoss"; break;
            case 2: case 7: str = "StickBoss"; break;
            case 3: case 8: str = "CaneBoss"; break;
            case 4: case 9: str = "BowBoss"; break;
            default: str = ""; break;

        }
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        bossModel.SetFinalBoss(currentstage, models[0]);

        MonsterController bosscon = bosses[str];
        bosscon.transform.position = bossPoint.position;
        bosscon.Model.BaseModel = bossModel;
        bosscon.gameObject.SetActive(true);

    }
    private void ActiveMonster(DefaultPool<MonsterController> pool, int pointIndex, int modelIndex)
    {
        if (currentstage < 601)
        {
            ParticleManager.Instance.GetParticle("M_12_Recall", spawwnPoints[pointIndex].position);
        }
        else
        {
            ParticleManager.Instance.GetParticle("M_34_Recall", spawwnPoints[pointIndex].position);
        }

        MonsterController monster = pool.GetItem(spawwnPoints[pointIndex].position);
        monster.Model.InitSprite(currentstage);
        monster.Model.BaseModel = models[modelIndex];
        monster.gameObject.SetActive(true);
    }
    private void SetModelStates()
    {
        foreach (MonsterModelBaseSO model in models)
        {
            model.SetFinal(currentstage);
        }
    }

    public void AddItemToList(DroppedItem item)
    {
        droppedItems.Push(item);
    }

    public void GetItems()
    {
        StartCoroutine(GetitemRoutine());
    }
    private IEnumerator GetitemRoutine()
    {
        yield return GetItemWfs;
        while (droppedItems.Count > 0)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, goldRect.position);
            Vector2 target = Camera.main.ScreenToWorldPoint(screenPos);
            droppedItems.Pop().Release(target);
            yield return GetItemWfs;
        }

    }

    public void MoveTo(Vector2 pos)
    {
        transform.position = pos;
    }



}
