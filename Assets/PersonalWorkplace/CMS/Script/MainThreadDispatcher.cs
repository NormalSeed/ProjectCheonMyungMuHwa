using UnityEngine;
using System;
using System.Collections.Generic;
public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance = null;
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    public static MainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<MainThreadDispatcher>();

            if (instance == null)
            {
                GameObject obj = new GameObject("MainThreadDispatcher");
                instance = obj.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }
        return instance;
    }
    /// <summary>
    /// 메인 스레드에서 실행할 액션 등록
    /// </summary>
    public void Enqueue(Action action)
    {
        if (action == null)
            return;

        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }
}
