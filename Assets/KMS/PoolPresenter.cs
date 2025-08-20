using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PoolPresenter : MonoBehaviour
{
    private Pool pool;

    private Stack<GameObject> pooledStack = new Stack<GameObject>();

    [Inject]
    public void Construct(Pool pool)
    {
        this.pool = pool;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            pooledStack.Push(pool.GetItem());

        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            pool.ReleaseItem(pooledStack.Pop());

        }
    }
}
