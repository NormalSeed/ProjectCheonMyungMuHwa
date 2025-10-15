
using UnityEngine;
using System.Collections;
using VContainer;
using UnityEngine.UI;
using VContainer.Unity;
using System.Collections.Generic;
using Unity.VisualScripting;
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public DefaultPool<MonsterController> PunchPool;
    public DefaultPool<MonsterController> StickPool;
    public DefaultPool<MonsterController> CanePool;
    public DefaultPool<MonsterController> BowPool;

    public DefaultPool<MonsterProjectile> ArrowPool;
    public DefaultPool<MonsterProjectile> MagicPool;
    public DefaultPool<DroppedItem> ItemPool;

    int currentDoor;

    [SerializeField] private MonsterModelBaseSO normalOrcModel;
    [SerializeField] private MonsterModelBaseSO bossModel;
    [SerializeField] private RectTransform goldRect;

    [SerializeField] LoadedMonsterSO loadedData;

    private Dictionary<string, MonsterController> bosses;

    private Queue<DroppedItem> droppedItems;

    private IObjectResolver container;
    private MonsterController bosscon;

    private WaitForSeconds GetItemWfs;

    private Camera maincam;

    [SerializeField] Canvas canvas;

    [Inject]
    public void VCTest(IObjectResolver container)
    {
        this.container = container;

    }

    void Awake()
    {
        maincam = Camera.main;
        GetItemWfs = new WaitForSeconds(0.1f);
        Instance = this;
        droppedItems = new();
        bosses = new();
        PunchPool = new DefaultPool<MonsterController>(loadedData.Objects["Punch"], 3, active: false, resolver: container, warmup: false, exceed: true);
        StickPool = new DefaultPool<MonsterController>(loadedData.Objects["Stick"], 3, active: false, resolver: container, warmup: false, exceed: true);
        CanePool = new DefaultPool<MonsterController>(loadedData.Objects["Cane"], 3, active: false, resolver: container, warmup: false, exceed: true);
        BowPool = new DefaultPool<MonsterController>(loadedData.Objects["Bow"], 3, active: false, resolver: container, warmup: false, exceed: true);
        ArrowPool = new DefaultPool<MonsterProjectile>(loadedData.Objects["Arrow"], 8, exceed: true, warmup: false, parent: gameObject.transform);
        MagicPool = new DefaultPool<MonsterProjectile>(loadedData.Objects["MagicBall"], 8, exceed: true, warmup: false, parent: gameObject.transform);
        ItemPool = new DefaultPool<DroppedItem>(loadedData.Objects["DroppedItem"], 60, exceed: true, warmup: false, parent: gameObject.transform);

        GameObject[] loadedbosses = new GameObject[] {
            loadedData.Objects["PunchBoss"],
            loadedData.Objects["StickBoss"],
            loadedData.Objects["CaneBoss"],
            loadedData.Objects["BowBoss"],
            loadedData.Objects["BigBoss"],
            loadedData.Objects["50Boss"],
            loadedData.Objects["100Boss"]
        };
        foreach (GameObject go in loadedbosses)
        {
            GameObject boss = container.Instantiate(go);
            MonsterController con = boss.GetComponent<MonsterController>();
            bosses.Add(go.name, con);
            con.OnLifeEnded += a => con.gameObject.SetActive(false);
            boss.SetActive(false);
        }
    }
    //현재 관문에 따른 몬스터, 보스 스텟 설정 
    public void SetMonsterState(int door)
    {
        currentDoor = door;
        normalOrcModel.SetState(door);
        bossModel.SetBossState(door);
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
        AudioManager.Instance.PlaySound("2. 보스 등장시 사운드");
        int last = currentDoor % 10;
        string str = "";
        switch (last)
        {
            case 0:
                if (currentDoor % 50 == 0)
                {
                    str = "100Boss";
                }
                else
                {
                    str = "BigBoss";
                }
                break;
            case 5:                 
                if (currentDoor % 25 == 0)
                {
                    str = "50Boss";
                }
                else
                {
                    str = "BigBoss";
                }
                break;
            case 1: case 6: str = "PunchBoss"; break;
            case 2: case 7: str = "StickBoss"; break;
            case 3: case 8: str = "CaneBoss"; break;
            case 4: case 9: str = "BowBoss"; break;
            default: str = "PunchBoss"; break;
        }
        bosscon = bosses[str];
        bosscon.transform.position = pos;
        bosscon.Model.BaseModel = bossModel;
        bosscon.gameObject.SetActive(true);
    }
    public void ActiveBoss(int dummy) //컴파일 오류 방지용 오버로딩
    {

    }
    private void ActiveMonster(DefaultPool<MonsterController> pool, Vector2 pos)
    {
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        MonsterController monster = pool.GetItem(pos);
        monster.Model.BaseModel = normalOrcModel;
        monster.gameObject.SetActive(true);
    }

    public void AddItemToList(DroppedItem item)
    {
        droppedItems.Enqueue(item);
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
            //Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, goldRect.position);
            //Vector2 target = maincam.ScreenToWorldPoint(screenPos);
            DroppedItem item = droppedItems.Dequeue();
            Vector2 pos = maincam.WorldToScreenPoint(item.transform.position);
            item.transform.SetParent(canvas.transform);
            item.transform.position = pos;
            item.Release(goldRect.position + Vector3.right * 10);
            yield return GetItemWfs;
        }

    }
    public void ReleaseAll()
    {
        PunchPool.ReleaseAllItes();
        StickPool.ReleaseAllItes();
        CanePool.ReleaseAllItes();
        BowPool.ReleaseAllItes();
        ArrowPool.ReleaseAllItes();
        MagicPool.ReleaseAllItes();
        bosscon?.gameObject.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            ReleaseAll();
        }
    }

}
public enum MonsterType { Punch, Stick, Cane, Bow, Boss }
