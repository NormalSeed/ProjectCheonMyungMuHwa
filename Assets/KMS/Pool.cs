using System.Diagnostics;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class Pool : MonoBehaviour
{
    ObjectPool<GameObject> objPool;
    GameObject targetObj;

    [SerializeField] Transform[] points;
    [SerializeField] string assetID;

    private Stack<GameObject> pooledStack = new Stack<GameObject>();

    int index;
    int maxCount => points.Length;

    private void Awake()
    {
        targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        objPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(targetObj), // 생성 규칙
            actionOnGet: obj => Active(obj),     // 꺼내올 때
            actionOnRelease: obj => Deactive(obj),// 반환할 때
            actionOnDestroy: obj => Destroy(obj),        // 풀에서 제거될 때
            defaultCapacity: 1,  // 기본 생성 개수
            maxSize: maxCount          // 최대 개수
        );
    }
    private void Active(GameObject obj)
    {
        obj.SetActive(true);
        pooledStack.Push(obj);
        obj.transform.position = points[index].position;
        index++;
    }
    private void Deactive(GameObject obj)
    {
        obj.SetActive(false);
        pooledStack.Pop();
        index--;
    }

    // 총알 요청
    public GameObject GetItem()
    {
        if (pooledStack.Count >= maxCount) return null;
        return objPool.Get();
    }

    public void ActiveAllItems()
    {
        while (GetItem() != null) ;
    }

    // 총알 반환
    public void ReleaseItem(GameObject pooled)
    {
        if (pooledStack.Count <= 0) return;
        objPool.Release(pooled);
    }

    public void RealeaseItem()
    {
        if (pooledStack.Count <= 0) return;
        objPool.Release(pooledStack.Peek());
    }
}
