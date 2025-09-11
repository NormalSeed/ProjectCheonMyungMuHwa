using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public interface IPooled<T>
{
    public Action<IPooled<T>> OnLifeEnded { get; set; }
}

// 1. 어드레서블 등록된 프리팹에 IPooled<T>를 상속한 MonoBehavior 클래스가 있다면 풀링.
// 2. 해당 클래스에서 엑션을 통해 조건이 되면 자동 반환
// 3. 미리 정해둔 개수 만큼 생성 후 비활성화
// 4. 오브젝트를 받아올 때 활성화 하지 않도록 설정 가능. 이 경우 받아온 측에서 직접 활성화 (OnEnable에서 초기화 시 타이밍 잡는 용도)
public class DefaultPool<T> where T : MonoBehaviour, IPooled<T>
{
    private ObjectPool<IPooled<T>> pool;
    private GameObject targetObj;
    private string assetID;
    private int maxCount;
    private bool activeOnGet;
    private bool canExceedMaxCount;
    private bool useWarmUp;
    private Transform parent;

    //private List<IPooled<T>> pooledItems;

    public DefaultPool(string id, int max, bool active = true, bool exceed = false, bool warmup = true, Transform parent = null)
    {
        assetID = id;
        //targetObj = Addressables.LoadAssetAsync<GameObject>(assetID).WaitForCompletion();
        targetObj = Resources.Load<GameObject>($"KMS/{assetID}");
        maxCount = max;
        activeOnGet = active;
        canExceedMaxCount = exceed;
        useWarmUp = warmup;
        this.parent = parent;
        Init();
    }
    public DefaultPool(GameObject obj, int maxCount, bool active = true, bool exceed = false, bool warmup = true, Transform parent = null)
    {
        targetObj = obj;
        this.maxCount = maxCount;
        activeOnGet = active;
        canExceedMaxCount = exceed;
        useWarmUp = warmup;
        this.parent = parent;
        Init();
    }

    private void Init()
    {
        //pooledItems = new();
        pool = new ObjectPool<IPooled<T>>(
            createFunc: () => Create(),
            actionOnGet: obj => Active(obj),
            actionOnRelease: obj => Deactive(obj),
            actionOnDestroy: obj => UnityEngine.Object.Destroy((obj as MonoBehaviour).gameObject),
            defaultCapacity: maxCount,
            maxSize: maxCount
        );
        WarmUp();
    }

    private IPooled<T> Create()
    {
        IPooled<T> obj = UnityEngine.Object.Instantiate(targetObj).GetComponent<IPooled<T>>();
        (obj as MonoBehaviour).transform.parent = parent;
        obj.OnLifeEnded = null;
        obj.OnLifeEnded += ReleaseItem;
        if (!activeOnGet)
        {
            (obj as MonoBehaviour).gameObject.SetActive(false);
        }

        return obj;
    }
    private void Active(IPooled<T> obj)
    {
        if (activeOnGet)
        {
            (obj as MonoBehaviour).gameObject.SetActive(true);
        }
    }
    private void Deactive(IPooled<T> obj)
    {
        (obj as MonoBehaviour).gameObject.SetActive(false);
    }
    private void WarmUp()
    {
        if (!useWarmUp) return;
        List<IPooled<T>> list = new();
        for (int i = 0; i < maxCount; i++)
        {
            list.Add(pool.Get());
        }
        foreach (IPooled<T> proj in list)
        {
            pool.Release(proj);
        }
    }

    public IPooled<T> GetPooledItem()
    {
        if (!canExceedMaxCount && pool.CountActive >= maxCount)
        {
            Debug.LogError($"<color=red>최대 개수 초과{pool.CountActive}</color>");
            return null;
        }
        //pooledItems.Add(pooled);
        return pool.Get();
    }

    public void ReleaseAllItes()
    {
        //for (int i = pooledItems.Count - 1; i >= 0; i--)
        //{
        //    ReleaseItem(pooledItems[i]);
        //}
    }

    public T GetItem()
    {
        return GetPooledItem() as T;
    }
    public T GetItem(Vector2 pos)
    {
        T obj = GetPooledItem() as T;
        if (obj != null)
        {
            obj.transform.position = pos;
        }
        return obj;
    }
    public void ReleaseItem(IPooled<T> pooled)
    {
        if (pool.CountActive <= 0) return;
        //pooledItems.Remove(pooled);
        pool.Release(pooled);
    }
}
