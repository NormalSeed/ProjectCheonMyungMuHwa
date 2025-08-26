using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class ParticlePool : MonoBehaviour
{
    ObjectPool<ParticleSystem> projPool;
    GameObject targetObj;
    [SerializeField] string assetID;

    private void Awake()
    {
        targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        projPool = new ObjectPool<ParticleSystem>(
            createFunc: () => Create(), // 생성 규칙
            actionOnGet: obj => { },     // 꺼내올 때
            actionOnRelease: obj => Deactive(obj),// 반환할 때
            actionOnDestroy: obj => Destroy(obj.gameObject),        // 풀에서 제거될 때
            defaultCapacity: 1,  // 기본 생성 개수
            maxSize: 100         // 최대 개수
        );
        InitObjs(12);
    }

    private ParticleSystem Create()
    {
        ParticleSystem model = Instantiate(targetObj).GetComponent<ParticleSystem>();
        return model;
    }
    private void Deactive(ParticleSystem obj)
    {
        obj.gameObject.SetActive(false);
    }

    public ParticleSystem GetProj()
    {
        return projPool.Get();
    }
    public void ReleaseItem(ParticleSystem pooled)
    {
        projPool.Release(pooled);
    }

    private void InitObjs(int count)
    {
        List<ParticleSystem> list = new();
        for (int i = 0; i < count; i++)
        {
            list.Add(GetProj());
        }
        foreach (ParticleSystem proj in list)
        {
            ReleaseItem(proj);
        }
    }
}
