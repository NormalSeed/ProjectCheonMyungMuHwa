using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PoolPresenter : MonoBehaviour
{
    private PoolManager poolManager;

    [Inject]
    public void Construct(PoolManager manager)
    {
        poolManager = manager;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            poolManager.ActiveAll();


        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            poolManager.PunchPool.RealeaseItem();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            poolManager.StickPool.RealeaseItem();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            poolManager.CainPool.RealeaseItem();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            poolManager.BowPool.RealeaseItem();
        }
    }
}
