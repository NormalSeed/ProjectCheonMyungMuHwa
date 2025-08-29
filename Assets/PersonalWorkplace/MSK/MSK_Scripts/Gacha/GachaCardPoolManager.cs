using System.Collections.Generic;
using UnityEngine;

public class GachaCardPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private int poolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(cardPrefab, parentTransform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }
    public GameObject GetCard()
    {
        if (pool.Count == 0)
        {
            var oldest = parentTransform.GetChild(0).gameObject;
            oldest.SetActive(false);
            pool.Enqueue(oldest);
        }
        return pool.Dequeue();
    }

    public void ReturnAll()
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            var go = parentTransform.GetChild(i).gameObject;
            if (go.activeSelf)
            {
                go.SetActive(false);
                pool.Enqueue(go);
            }
        }
    }
}
