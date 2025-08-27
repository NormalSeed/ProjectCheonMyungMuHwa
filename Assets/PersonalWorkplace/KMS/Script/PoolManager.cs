
using UnityEngine;
using System.Collections;

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

    public DefaultPool<MonsterController> bossPool;

    public ParticlePool SpawnPartPool;

    [SerializeField] private Transform[] spawwnPoints;
    [SerializeField] private Transform bossPoint;

    int currentstage = 5;

    [SerializeField] private MonsterModelBaseSO[] models;
    [SerializeField] private MonsterModelBaseSO bossModel;

    void Awake()
    {
        Instance = this;
        PunchPool = new DefaultPool<MonsterController>("Punch", 3, false);
        StickPool = new DefaultPool<MonsterController>("Stick", 3, false);
        CainPool = new DefaultPool<MonsterController>("Cane", 3, false);
        BowPool = new DefaultPool<MonsterController>("Bow", 3, false);
        ArrowPool = new DefaultPool<MonsterProjectile>("Arrow", 8);
        MagicPool = new DefaultPool<MonsterProjectile>("MagicBall", 8);
        DamagePool = new DefaultPool<DamageText>("DamageText", 15);
        ItemPool = new DefaultPool<DroppedItem>("DroppedItem", 100);

        //test
        bossPool = new DefaultPool<MonsterController>("Boss", 10, false);
    }


    public void ActiveAll(int stageNum)
    {
        AudioManager.Instance.PlaySound("Monster_Recall_New");
        SetModelStates(stageNum);

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

        ActiveBoss(stageNum);
    }

    public void ActiveBoss(int stageNum)
    {
        SetModelStates(stageNum);
        bossModel.SetFinalBoss(stageNum, models[0]);
        MonsterController boss = bossPool.GetItem(bossPoint.position);
        boss.Model.BaseModel = bossModel;
        boss.gameObject.SetActive(true);

    }
    private void ActiveMonster(DefaultPool<MonsterController> pool, int pointIndex, int modelIndex)
    {
        ParticleSystem pt = SpawnPartPool.GetProj();
        pt.transform.position = spawwnPoints[pointIndex].position;
        pt.gameObject.SetActive(true);
        StartCoroutine(WaitForParticleEnd(pt));

        MonsterController monster = pool.GetItem(spawwnPoints[pointIndex].position);
        monster.Model.InitSprite(currentstage);
        monster.Model.BaseModel = models[modelIndex];
        monster.gameObject.SetActive(true);
    }
    private void SetModelStates(int currentstage)
    {
        foreach (MonsterModelBaseSO model in models)
        {
            model.SetFinal(currentstage);
        }
    }

    IEnumerator WaitForParticleEnd(ParticleSystem pt)
    {
        pt.Play();
        yield return new WaitForSeconds(pt.main.duration);
        SpawnPartPool.ReleaseItem(pt);
    }



}
