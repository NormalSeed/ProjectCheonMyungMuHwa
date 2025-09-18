
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

    int currentstage;

    [SerializeField] private MonsterModelBaseSO normalOrcModel;
    [SerializeField] private MonsterModelBaseSO bossModel;
    [SerializeField] private RectTransform goldRect;

    [SerializeField] LoadedMonsterSO loadedData;

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
        PunchPool = new DefaultPool<MonsterController>(loadedData.Objects["Punch"], 3, active: false);
        StickPool = new DefaultPool<MonsterController>(loadedData.Objects["Stick"], 3, active: false);
        CanePool = new DefaultPool<MonsterController>(loadedData.Objects["Cane"], 3, active: false);
        BowPool = new DefaultPool<MonsterController>(loadedData.Objects["Bow"], 3, active: false);
        ArrowPool = new DefaultPool<MonsterProjectile>(loadedData.Objects["Arrow"], 8, exceed: true, warmup: false, parent: gameObject.transform);
        MagicPool = new DefaultPool<MonsterProjectile>(loadedData.Objects["MagicBall"], 8, exceed: true, warmup: false, parent: gameObject.transform);
        ItemPool = new DefaultPool<DroppedItem>(loadedData.Objects["DroppedItem"], 60, exceed: true, warmup: false, parent: gameObject.transform);

        GameObject[] loadedbosses = new GameObject[] {
            loadedData.Objects["PunchBoss"],
            loadedData.Objects["StickBoss"],
            loadedData.Objects["CaneBoss"],
            loadedData.Objects["BowBoss"],
            loadedData.Objects["BigBoss"]
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
    //현재 스테이지에 따른 몬스터, 보스 스텟 설정 (1~1200)
    public void SetMonsterState(int stage)
    {
        currentstage = stage;
        normalOrcModel.SetFinal(stage);
        bossModel.SetFinalBoss(stage, normalOrcModel);
    }

    //지정된 위치에 몬스터 소환
    public void SpawnMonster(Vector2 pos, MonsterType type)
    {
        Debug.Log($"<color=green> 소환 시도 </color>");
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
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        int door = currentstage / 3;
        int last = door % 10;
        string str = "";
        switch (last)
        {
            case 0:
                if (door % 100 == 0)
                {
                    str = "100Boss";
                }
                else if (door % 50 == 0)
                {
                    str = "50Boss";
                }
                break;
            case 5: str = "BigBoss"; break;
            case 1: case 6: str = "PunchBoss"; break;
            case 2: case 7: str = "StickBoss"; break;
            case 3: case 8: str = "CaneBoss"; break;
            case 4: case 9: str = "BowBoss"; break;
            default: str = ""; break;
        }
        MonsterController bosscon = bosses[str];
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
