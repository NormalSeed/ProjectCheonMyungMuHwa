
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

public class MonsterPool : MonoBehaviour
{
    ObjectPool<MonsterModel> monsterPool;
    GameObject targetObj;
    [SerializeField] string assetID;

    private Stack<MonsterModel> pooledStack = new Stack<MonsterModel>();
    [SerializeField] int maxCount;

    private void Awake()
    {
        targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        monsterPool = new ObjectPool<MonsterModel>(
            createFunc: () => Create(), // 생성 규칙
            actionOnGet: obj => Active(obj),     // 꺼내올 때
            actionOnRelease: obj => Deactive(obj),// 반환할 때
            actionOnDestroy: obj => Destroy(obj.gameObject),        // 풀에서 제거될 때
            defaultCapacity: 1,  // 기본 생성 개수
            maxSize: maxCount          // 최대 개수
        );
        GetAllItems();
        ReleaseAllItems();
    }

    private MonsterModel Create()
    {
        MonsterModel model = Instantiate(targetObj).GetComponent<MonsterModel>();
        return model;
    }
    private void Active(MonsterModel obj)
    {
        //obj.gameObject.SetActive(true);
        pooledStack.Push(obj);
    }
    private void Deactive(MonsterModel obj)
    {
        obj.gameObject.SetActive(false);
        pooledStack.Pop();
    }

    // 총알 요청
    public MonsterModel GetMonster()
    {
        if (pooledStack.Count >= maxCount) return null;
        return monsterPool.Get();
    }

    public void GetAllItems()
    {
        while (GetMonster() != null) ;
    }

    // 총알 반환
    public void ReleaseItem(MonsterModel pooled)
    {
        if (pooledStack.Count <= 0) return;
        monsterPool.Release(pooled);
    }

    public void ReleaseItem()
    {
        if (pooledStack.Count <= 0) return;
        monsterPool.Release(pooledStack.Peek());
    }
    public void ReleaseAllItems()
    {
        for (int i = 0; i < maxCount; i++)
        {
            ReleaseItem();
        }
    }
}
