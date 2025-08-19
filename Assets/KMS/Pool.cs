using System.Diagnostics;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class Pool : MonoBehaviour
{
    ObjectPool<GameObject> pool;

    private void Awake()
    {
        GameObject pooled = Addressables.LoadAssetAsync<GameObject>("Pooled").WaitForCompletion();
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(pooled, transform), // 생성 규칙
            actionOnGet: obj => obj.SetActive(true),     // 꺼내올 때
            actionOnRelease: obj => obj.SetActive(false),// 반환할 때
            actionOnDestroy: obj => Destroy(obj),        // 풀에서 제거될 때
            defaultCapacity: 10,  // 기본 생성 개수
            maxSize: 100          // 최대 개수
        );
    }

    // 총알 요청
    public GameObject GetItem()
    {
        return pool.Get();
    }

    // 총알 반환
    public void ReleaseItem(GameObject pooled)
    {
        pool.Release(pooled);
    }
}
