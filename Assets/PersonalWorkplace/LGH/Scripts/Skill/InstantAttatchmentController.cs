using System.Collections.Generic;
using UnityEngine;

public class InstantAttatchmentController : MonoBehaviour
{
    [SerializeField] private GameObject[] instantAttatchmentPrefabs;
    public Dictionary<string, LGH_ObjectPool> attatchmentPools;

    public void Init()
    {
        attatchmentPools = new Dictionary<string, LGH_ObjectPool>();

        foreach (var prefab in instantAttatchmentPrefabs)
        {
            PooledObject po = prefab.GetComponent<PooledObject>();
            var pool = new LGH_ObjectPool(transform, po, 5);
            attatchmentPools.Add(prefab.name, pool);
        }
    }

    public InstantAttatchment SpawnAttatchment(string attatchmentID, Transform target)
    {
        if (!attatchmentPools.ContainsKey(attatchmentID))
        {
            Debug.LogWarning($"Attatchment ID '{attatchmentID}' not found");
        }

        var pooledObj = attatchmentPools[attatchmentID].PopPool();
        pooledObj.transform.SetParent(target);
        pooledObj.transform.position = target.position;

        var attatchment = pooledObj as InstantAttatchment;
        if (attatchment == null)
        {
            Debug.LogError("Attatchment 캐스팅 실패");
            return null;
        }

        return attatchment;
    }
}
