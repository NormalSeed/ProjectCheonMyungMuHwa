using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    ObjectPool<MonsterProjectile> projPool;
    GameObject targetObj;
    [SerializeField] string assetID;

    private void Awake()
    {
        targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        projPool = new ObjectPool<MonsterProjectile>(
            createFunc: () => Create(), // 생성 규칙
            actionOnGet: obj => Active(obj),     // 꺼내올 때
            actionOnRelease: obj => Deactive(obj),// 반환할 때
            actionOnDestroy: obj => Destroy(obj.gameObject),        // 풀에서 제거될 때
            defaultCapacity: 1,  // 기본 생성 개수
            maxSize: 100         // 최대 개수
        );
        InitObjs(6);
    }

    private MonsterProjectile Create()
    {
        MonsterProjectile model = Instantiate(targetObj).GetComponent<MonsterProjectile>();

        return model;
    }
    private void Active(MonsterProjectile obj)
    {
    }
    private void Deactive(MonsterProjectile obj)
    {
        obj.gameObject.SetActive(false);
    }

    public MonsterProjectile GetProj()
    {
        return projPool.Get();
    }
    public void ReleaseItem(MonsterProjectile pooled)
    {
        projPool.Release(pooled);
    }

    private void InitObjs(int count)
    {
        List<MonsterProjectile> list = new();
        for (int i = 0; i < count; i++)
        {
            list.Add(GetProj());
        }
        foreach (MonsterProjectile proj in list)
        {
            ReleaseItem(proj);
        }
    }
}
