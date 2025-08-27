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
        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    //poolManager.ActiveAll();
        //}
        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    //poolManager.ActiveBoss();
        //}
    }
}
