
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
    public DefaultPool<MonsterController> CanePool;
    public DefaultPool<MonsterController> BowPool;

    public DefaultPool<MonsterProjectile> ArrowPool;
    public DefaultPool<MonsterProjectile> MagicPool;

    public DefaultPool<DamageText> DamagePool;

    public DefaultPool<DroppedItem> ItemPool;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform bossPoint;

    int currentstage;

    [SerializeField] private MonsterModelBaseSO normalOrcModel;
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
        GetItemWfs = new WaitForSeconds(0.03f);
        Instance = this;
        droppedItems = new();
        bosses = new();
        PunchPool = new DefaultPool<MonsterController>("Punch", 3, false);
        StickPool = new DefaultPool<MonsterController>("Stick", 3, false);
        CanePool = new DefaultPool<MonsterController>("Cane", 3, false);
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
    //현재 스테이지에 따른 몬스터, 보스 스텟 설정 (1~1200)
    public void SetMonsterState(int stage)
    {
        currentstage = stage;
        normalOrcModel.SetFinal(stage);
        bossModel.SetFinalBoss(currentstage, normalOrcModel);
    }

    //지정된 위치에 몬스터 소환
    public void SpawnMonster(Vector2 pos, MonsterType type)
    {
        if (type == MonsterType.Punch)
        {
            ActiveMonster(PunchPool, pos);
        }
        else if (type == MonsterType.Stick)
        {
            ActiveMonster(StickPool, pos);
        }
        else if (type == MonsterType.Cane)
        {
            ActiveMonster(CanePool, pos);
        }
        else if (type == MonsterType.Bow)
        {
            ActiveMonster(BowPool, pos);
        }
        else if (type == MonsterType.Boss)
        {
            ActiveBoss(pos);
        }
    }

    public void ActiveAll(int stageNum) // 기존에 사용하던 몬스터 소환 함수 (사용 X)
    {

    }

    private void ActiveBoss(Vector2 pos)
    {
        ParticleManager.Instance.GetParticle("Boss_1_Recall", pos);
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        int last = currentstage / 3 % 10;
        string str;
        switch (last)
        {
            case 0: case 5: str = "BigBoss"; break;
            case 1: case 6: str = "SpiritBoss"; break;
            case 2: case 7: str = "StickBoss"; break;
            case 3: case 8: str = "CaneBoss"; break;
            case 4: case 9: str = "BowBoss"; break;
            default: str = ""; break;

        }
        MonsterController bosscon = bosses[str];
        bosscon.transform.position = pos;
        bosscon.Model.InitSprite(currentstage);
        bosscon.Model.BaseModel = bossModel;
        bosscon.gameObject.SetActive(true);
    }
    public void ActiveBoss(int dummy) //컴파일 오류 방지용 오버로딩
    {

    }
    private void ActiveMonster(DefaultPool<MonsterController> pool, Vector2 pos)
    {
        ParticleManager.Instance.GetParticle("M_12_Recall", pos);
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        MonsterController monster = pool.GetItem(pos);
        monster.Model.InitSprite(currentstage);
        monster.Model.BaseModel = normalOrcModel;
        monster.gameObject.SetActive(true);
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

}
public enum MonsterType { Punch, Stick, Cane, Bow, Boss }
