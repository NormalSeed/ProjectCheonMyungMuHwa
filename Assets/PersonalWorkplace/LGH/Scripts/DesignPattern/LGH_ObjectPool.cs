using System.Collections.Generic;
using UnityEngine;

public class LGH_ObjectPool
{
    private Stack<PooledObject> pool;
    private PooledObject prefab;
    private GameObject poolObject;

    private List<PooledObject> activeObjects = new List<PooledObject>();

    public LGH_ObjectPool(Transform parent, PooledObject targetPrefab, int initSize)
    {
        Init(parent, targetPrefab, initSize);
        GameEvents.OnSceneLoaded += ReturnAllActiveObjects;
    }

    private void Init(Transform parent, PooledObject targetPrefab, int initSize)
    {
        pool = new Stack<PooledObject>(initSize);
        prefab = targetPrefab;
        poolObject = new GameObject($"{targetPrefab.name}.pool");
        poolObject.transform.parent = parent;

        for (int i = 0; i < initSize; i++)
        {
            CreatePooledObject();
        }
    }

    public void PushPool(PooledObject target)
    {
        target.transform.parent = poolObject.transform;
        target.gameObject.SetActive(false);
        activeObjects.Remove(target);
        pool.Push(target);
    }

    public PooledObject PopPool()
    {
        if (pool.Count == 0) CreatePooledObject();

        PooledObject obj = pool.Pop();
        //obj.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    private void CreatePooledObject()
    {
        GameObject objGO = MonoBehaviour.Instantiate(prefab.gameObject); // GameObject 기준으로 복제
        PooledObject obj = objGO.GetComponent<PooledObject>();           // 다시 컴포넌트 꺼내기
        obj.PooledInit(this);
        PushPool(obj);
    }

    public void ReturnAllActiveObjects()
    {
        foreach (var obj in activeObjects.ToArray()) // 복사본으로 루프
        {
            obj.ReturnPool();
        }
    }

    ~LGH_ObjectPool()
    {
        GameEvents.OnSceneLoaded -= ReturnAllActiveObjects;
    }
}
