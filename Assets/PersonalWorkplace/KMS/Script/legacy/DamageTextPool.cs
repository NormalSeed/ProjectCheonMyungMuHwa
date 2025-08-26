using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class DamageTextPool : MonoBehaviour
{
    ObjectPool<DamageText> projPool;
    GameObject targetObj;
    [SerializeField] string assetID;

    private void Awake()
    {
        targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        projPool = new ObjectPool<DamageText>(
            createFunc: () => Create(), // 생성 규칙
            actionOnGet: obj => Active(obj),     // 꺼내올 때
            actionOnRelease: obj => Deactive(obj),// 반환할 때
            actionOnDestroy: obj => Destroy(obj.gameObject),        // 풀에서 제거될 때
            defaultCapacity: 1,  // 기본 생성 개수
            maxSize: 100         // 최대 개수
        );
        InitObjs(20);
    }

    private DamageText Create()
    {
        DamageText model = Instantiate(targetObj).GetComponent<DamageText>();
        //model.OnDisable += ReleaseItem;

        return model;
    }
    private void Active(DamageText obj)
    {
    }
    private void Deactive(DamageText obj)
    {
        obj.gameObject.SetActive(false);
    }

    public DamageText GetProj()
    {
        return projPool.Get();
    }
    public void ReleaseItem(DamageText pooled)
    {
        projPool.Release(pooled);
    }

    private void InitObjs(int count)
    {
        List<DamageText> list = new();
        for (int i = 0; i < count; i++)
        {
            list.Add(GetProj());
        }
        foreach (DamageText proj in list)
        {
            ReleaseItem(proj);
        }
    }
}
